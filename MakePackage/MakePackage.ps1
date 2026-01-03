# パッケージ生成スクリプト
#
# 使用ツール：
#   - Wix Toolset
#   - pandoc

Param(
	[ValidateSet("All", "Zip", "Installer", "Appx", "Alpha", "Beta", "Dev")]$Target = "All",

	# ビルドをスキップする
	[switch]$continue,

	# ログ出力のあるパッケージ作成
	[switch]$trace,

	# ビルドバージョンは更新しない
	[switch]$noVersionUp
)

# error to break
trap { break }

$ErrorActionPreference = "stop"

Write-Host
Write-Host "[Properties] ..." -fore Cyan
Write-Host "Target: $Target"
Write-Host "Continue: $continue"
Write-Host "Trace: $trace"
Write-Host
Read-Host "Press Enter to continue"

# environment
$product = 'NeeView'
$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64"
$issuesUrl = "https://github.com/neelabo/NeeView/issues"

# sync current directory
[System.IO.Directory]::SetCurrentDirectory((Get-Location -PSProvider FileSystem).Path)

# get file version
function Get-FileVersion($fileName) {
	throw "not supported."

	$major = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMajorPart
	$minor = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMinorPart

	"$major.$minor"
}

# get base version from Version.xml
function Get-BaseVersion($versionXml) {
	$xml = [xml](Get-Content $versionXml)
	
	$version = [String]$xml.Version
	
	if ($version -match '\d+\.\d+') {
		return $version
	}
	
	throw "Cannot get base version."
}

# get version from _Version.props
function Get-Version($projectFile) {
	$xml = [xml](Get-Content $projectFile)
	
	$version = [String]$xml.Project.PropertyGroup.VersionPrefix
	
	if ($version -match '\d+\.\d+\.\d+') {
		return $version
	}
	
	throw "Cannot get version."
}

# create display version (MajorVersion.MinorVersion) from raw version
function Get-AppVersion($version) {
	$tokens = $version.Split(".")
	if ($tokens.Length -ne 3) {
		throw "Wrong version format."
	}
	$tokens = $version.Split(".")
	$majorVersion = [int]$tokens[0]
	$minorVersion = [int]$tokens[1]
	return "$majorVersion.$minorVersion"
}

