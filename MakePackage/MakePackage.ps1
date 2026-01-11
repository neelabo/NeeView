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
function Get-FileVersion {
	param ([string]$fileName) 

	throw "not supported."

	$major = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMajorPart
	$minor = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMinorPart

	"$major.$minor"
}

# get base version from Version.xml
function Get-BaseVersion {
	param ([string]$versionXml) 

	$xml = [xml](Get-Content $versionXml)
	
	$version = [String]$xml.Version
	
	if ($version -match '\d+\.\d+') {
		return $version
	}
	
	throw "Cannot get base version."
}

# get version from _Version.props
function Get-Version {
	param ([string]$projectFile)

	$xml = [xml](Get-Content $projectFile)
	
	$version = [String]$xml.Project.PropertyGroup.VersionPrefix
	
	if ($version -match '\d+\.\d+\.\d+') {
		return $version
	}
	
	throw "Cannot get version."
}

# create display version (MajorVersion.MinorVersion) from raw version
function Get-AppVersion {
	param ([string]$version)

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
	param(
		[string]$filepath,
		[string]$rep1,
		[string]$rep2
	)

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
function Replace-Alert {
	param ([string]$filepath) 

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
function Get-DefaultOptions {
	$defaultOptions = @(
		"-p:PublishProfile=FolderProfile-x64.pubxml"
		"-c", "Release"
	)

	switch ($Target) {
		"Dev" { $defaultOptions += "-p:VersionSuffix=dev.${dateVersion}"; break; }
		"Alpha" { $defaultOptions += "-p:VersionSuffix=alpha.${packageAlphaVersion}"; break; }
		"Beta" { $defaultOptions += "-p:VersionSuffix=beta.${packageBetaVersion}"; break; }
	}

	return $defaultOptions
}

function Build-Project {
	param (
		[string]$outputDir,
		[string]$options
	)

	$defaultOptions = Get-DefaultOptions

	Write-Host "> dotnet publish $project $defaultOptions $options -o $outputDir`n" -fore Cyan

	& dotnet publish $project $defaultOptions $options -o $outputDir
	if ($? -ne $true) {
		throw "build error"
	}
}

function Build-SusieProject {
	param (
		[string]$outputDir
	)

	$defaultOptions = Get-DefaultOptions

	& dotnet publish $projectSusie $defaultOptions -o $outputDir\Libraries\Susie
	if ($? -ne $true) {
		throw "build error"
	}
}

function Build-ProjectSelfContained {
	$options = @(
		"-p:SelfContained=true"
	)

	Build-Project  $publishDir $options
	Build-SusieProject $publishDir
}

function Build-ProjectFrameworkDependent {
	$options = @(
		"-p:SelfContained=false"
	)

	Build-Project  $publishDir_fd $options
	Build-SusieProject $publishDir_fd
}

# package section
function New-Package {
	param (
		[string]$productName,
		[string]$productDir,
		[string]$packageDir,
		[bool]$fd
	)

	New-Item $packageDir -ItemType Directory > $null

	Copy-Item $productDir\* $packageDir -Recurse -Exclude ("*.pdb", "$product.settings.json")

	#Write-Host Remove native x86
	Remove-Item $packageDir\Libraries\x86 -Recurse

	# custom config
	New-ConfigForZip $productDir "$productName.settings.json" $packageDir

	# generate README.html
	$target = $fd ? "Zip-fd" : "Zip"
	New-Readme $packageDir "en-us" $target "Stable"
	New-Readme $packageDir "ja-jp" $target "Stable"
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

function New-AppVersionName {
	param ([string]$appVersion, [string]$stage)

	if ($stage -eq "Alpha") {
		return "$appVersion-Alpha.${packageAlphaVersion}"
	}
	elseif ($stage -eq "Beta") {
		return "$appVersion-Beta.${packageBetaVersion}"
	}
	else {
		return $appVersion
	}
}

# generate README.html
function New-Readme {
	param (
		[string]$packageDir,
	 	[string]$culture,
		[string]$target,
		[string]$stage
	) 
	
	$readmeSource = "$solutionDir\docs\$culture"

	$postfix = New-AppVersionName $appVersion $stage

	$source = @()

	$rev = "Rev. ${revision}"
	if ($stage -eq "Alpha") {
		$source += Get-Content "$readmeSource\package-alpha.md" | Edit-Markdown -IncrementDepth | ForEach-Object { $_.replace("<custom-revision/>", $rev) }
	}
	elseif ($stage -eq "Beta") {
		$source += Get-Content "$readmeSource\package-beta.md" | Edit-Markdown -IncrementDepth | ForEach-Object { $_.replace("<custom-revision/>", $rev) }
	}

	$overviewContent = Get-Content "$readmeSource\index.md" | Get-MarkdownSection -Section "overview"
	$source += @("")
	$source += $overviewContent
	
	$source += @("")
	if ($target -eq "Zip") {
		$source += Get-Content "$readmeSource\package-zip.md" | Edit-Markdown -IncrementDepth
	}
	elseif ($target -eq "Zip-fd") {
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

	if ($stage -eq "Alpha") {
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

# archive to ZIP
function New-Zip {
	param (
		[string]$referenceDir,
		[string]$packageDir,
		[string]$packageZip,
		[string]$target,
		[string]$stage
	)

	Copy-Item $referenceDir $packageDir -Recurse

	if ($stage -ne "Stable") {
		New-ConfigForDevPackage $referenceDir "${product}.settings.json" $stage $packageDir

		New-Readme $packageDir "en-us" $target $stage
		New-Readme $packageDir "ja-jp" $target $stage
	}

	Optimize-Package $packageDir
	Compress-Archive $packageDir -DestinationPath $packageZip
}

# make config for zip
function New-ConfigForZip {
	param (
		[string]$inputDir,
		[string]$config,
		[string]$outputDir
	) 
	
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
function New-ConfigForMsi {
	param (
		[string]$inputDir,
		[string]$config,
		[string]$outputDir,
		[string]$stage
	)

	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = "Msi"
	$jsonObject.SelfContained = $true
	$jsonObject.Watermark = $stage -ne "Stable"
	$jsonObject.UseLocalApplicationData = $true
	$jsonObject.Revision = $revision
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}

# make config for appx
function New-ConfigForAppx {
	param (
		[string]$inputDir,
		[string]$config,
		[string]$outputDir
	)

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
function New-ConfigForDevPackage {
	param (
		[string]$inputDir,
		[string]$config,
		[string]$target,
		[string]$outputDir
	)
	
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

function New-EmptyFolder {
	param ([string]$dir) 

	# remove folder
	if (Test-Path $dir) {
		Remove-Item $dir -Recurse
		Start-Sleep -m 100
	}

	# make folder
	New-Item $dir -ItemType Directory > $null
}

function New-PackageAppend {
	param (
		[string]$referenceDir,
		[string]$packageAppendDir,
		[string]$stage
	)

	New-EmptyFolder $packageAppendDir

	# configure customize
	New-ConfigForMsi $referenceDir "${product}.settings.json" $packageAppendDir $stage

	# generate README.html
	New-Readme $packageAppendDir "en-us" "Msi" $stage
	New-Readme $packageAppendDir "ja-jp" "Msi" $stage

	# icons
	Copy-Item "$projectDir\Resources\App.ico" $packageAppendDir
}

# Msi
function New-Msi {
	param (
		[string]$referenceDir,
		[string]$packageAppendDir,
		[string]$packageMsi,
		[string]$stage
	)

    # Require Wix toolset (e.g.; version 5.0.2)
    # > dotnet tool install --global wix -version 5.02
    #
    # Use WiX UI Extension
    # > wix extension add --global WixToolset.UI.wixext/5.0.2

    $wisubstg = "$Win10SDK\wisubstg.vbs"
    $wilangid = "$Win10SDK\wilangid.vbs"

    function New-MsiSub($culture, $packageMsi, $stage) {

        $properties = @(
            "-d", "ProductName=NeeView $(New-AppVersionName $appVersion $stage)"
            "-d", "ProductVersion=$version",
            "-b", "Contents=$(Convert-Path $referenceDir)",
            "-b", "Append=$(Convert-Path $packageAppendDir)",
            "-ext", "WixToolset.UI.wixext"
        )

        Write-Host "[wix] build $packageMsi -culture $culture" -fore Cyan 
        & wix.exe build -arch x64 -out $packageMsi -culture $culture @properties WixSource\*.wx?
        if ($? -ne $true) {
            throw "wix build error"
        }
    }

    $1033Msi = "$packageAppendDir\1033.msi"
    $1041Msi = "$packageAppendDir\1041.msi"
    $1041Mst = "$packageAppendDir\1041.mst"

    New-MsiSub "en-us" $1033Msi $stage
    New-MsiSub "ja-jp" $1041Msi  $stage

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

# Appx 
function New-Appx {
	param (
		[string]$referenceDir,
		[string]$packageAppxDir,
		[string]$appx
	)

	$packageFilesDir = "$packageAppxDir/PackageFiles"
	$contentDir = "$packageFilesDir/$product"

	# copy package base files
	Copy-Item "Appx\Resources" $packageFilesDir -Recurse -Force

	# copy resources.pri
	Copy-Item "Appx\resources.pri" $packageFilesDir

	# update assembly
	Copy-Item $referenceDir $contentDir -Recurse -Force
	New-ConfigForAppx $referenceDir "${product}.settings.json" $contentDir

	# generate README.html
	New-Readme $contentDir "en-us" "Appx" "Stable"
	New-Readme $contentDir "ja-jp" "Appx" "Stable"

	$param = Get-Content -Raw $env:CersPath/_$product.Parameter.json | ConvertFrom-Json
	$appxName = $param.name
	$appxPublisher = $param.publisher

	# generate AppManifest
	$content = Get-Content "Appx\AppxManifest.xml"
	$content = $content -replace "%NAME%", "$appxName"
	$content = $content -replace "%PUBLISHER%", "$appxPublisher"
	$content = $content -replace "%VERSION%", "$assemblyVersion"
	$content = $content -replace "%ARCH%", "x64"
	$content | Out-File -Encoding UTF8 "$packageFilesDir\AppxManifest.xml"

	# re-package
	& "$Win10SDK\makeappx.exe" pack /l /d "$packageFilesDir" /p "$appx"
	if ($? -ne $true) {
		throw "makeappx.exe error"
	}

	# signing
	& "$Win10SDK\signtool.exe" sign -f "$env:CersPath/_$product.pfx" -fd SHA256 -v "$appx"
	if ($? -ne $true) {
		throw "signtool.exe error"
	}
}

function New-AppxSym {
	param (
		[string]$publishDir,
		[string]$appxSym
	)
	$files = Get-ChildItem $publishDir -File -Filter *.pdb
	Compress-Archive -Path $files -DestinationPath $appxSym
}

function New-AppxUpload {
	param (
		[string]$publishDir,
		[string]$referenceDir,
		[string]$packageDir,
		[string]$name
	)
	$appx = "$name.msix"
	$appxSym = "$name.appxsym"
	$appxUpload = "$name.msixupload"
	New-Appx $referenceDir $packageDir $appx
	New-AppxSym $publishDir $appxSym
	Compress-Archive -Path $appx, $appxSym -DestinationPath $appxUpload
}

# Optimizing file placement with NetBeauty
# https://github.com/nulastudio/NetBeauty2
function Optimize-Package {
	param ([string]$packageDir)

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
	param (
		[bool]$keepPublish
	)

	if ([string]::IsNullOrWhiteSpace($appName)) {
		throw "$appName is empty."
	}

	if ($keepPublish) {
		Get-ChildItem -Directory "$appName*" | Where-Object { $_.Name -notin @($publishDir, $publishDir_fd, $referenceDir, $referenceDir_fd) } | Remove-Item -Recurse
	}
	else {
		Get-ChildItem -Directory "$appName*" | Remove-Item -Recurse
	}

	Get-ChildItem -File "$appName*.*" | Remove-Item

	Start-Sleep -m 200
}

function Build-Clear {
	# clear
	Write-Host "`n[Clear] ...`n" -fore Cyan
	Remove-BuildObjects $continue
}

function Build-UpdateState {
	$global:build_x64 = Test-Path $publishDir
	$global:build_x64_fd = Test-Path $publishDir_fd
}

function Build-PackageSource {
	if ($global:build_x64 -eq $true) { return }

	# build
	Write-Host "`n[Build] ...`n" -fore Cyan
	Build-ProjectSelfContained
	
	# create package source
	Write-Host "`n[Package] ...`n" -fore Cyan
	New-Package $product $publishDir $referenceDir $false

	$global:build_x64 = $true
}

function Build-PackageSource-fd {
	if ($global:build_x64_fd -eq $true) { return }

	# build
	Write-Host "`n[Build framework dependent] ...`n" -fore Cyan
	Build-ProjectFrameworkDependent
	
	# create package source
	Write-Host "`n[Package framework dependent] ...`n" -fore Cyan
	New-Package $product $publishDir_fd $referenceDir_fd $true

	$global:build_x64_fd = $true
}

function Build-Zip {
	param ([string]$stage = "Stable")

	Write-Host "`n`[Zip $stage] ...`n" -fore Cyan

	$packageName = New-PackageName $stage
	$packageZip = "$packageName.zip"
	$packageDir = "$packageName"
	New-Zip $referenceDir $packageDir $packageZip "Zip" $stage
	Write-Host "`nExport $packageZip succeeded.`n" -fore Green
}

function Build-Zip-fd {
	param ([string]$stage = "Stable")

	Write-Host "`n`[Zip-fd $stage] ...`n" -fore Cyan

	$packageName = New-PackageName $stage
	$packageZip = "$packageName-fd.zip"
	$packageDir = "$packageName-fd"
	New-Zip $referenceDir_fd $packageDir $packageZip "Zip-fd" $stage
	Write-Host "`nExport $packageZip succeeded.`n" -fore Green
}

function Build-Installer {
	param ([string]$stage = "Stable")

	Write-Host "`n[Installer $stage] ...`n" -fore Cyan
	
	$packageName = New-PackageName $stage
	$packageMsi = "$packageName.msi"
	$packageAppendDir = "$packageName-append"
	New-PackageAppend $referenceDir $packageAppendDir $stage
	New-Msi $referenceDir $packageAppendDir $packageMsi $stage
	Write-Host "`nExport $packageMsi succeeded.`n" -fore Green
}

function Build-Appx {
	param ([string]$stage = "Stable")
	if ($stage -ne "Stable") {
		throw "Build-Appx only supports 'Stable'."
	}

	Write-Host "`n[Appx $stage] ...`n" -fore Cyan

	if (Test-Path "$env:CersPath\_Parameter.ps1") {
		$packageName = New-PackageName $stage
		$packageAppx = "$packageName.msix"
		$packageAppxDir = "$packageName-appx"
		New-AppxUpload $publishDir $referenceDir $packageAppxDir $packageName
		Write-Host "`nExport $packageAppx succeeded.`n" -fore Green
	}
	else {
		Write-Host "`nWarning: not exist make appx environment. skip!`n" -fore Yellow
	}
}

function Export-Current {
	Write-Host "`n[Current] ...`n" -fore Cyan
	if (Test-Path $referenceDir_fd) {
		if (-not (Test-Path $product)) {
			New-Item $product -ItemType Directory
		}
		Copy-Item "$referenceDir_fd\*" "$product\" -Recurse -Force
	}
	else {
		Write-Host "`nWarning: not exist $referenceDir_fd. skip!`n" -fore Yellow
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

$appName = "${product}${appVersion}"

$publishDir = "$appName-publish"
$publishDir_fd = "$appName-publish-fd"
$referenceDir = "$product$appVersion-reference"
$referenceDir_fd = "$product$appVersion-reference-fd"


function New-PackageName {
	param ([string]$stage = "Stable")

	if ($stage -eq "Alpha") {
		"$appName-Alpha.$packageAlphaVersion" 
	}
	elseif ($stage -eq "Beta") {
		"$appName-Beta.$packageBetaVersion"
	}
	else {
		$appName
	}
}

Build-Clear
Build-UpdateState

if (($Target -eq "All") -or ($Target -eq "Zip")) {
	Build-PackageSource-fd
	Build-Zip-fd "Stable"
	Build-PackageSource
	Build-Zip "Stable"
}

if (($Target -eq "All") -or ($Target -eq "Installer")) {
	Build-PackageSource
	Build-Installer "Stable"
}

if (($Target -eq "All") -or ($Target -eq "Appx")) {
	Build-PackageSource
	Build-Appx
}

if ($Target -eq "Alpha") {
	Build-PackageSource-fd
	Build-Zip-fd "Alpha"
	Build-PackageSource
	Build-Zip "Alpha"
	Build-Installer "Alpha"
}

if ($Target -eq "Beta") {
	Build-PackageSource-fd
	Build-Zip-fd "Beta"
	Build-PackageSource
	Build-Zip "Beta"
	Build-Installer "Beta"
}

if (-not $continue) {
	Build-PackageSource-fd
	Export-Current
}

# Finish.
Write-Host "`nBuild $version All done.`n" -fore Green