# get git log
function Get-GitLog {
	param (
		[string]$IssuesUrl
	)

	$branch = Invoke-Expression "git rev-parse --abbrev-ref HEAD"
	$descrive = Invoke-Expression "git tag" | Where-Object { $_ -match "^\d+\.\d+$" } | Select-Object -Last 1
	$date = Invoke-Expression 'git log -1 --pretty=format:"%ad" --date=iso'
	$result = Invoke-Expression "git log $descrive..head --encoding=$([Console]::OutputEncoding.WebName) --pretty=format:`"%s`""
	$result = $result | Where-Object { -not ($_ -match '^Merge |^chore:|^docs:|^refactor:|^-|^\.\.') } 
	#$result = $result | Select-Object -Unique
	if ($IssuesUrl -ne "") {
		$result = $result | ForEach-Object { $_ -replace "#(\d+)", "[#`$1]($IssuesUrl/`$1)" }
	}

	return "[${branch}] $descrive to head", $date, $result
}

# get git log (markdown)
function Get-GitLogMarkdown {
	param (
		[string]$Title,
		[string]$IssuesUrl
	)

	$result = Get-GitLog $IssuesUrl
	$header = $result[0]
	$date = $result[1]
	$logs = $result[2]

	"## $Title"
	"### $header"
	"Rev. $revision / $date"
	""
	$logs | ForEach-Object { "- $_" }
	""
	"This change log was generated automatically."
}

# replace keyword
function Replace-Content {
	Param([string]$filepath, [string]$rep1, [string]$rep2)
	if ($(Test-Path $filepath) -ne $True) {
		Write-Error "file not found"
		return
	}
	if ($rep1 -eq "@HEAD") {
		$file_contents = $(Get-Content -Encoding UTF8 $filepath)
		$file_contents = @($rep2) + $file_contents
	}
	else {
		$file_contents = $(Get-Content -Encoding UTF8 $filepath) -replace $rep1, $rep2
	}
	$file_contents | Out-File -Encoding UTF8 $filepath
}

# string to PascalCase
function ConvertTo-PascalCase {
	[OutputType('System.String')]
	param(
		[Parameter(Position = 0)]
		[string] $Value
	)

	# https://devblogs.microsoft.com/oldnewthing/20190909-00/?p=102844
	return [regex]::replace($Value.ToLower(), '(^|_)(.)', { $args[0].Groups[2].Value.ToUpper() })
}

# Replace HTML blockquote alerts
# Blockquote alerts like GitHub
# NOTE: 限定された構造のHTMLのみ対応しているので汎用性はない
function Replace-Alert([string]$filepath) {
	if ($(Test-Path $filepath) -ne $True) {
		Write-Error "file not found"
		return
	}
	$file_contents = $(Get-Content -Encoding UTF8 $filepath)

	$blockquoteSection = $false
	$blockquoteStartIndex = 0
	for ($i = 0; $i -lt $file_contents.Count; $i++) {
		$line = $file_contents[$i]
		if ($blockquoteSection) {
			$pattern = "\[!([a-zA-Z]+)\](.*)$"
			if ($line -match $pattern) {
				$name = $Matches[1].ToLower()
				$rest = $Matches[2]
				$file_contents[$blockquoteStartIndex] = "<blockquote class=""alert $name"">"
				$title = ConvertTo-PascalCase $name
				if (($rest -eq "</p>") -or ($rest -eq "<br />")) {
					$rest = ""
				}
				$file_contents[$i] = $line -replace $pattern, "<span class=""alert $name"">$title</span></p><p>$rest"
				$blockquoteSection = $false
			}
			elseif ($line -match "</blockquote>") {
				$blockquoteSection = $false
			}
		}
		else {
			if ($line -eq "<blockquote>") {
				$blockquoteSection = $true
				$blockquoteStartIndex = $i
			}
		}
	}

	$file_contents | Out-File -Encoding UTF8 $filepath
}

# build
function Get-DefaultOptions($platform)
{
	$defaultOptions = @(
		"-p:PublishProfile=FolderProfile-$platform.pubxml"
		"-c", "Release"
	)

	switch ($Target) {
		"Dev" { $defaultOptions += "-p:VersionSuffix=dev.${dateVersion}"; break; }
		"Alpha" { $defaultOptions += "-p:VersionSuffix=alpha.${packageAlphaVersion}"; break; }
		"Beta" { $defaultOptions += "-p:VersionSuffix=beta.${packageBetaVersion}"; break; }
	}

	return $defaultOptions
}

function Build-Project($platform, $outputDir, $options) {
	$defaultOptions = Get-DefaultOptions $platform

	Write-Host "> dotnet publish $project $defaultOptions $options -o Publish\$outputDir`n" -fore Cyan

	& dotnet publish $project $defaultOptions $options -o Publish\$outputDir
	if ($? -ne $true) {
		throw "build error"
	}
}

function Build-SusieProject($platform, $outputDir) 
{
	$defaultOptions = Get-DefaultOptions $platform

	& dotnet publish $projectSusie $defaultOptions -o Publish\$outputDir\Libraries\Susie
	if ($? -ne $true) {
		throw "build error"
	}
}

function Build-ProjectSelfContained($platform) {
	$options = @(
		"-p:SelfContained=true"
	)
	Build-Project $platform "$product-$platform" $options
	Build-SusieProject $platform "$product-$platform"
}

function Build-ProjectFrameworkDependent($platform) {
	$options = @(
		"-p:SelfContained=false"
	)

	Build-Project $platform "$product-$platform-fd" $options
	Build-SusieProject $platform "$product-$platform-fd"
}

# package section
function New-Package($platform, $productName, $productDir, $packageDir, $fd) {
	New-Item $packageDir -ItemType Directory > $null

	Copy-Item $productDir\* $packageDir -Recurse -Exclude ("*.pdb", "$product.settings.json")

	# fix native dll
	if ($platform -eq "x86") {
		#Write-Host Remove native x64
		Remove-Item $packageDir\Libraries\x64 -Recurse
	}
	if ($platform -eq "x64") {
		#Write-Host Remove native x86
		Remove-Item $packageDir\Libraries\x86 -Recurse
	}

	# custom config
	New-ConfigForZip $productDir "$productName.settings.json" $packageDir

	# generate README.html
	$target = $fd ? "Zip-fd" : "Zip"
	New-Readme $packageDir "en-us" $target
	New-Readme $packageDir "ja-jp" $target
}

function Edit-Markdown {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory = $True, ValueFromPipeline = $True)]
		[object[]]$Value,
		[switch]$IncrementDepth,
		[switch]$DecrementDepth,
		[switch]$ChopTitle
	)

	begin {
		$chop = $false
	}

	process {
		foreach ($line in $Value) {
			if ($line.StartsWith("#")) {
				if ($ChopTitle -and !$chop) {
					$chop = $true
				}
				elseif ($IncrementDepth) {
					"#" + $line
				}
				elseif ($DecrementDepth) {
					$line.Remove(0, 1)
				}
				else {
					$line
				}
			}
			else {
				$line
			}
		}
	}
}

function Get-MarkdownSection {

	[CmdletBinding()]
	param (
		[Parameter(Mandatory = $True, ValueFromPipeline = $True)]
		[object[]]$Content,
		[string]$Section
	)
	begin {
		$phase = 0
	}
	process {
		foreach ($line in $Content) {
			if ($phase -eq 0) {
				if ($line -match "<!--\s*section:\s*([^\s].+[^\s])\s*-->") {
					$name = $Matches[1]
					if ($name -eq $Section) {
						$phase = 1
					}
					else {
					}
				}
			}
			elseif ($phase -eq 1) {
				if ($line -match "<!--\s*end_section[^a-zA-Z0-9_]") {
					$phase = 2
				}
				else {
					Write-Output $line
				}
			}
		}
	}
}

# generate README.html
function New-Readme {
	param ([string]$packageDir, [string]$culture, [string]$target) 
	
	$readmeSource = "$solutionDir\docs\$culture"

	$postfix = $appVersion
	if ($target.StartsWith("Alpha")) {
		$postfix = "$appVersion-Alpha.${packageAlphaVersion}"
	}
	elseif ($target.StartsWith("Beta")) {
		$postfix = "$appVersion-Beta.${packageBetaVersion}"
	}

	$source = @()

	$rev = "Rev. ${revision}"
	if ($target.StartsWith("Alpha")) {
		$source += Get-Content "$readmeSource\package-alpha.md" | Edit-Markdown -IncrementDepth | ForEach-Object { $_.replace("<custom-revision/>", $rev) }
	}
	elseif ($target.StartsWith("Beta")) {
		$source += Get-Content "$readmeSource\package-beta.md" | Edit-Markdown -IncrementDepth | ForEach-Object { $_.replace("<custom-revision/>", $rev) }
	}

	$overviewContent = Get-Content "$readmeSource\index.md" | Get-MarkdownSection -Section "overview"
	$source += @("")
	$source += $overviewContent
	
	$source += @("")
	if (($target -eq "Zip") -or ($target -eq "Alpha") -or ($target -eq "Beta")) {
		$source += Get-Content "$readmeSource\package-zip.md" | Edit-Markdown -IncrementDepth
	}
	elseif (($target -eq "Zip-fd") -or ($target -eq "Alpha-fd") -or ($target -eq "Beta-fd")) {
		$source += Get-Content "$readmeSource\package-zip-fd.md" | Edit-Markdown -IncrementDepth
	}
	elseif ($target -eq "Msi") {
		$source += Get-Content "$readmeSource\package-installer.md" | Edit-Markdown -IncrementDepth
	}
	elseif ($target -eq "Appx") {
		$source += Get-Content "$readmeSource\package-storeapp.md" | Edit-Markdown -IncrementDepth
	}

	$source += @("")
	if ($culture -eq "ja-jp") {
		$source += @("## ポータル サイト")
		$source += @("- [NeeView Portal Site](https://neelabo.github.io/NeeView)")
	}
	else {
		$source += @("## Portal Site")
		$source += @("- [NeeView Portal Site](https://neelabo.github.io/NeeView)")
	}

	$contactContent = Get-Content "$readmeSource\contact.md" | Edit-Markdown -IncrementDepth
	$source += @("")
	$source += $contactContent

	$licenseContent = Get-Content "$solutionDir\LICENSE.md"
	$licenseContent = @("## License") + $licenseContent
	if (Test-Path "$readmeSource\license-appendix.md") {
		$licenseAppendixContent = Get-Content "$readmeSource\license-appendix.md"
		$licenseContent += @("")
		$licenseContent += $licenseAppendixContent
	}
	$source += @("")
	$source += $licenseContent

	$thirdPartyLicenseContent = Get-Content "$solutionDir\THIRDPARTY_LICENSES.md"
	$thirdPartyLicenseContent += @("")
	$thirdPartyLicenseContent += Get-Content "$solutionDir\NeeLaboratory.IO.Search\THIRDPARTY_LICENSES.md"
	$source += @("")
	$source += $thirdPartyLicenseContent

	if ($target.StartsWith("Alpha")) {
		$changeLogContent = Get-GitLogMarkdown -Title "$product $postfix - Changelog"  -IssuesUrl $issuesUrl
	}
	else {
		$changeLogContent = .\SelectChangelog.ps1 -Path "$readmeSource\changelog.md" -Culture $culture
	}
	$source += @("")
	$source += $changeLogContent


	if (-not ($culture -eq "en-us")) {
		$readmeHtml = "README.$culture.html"
	}
	else {
		$readmeHtml = "README.html"
	}

	$output = "$packageDir\$readmeHtml"
	$css = "Style.html"

	# markdown to html by pandoc
	$source | pandoc -s -t html5 -o $output --metadata title="$product $postfix" -H $css
	if ($? -ne $true) {
		throw "pandoc error"
	}

	Replace-Alert $output
}

# remove ZIP
function Remove-Zip($packageZip) {
	if (Test-Path $packageZip) {
		Remove-Item $packageZip
	}
}

# archive to ZIP
function New-Zip($referenceDir, $packageDir, $packageZip) {
	Copy-Item $referenceDir $packageDir -Recurse
	Optimize-Package $packageDir
	Compress-Archive $packageDir -DestinationPath $packageZip
}

# make config for zip
function New-ConfigForZip($inputDir, $config, $outputDir) {
	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = "Zip"
	$jsonObject.SelfContained = Test-Path("$inputDir\hostfxr.dll")
	$jsonObject.Watermark = $false
	$jsonObject.UseLocalApplicationData = $false
	$jsonObject.Revision = $revision
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}

# make config for installer
function New-ConfigForMsi($inputDir, $config, $outputDir) {
	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = "Msi"
	$jsonObject.SelfContained = $true
	$jsonObject.Watermark = $false
	$jsonObject.UseLocalApplicationData = $true
	$jsonObject.Revision = $revision
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}

# make config for appx
function New-ConfigForAppx($inputDir, $config, $outputDir) {
	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = "Appx"
	$jsonObject.SelfContained = $true
	$jsonObject.Watermark = $false
	$jsonObject.UseLocalApplicationData = $true
	$jsonObject.Revision = $revision
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}

# make config for alpha / beta
function New-ConfigForDevPackage($inputDir, $config, $target, $outputDir) {
	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = "Zip"
	$jsonObject.SelfContained = Test-Path("$inputDir\hostfxr.dll")
	$jsonObject.Watermark = $true
	$jsonObject.UseLocalApplicationData = $false
	$jsonObject.Revision = $revision
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}

function New-EmptyFolder($dir) {
	# remove folder
	if (Test-Path $dir) {
		Remove-Item $dir -Recurse
		Start-Sleep -m 100
	}

	# make folder
	New-Item $dir -ItemType Directory > $null
}

function New-PackageAppend($packageDir, $packageAppendDir) {
	New-EmptyFolder $packageAppendDir

	# configure customize
	New-ConfigForMsi $packageDir "${product}.settings.json" $packageAppendDir

	# generate README.html
	New-Readme $packageAppendDir "en-us" "Msi"
	New-Readme $packageAppendDir "ja-jp" "Msi"

	# icons
	Copy-Item "$projectDir\Resources\App.ico" $packageAppendDir
}

# remove Msi
function Remove-Msi($packageAppendDir, $packageMsi) {
	if (Test-Path $packageMsi) {
		Remove-Item $packageMsi
	}

	if (Test-Path $packageAppxDir_x64) {
		Remove-Item $packageAppxDir_x64 -Recurse
	}
}

# Msi
function New-Msi($arch, $packageDir, $packageAppendDir, $packageMsi) {

    # Require Wix toolset (e.g.; version 5.0.2)
    # > dotnet tool install --global wix -version 5.02
    #
    # Use WiX UI Extension
    # > wix extension add --global WixToolset.UI.wixext/5.0.2

    $wisubstg = "$Win10SDK\wisubstg.vbs"
    $wilangid = "$Win10SDK\wilangid.vbs"

    function New-MsiSub($arch, $culture, $packageMsi) {

        $properties = @(
            "-d", "ProductName=NeeView $appVersion"
            "-d", "ProductVersion=$version",
            "-b", "Contents=$(Convert-Path $packageDir)",
            "-b", "Append=$(Convert-Path $packageAppendDir)",
            "-ext", "WixToolset.UI.wixext"
        )

        Write-Host "[wix] build $packageMsi -culture $culture" -fore Cyan 
        & wix.exe build -arch $arch -out $packageMsi -culture $culture @properties WixSource\*.wx?
        if ($? -ne $true) {
            throw "wix build error"
        }
    }

    $1033Msi = "$packageAppendDir\1033.msi"
    $1041Msi = "$packageAppendDir\1041.msi"
    $1041Mst = "$packageAppendDir\1041.mst"

    New-MsiSub $arch "en-us" $1033Msi
    New-MsiSub $arch "ja-jp" $1041Msi 

    Write-Host "[wix] msi transform $1041Mst" -fore Cyan
    & wix.exe msi transform -p -t language $1033Msi $1041Msi -out $1041Mst
    if ($? -ne $true) {
        throw "wix msi transform error"
    }

    Copy-Item $1033Msi $packageMsi

    Write-Host "[msi] wisubstg $packageMsi" -fore Cyan
    & cscript "$wisubstg" "$packageMsi" $1041Mst 1041
    if ($? -ne $true) {
        throw "$wisubstg error"
    }

    Write-Host "[msi] wilangid $packageMsi" -fore Cyan
    & cscript "$wilangid" "$packageMsi" Package 1033, 1041
    if ($? -ne $true) {
        throw "$wilangid error"
    }
}

# Appx remove
function Remove-Appx($packageAppendDir, $appx) {
	if (Test-Path $appx) {
		Remove-Item $appx
	}

	if (Test-Path $packageAppxDir_x64) {
		Remove-Item $packageAppxDir_x64 -Recurse
	}
}

# Appx 
function New-Appx($arch, $packageDir, $packageAppendDir, $appx) {
	$packgaeFilesDir = "$packageAppendDir/PackageFiles"
	$contentDir = "$packgaeFilesDir/$product"

	# copy package base files
	Copy-Item "Appx\Resources" $packgaeFilesDir -Recurse -Force

	# copy resources.pri
	Copy-Item "Appx\resources.pri" $packgaeFilesDir

	# update assembly
	Copy-Item $packageDir $contentDir -Recurse -Force
	New-ConfigForAppx $packageDir "${product}.settings.json" $contentDir

	# generate README.html
	New-Readme $contentDir "en-us" "Appx"
	New-Readme $contentDir "ja-jp" "Appx"

	$param = Get-Content -Raw $env:CersPath/_$product.Parameter.json | ConvertFrom-Json
	$appxName = $param.name
	$appxPublisher = $param.publisher

	# generate AppManifest
	$content = Get-Content "Appx\AppxManifest.xml"
	$content = $content -replace "%NAME%", "$appxName"
	$content = $content -replace "%PUBLISHER%", "$appxPublisher"
	$content = $content -replace "%VERSION%", "$assemblyVersion"
	$content = $content -replace "%ARCH%", "$arch"
	$content | Out-File -Encoding UTF8 "$packgaeFilesDir\AppxManifest.xml"

	# re-package
	& "$Win10SDK\makeappx.exe" pack /l /d "$packgaeFilesDir" /p "$appx"
	if ($? -ne $true) {
		throw "makeappx.exe error"
	}

	# signing
	& "$Win10SDK\signtool.exe" sign -f "$env:CersPath/_$product.pfx" -fd SHA256 -v "$appx"
	if ($? -ne $true) {
		throw "signtool.exe error"
	}
}

function New-AppxSym($publishDir, $appxSym) {
	$files = Get-ChildItem $publishDir -File -Filter *.pdb
	Compress-Archive -Path $files -DestinationPath $appxSym
}

function New-AppxUpload($arch, $publishDir, $packageDir, $packageAppendDir, $name) {
	$appx = "$name.msix"
	$appxSym = "$name.appxsym"
	$appxUpload = "$name.msixupload"
	New-Appx $arch $packageDir $packageAppendDir $appx
	New-AppxSym $publishDir $appxSym
	Compress-Archive -Path $appx, $appxSym -DestinationPath $appxupload
}

# archive to Alpha.ZIP
function Remove-Alpha($packageDir, $packageZip) {
	if (Test-Path $packageZip) {
		Remove-Item $packageZip
	}

	if (Test-Path $packageDir) {
		Remove-Item $packageDir -Recurse
	}
}

function New-Alpha($referenceDir, $packageDir, $packageZip, $fd) {
	New-DevPackage $referenceDir $packageDir $packageZip "Alpha" $fd
}

# archive to Beta.ZIP
function Remove-Beta($packageDir, $packageZip) {
	if (Test-Path $packageZip) {
		Remove-Item $packageZip
	}

	if (Test-Path $packageDir) {
		Remove-Item $packageDir -Recurse
	}
}

function New-Beta($referenceDir, $packageDir, $packageZip, $fd) {
	New-DevPackage $referenceDir $packageDir $packageZip "Beta" $fd
}

# archive to Alpha/Beta.ZIP
function New-DevPackage($packageDir, $devPackageDir, $devPackage, $target, $fd) {
	# update assembly
	Copy-Item $packageDir $devPackageDir -Recurse
	New-ConfigForDevPackage $packageDir "${product}.settings.json" $target $devPackageDir

	# generate README.html
	$targetName = $fd ? "$target-fd" : $target
	New-Readme $devPackageDir "en-us" $targetName
	New-Readme $devPackageDir "ja-jp" $targetName

	Optimize-Package $devPackageDir
	Compress-Archive $devPackageDir -DestinationPath $devPackage
}

# Optimizing file placement with NetBeauty
# https://github.com/nulastudio/NetBeauty2
function Optimize-Package($packageDir) {
	Write-Host "NetBeauty2" -fore Cyan

	& nbeauty2 --usepatch --loglevel Detail $packageDir Libraries
	if ($? -ne $true) {
		throw "nbeauty2 error"
	}

	$unusedFile = "$packageDir\hostfxr.dll.bak"
	if (Test-Path $unusedFile) {
		Remove-Item $unusedFile
	}
}

# remove build objects
function Remove-BuildObjects {
	Get-ChildItem -Directory "$packagePrefix*" | Remove-Item -Recurse

	Get-ChildItem -File "$packagePrefix*.*" | Remove-Item

	if (Test-Path $publishDir) {
		Remove-Item $publishDir -Recurse
	}
	if (Test-Path $packageAlphaDir) {
		Remove-Item $packageAlphaDir -Recurse -Force
	}
	if (Test-Path $packageAlphaDir_fd) {
		Remove-Item $packageAlphaDir_fd -Recurse -Force
	}
	if (Test-Path $packageBetaDir) {
		Remove-Item $packageBetaDir -Recurse -Force
	}
	if (Test-Path $packageBetaDir_fd) {
		Remove-Item $packageBetaDir_fd -Recurse -Force
	}
	if (Test-Path $packageAlphaWild) {
		Remove-Item $packageAlphaWild
	}
	if (Test-Path $packageBetaWild) {
		Remove-Item $packageBetaWild
	}
	if (Test-Path $packageAppxWild) {
		Remove-Item $packageAppxWild
	}
	if (Test-Path $packageMsixWild) {
		Remove-Item $packageMsixWild
	}

	Start-Sleep -m 100
}

function Build-Clear {
	# clear
	Write-Host "`n[Clear] ...`n" -fore Cyan
	Remove-BuildObjects
}

function Build-UpdateState {
	$global:build_x64 = Test-Path $publishDir_x64
	$global:build_x64_fd = Test-Path $publishDir_x64_fd
}

function Build-PackageSource-x64 {
	if ($global:build_x64 -eq $true) { return }

	# build
	Write-Host "`n[Build] ...`n" -fore Cyan
	Build-ProjectSelfContained "x64"
	
	# create package source
	Write-Host "`n[Package] ...`n" -fore Cyan
	New-Package "x64" $product $publishDir_x64 $packageDir_x64 $false

	$global:build_x64 = $true
}

function Build-PackageSource-x64-fd {
	if ($global:build_x64_fd -eq $true) { return }

	# build
	Write-Host "`n[Build framework dependent] ...`n" -fore Cyan
	Build-ProjectFrameworkDependent "x64"
	
	# create package source
	Write-Host "`n[Package framework dependent] ...`n" -fore Cyan
	New-Package "x64" $product $publishDir_x64_fd $packageDir_x64_fd $true

	$global:build_x64_fd = $true
}

function Build-Zip-x64 {
	Write-Host "`[Zip] ...`n" -fore Cyan

	Remove-Zip $packageZip_x64
	New-Zip $packageDir_x64 $packageName_x64 $packageZip_x64
	Write-Host "`nExport $packageZip_x64 succeeded.`n" -fore Green
}

function Build-Zip-x64-fd {
	Write-Host "`[Zip fd] ...`n" -fore Cyan

	Remove-Zip $packageZip_x64_fd
	New-Zip $packageDir_x64_fd $packageName_x64_fd $packageZip_x64_fd
	Write-Host "`nExport $packageZip_x64_fd succeeded.`n" -fore Green
}

function Build-Installer-x64 {
	Write-Host "`n[Installer] ...`n" -fore Cyan
	
	Remove-Msi $packageAppendDir_x64 $packageMsi_x64
	New-PackageAppend $packageDir_x64 $packageAppendDir_x64
	New-Msi "x64" $packageDir_x64 $packageAppendDir_x64 $packageMsi_x64
	Write-Host "`nExport $packageMsi_x64 succeeded.`n" -fore Green
}

function Build-Appx-x64 {
	Write-Host "`n[Appx] ...`n" -fore Cyan

	if (Test-Path "$env:CersPath\_Parameter.ps1") {
		Remove-Appx $packageAppxDir_x64 $packageX64Appx
		#New-Appx "x64" $packageDir_x64 $packageAppxDir_x64 $packageX64Appx
		New-AppxUpload "x64" $publishDir_x64 $packageDir_x64 $packageAppxDir_x64 $packageName_x64
		Write-Host "`nExport $packageX64Appx succeeded.`n" -fore Green
	}
	else {
		Write-Host "`nWarning: not exist make appx environment. skip!`n" -fore Yellow
	}
}

function Build-Alpha {
	Write-Host "`n[Alpha] ...`n" -fore Cyan
	Remove-Alpha $packageAlphaDir $packageAlpha
	New-Alpha $packageDir_x64 $packageAlphaDir $packageAlpha $false
	Write-Host "`nExport $packageAlpha succeeded.`n" -fore Green
}

function Build-Alpha-fd {
	Write-Host "`n[Alpha fd] ...`n" -fore Cyan
	Remove-Alpha $packageAlphaDir_fd $packageAlpha_fd
	New-Alpha $packageDir_x64_fd $packageAlphaDir_fd $packageAlpha_fd $true
	Write-Host "`nExport $packageAlpha_fd succeeded.`n" -fore Green
}

function Build-Beta {
	Write-Host "`n[Beta] ...`n" -fore Cyan
	Remove-Beta $packageBetaDir $packageBeta
	New-Beta $packageDir_x64 $packageBetaDir $packageBeta $false
	Write-Host "`nExport $packageBeta succeeded.`n" -fore Green
}

function Build-Beta-fd {
	Write-Host "`n[Beta fd] ...`n" -fore Cyan
	Remove-Beta $packageBetaDir_fd $packageBeta_fd
	New-Beta $packageDir_x64_fd $packageBetaDir_fd $packageBeta_fd $true
	Write-Host "`nExport $packageBeta_fd succeeded.`n" -fore Green
}

function Export-Current {
	Write-Host "`n[Current] ...`n" -fore Cyan
	if (Test-Path $packageDir_x64_fd) {
		if (-not (Test-Path $product)) {
			New-Item $product -ItemType Directory
		}
		Copy-Item "$packageDir_x64_fd\*" "$product\" -Recurse -Force
	}
	else {
		Write-Host "`nWarning: not exist $packageDir_x64_fd. skip!`n" -fore Yellow
	}
}

function Update-Version($baseVersion) {
	
	Write-Host "`n`[Update NeeLaboratory.IO.Search Version] ...`n" -fore Cyan
	..\NeeLaboratory.IO.Search\CreateVersionProps.ps1
	
	Write-Host "`n`[Update NeeView Version] ...`n" -fore Cyan
	$versionSuffix = switch ( $Target ) {
		"Dev" { "dev.$dateVersion" }
		"Alpha" { "alpha.$packageAlphaVersion" }
		"Beta" { "beta.$packageBetaVersion" }
		default { "" }
	}
	..\CreateVersionProps.ps1 -baseVersion $baseVersion -suffix $versionSuffix
}

function Get-LatestPreReleaseVersion($version, $preRelease) {

	$regexVersion = [Regex]::Escape($version)
	$tagPreRelease = $preRelease.ToLower()
	
	$tag = git tag | Where-Object { $_ -match "^$regexVersion-$tagPreRelease\." } | Select-Object -Last 1

	#Write-Host "Get latest $preRelease version tag: $tag" 

	if ($tag -match "$tagPreRelease\.(\d+)$") {
		return [int]$Matches[1]
	}
	else {
		return 0
	}
}


#======================
# main
#======================

# variables
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Convert-Path "$scriptPath\.."
$solution = "$solutionDir\$product.sln"
$projectDir = "$solutionDir\$product"
$project = "$projectDir\$product.csproj"
$projectSusieDir = "$solutionDir\$product.Susie.Server"
$projectSusie = "$projectSusieDir\$product.Susie.Server.csproj"
$versionProps = "$solutionDir\_Version.props"

# build completion flags
$build_x64 = $false
$build_x64_fd = $false

# versions
$revision = (& git rev-parse --short HEAD).ToString()

$baseVersion = Get-BaseVersion ..\NeeViewVersion.xml
$dateVersion = (Get-Date).ToString("yyMMdd") 
$packageAlphaVersion = (Get-LatestPreReleaseVersion $baseVersion "Alpha") + 1
$packageBetaVersion = (Get-LatestPreReleaseVersion $baseVersion "Beta") + 1
Update-Version $baseVersion

$version = Get-Version $versionProps
$appVersion = Get-AppVersion $version
$assemblyVersion = "$version.0"

$publishDir = "Publish"
$publishDir_x64 = "$publishDir\$product-x64"
$publishDir_x64_fd = "$publishDir\$product-x64-fd"
$packagePrefix = "$product$appVersion"
$packageDir_x64 = "$product$appVersion-x64"
$packageDir_x64_fd = "$product$appVersion-x64-fd"
$packageAppendDir_x64 = "$packageDir_x64.append"
$packageName_x64 = "${product}${appVersion}"
$packageName_x64_fd = "${product}${appVersion}-fd"
$packageZip_x64 = "$packageName_x64.zip"
$packageZip_x64_fd = "$packageName_x64_fd.zip"
$packageMsi_x64 = "$packageName_x64.msi"
$packageAppxDir_x64 = "${product}${appVersion}-appx-x64"
$packageX64Appx = "${product}${appVersion}.msix"
$packageAppxWild = "${product}${appVersion}.appx*"
$packageMsixWild = "${product}${appVersion}.msix*"

$packageNameAlpha = "${product}${appVersion}-Alpha.${packageAlphaVersion}"
$packageAlphaDir = "$packageNameAlpha"
$packageAlphaDir_fd = "$packageNameAlpha-fd"
$packageAlpha = "$packageNameAlpha.zip"
$packageAlpha_fd = "$packageNameAlpha-fd.zip"
$packageAlphaWild = "${product}${appVersion}-Alpha*.zip"

$packageNameBeta = "${product}${appVersion}-Beta.${packageBetaVersion}"
$packageBetaDir = "$packageNameBeta"
$packageBetaDir_fd = "$packageNameBeta-fd"
$packageBeta = "$packageNameBeta.zip"
$packageBeta_fd = "$packageNameBeta-fd.zip"
$packageBetaWild = "${product}${appVersion}-Beta*.zip"

if (-not $continue) {
	Build-Clear
}

Build-UpdateState

if (($Target -eq "All") -or ($Target -eq "Zip")) {
	Build-PackageSource-x64
	Build-Zip-x64
	Build-PackageSource-x64-fd
	Build-Zip-x64-fd
}

if (($Target -eq "All") -or ($Target -eq "Installer")) {
	Build-PackageSource-x64
	Build-Installer-x64
}

if (($Target -eq "All") -or ($Target -eq "Appx")) {
	Build-PackageSource-x64
	Build-Appx-x64
}

if ($Target -eq "Alpha") {
	Build-PackageSource-x64
	Build-Alpha
	Build-PackageSource-x64-fd
	Build-Alpha-fd
}

if ($Target -eq "Beta") {
	Build-PackageSource-x64
	Build-Beta
	Build-PackageSource-x64-fd
	Build-Beta-fd
}

if (-not $continue) {
	Build-PackageSource-x64-fd
	Export-Current
}

# Finish.
Write-Host "`nBuild $version All done.`n" -fore Green





