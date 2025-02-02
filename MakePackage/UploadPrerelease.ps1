# Canary, Beta のアップロード

param (
    [switch]$SkipCheckRepository,
    [switch]$SkipDelete
)

# error to break
trap { break }
$ErrorActionPreference = "stop"

# System.IO.Compression 名前空間を使用するためのアセンブリを読み込む
Add-Type -AssemblyName System.IO.Compression.FileSystem


# 配列から１つ選択する
function Select-OneFromArray {

    param ($array)

    Write-Host "Select an option:"
    for ($i = 0; $i -lt $array.Length; $i++) {
        Write-Host "$($i + 1). $($array[$i])"
    }
    
    $input_num = Read-Host "Enter the number of your choice"
    
    if ($input_num -match '^\d+$' -and $input_num -gt 0 -and $input_num -le $array.Length) {
        $selectedOption = $array[$input_num - 1]
        Write-Host "You selected: $selectedOption"
        return $selectedOption
    }
    else {
        throw "Invalid selection. Please run the script again and enter a valid number."
    }
}

# ZIPファイルから１エントリをJSONとして読み込む
function Get-JsonContentFromZip {
    param (
        [parameter(mandatory = $true)][string]$Path,
        [parameter(mandatory = $true)][string]$EntryName
    )

    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
    $entry = $zip.Entries | Where-Object { $_.Name -eq $EntryName }

    if ($null -ne $entry) {
        $stream = $entry.Open()
        $reader = New-Object System.IO.StreamReader($stream)
        $content = $reader.ReadToEnd()
        $reader.Close()
        $stream.Close()
        $jsonContent = $content | ConvertFrom-Json
        $jsonContent | ForEach-Object {
            Write-Output $_
        }
    }
    else {
        throw "File not found: $EntryName"
    }

    $zip.Dispose()
}


# アップロードするファイルを選択
$packages = Get-ChildItem -File | Where-Object Name -match "(Canary|Beta)\d+\.zip$"

if ($packages.Count -eq 0) {
    throw "Prerelease version file not found."
}
elseif ($packages.Count -eq 1) {
    $package = $packages[0]
}
else {
    $package = Select-OneFromArray $packages
}

if ($package -match "(Canary|Beta)") {
    $package_type = $Matches[1]
}
else {
    throw "Cannot found package type"
}

# タグ名生成
if ($package.Name -match "^[^\d]+(.+)\.zip$") {
    $release_name = $Matches[1]
    $tag_name = $release_name.ToLower()
}
else {
    throw "Cannot get tag name"
}


if (-not $SkipCheckRepository) {
    
    # git リポジトリが最新版であるかチェック
    git fetch
    $rev_count = git rev-list origin/master..master  --count      
    if ($rev_count -gt 0) {
        Write-Host "There are $rev_count differences from the remote repository."
        throw "There are differences with remote repositories. Please git push."
    }

    # パッケージのリビジョンが最新版であるかチェック
    $setting = Get-JsonContentFromZip -Path $package -EntryName "NeeView.settings.json"
    $git_hash = git show --format='%h' --no-patch
    if ($git_hash -ne $setting.Revision) {
        throw "The package is not made with the latest revision: $($setting.Revision), latest is $git_hash"
    }
}

# タグ存在チェック
<#
$tag_result = git tag -l $tag_name
if ($tag_result -ne $tag_name) {
    if ($CreateTag) {
        git tag $tag_name
        Write-Host "Tag $tag_name created. Please git push."
        exit 0
    }
    else {
        throw "The tag $tag_name does not exist"
    }
}
#>


# 開始確認
Write-Host
Write-Host "[Upload Prerelease Package]" -fore Cyan
Write-Host $package.Name
Write-Host 
Read-Host "Press Enter to continue"

# リポジトリ情報
$owner = "neelabo"
$repo = "NeeView"
$token = Get-Content "$env:CersPath/_GH.ReleaseAccessToken.txt" | Select-Object -First 1
$token = $token.Trim()
$release_url = "https://api.github.com/repos/$owner/$repo/releases"


# 既存の Prerelease パッケージを削除
if (-not $SkipDelete) {

    # GitHub REST API: List releases
    Write-Host "REST API: List releases" -fore Green
    $list_response = Invoke-RestMethod -Uri $release_url -Headers @{
        Authorization = "token $token"
        Accept        = "application/vnd.github.v3+json"
    }
    
    $pre_releases = $list_response | Where-Object { $_.prerelease -eq $true }
    
    if ($pre_releases.Count -gt 0) {
        Write-Host
        Write-Host "[Delete]" -fore Cyan
        $pre_releases | ForEach-Object {
            Write-Host $_.name
        }
        Write-Host
        Read-Host "Press Enter to delete all prerelease"
        
        $pre_releases | ForEach-Object {
            $delete_url = $_.url

            # GitHub REST API: Delete a release
            Write-Host "REST API: Delete $($_.name)" -fore Green
            Invoke-RestMethod -Method Delete -Uri $delete_url -Headers @{
                Authorization = "token $token"
                Accept        = "application/vnd.github.v3+json"
            }
        }
    }
    else {
        Write-Host "No prereleases currently exist."
    }
}


# リリースを作成

$canary_body = @"
## Canary Version

NeeView Canary is a snapshot of the development process.
It is intended to give you a preview of features that will be available in the official version.

See also: [About Canary Version](https://neelabo.github.io/NeeView/package-canary.html)
"@

$beta_body = @"
## Beta Version

NeeView Beta is the version just before the official release.
No new features will be added, only bug fixes. The official version will be released in approximately one to two weeks.

See also: [About Beta Version](https://neelabo.github.io/NeeView/package-beta.html)
"@

$date = (Get-Date).ToString("yyyy-MM-dd") 
$release_body = "($date)`r`n"
if ($package_type -eq "Canary") {
    $release_body += $canary_body
}
elseif ($package_type -eq "Beta") {
    $release_body += $beta_body
}
else {
    throw "Not supported package type: $package_type"
}

$release_data = @{
    tag_name   = $tag_name
    name       = $release_name
    body       = $release_body
    draft      = $true
    prerelease = $true
} | ConvertTo-Json

Write-Host
Write-Host "[Create]" -fore Cyan
Write-Host ”Tag: $tag_name"
Write-Host ”Title: $release_name"
Write-Host
Write-Host $release_body
Write-Host
Read-Host "Press Enter to create"

# GitHub REST API: Create a release
Write-Host "REST API: Create $release_name" -fore Green
$release_response = Invoke-RestMethod -Uri $release_url -Method Post -Headers @{
    Authorization  = "token $token"
    "Content-Type" = "application/json"
} -Body $release_data

$release_response | Format-List


# ファイルのアップロード
$release_id = $release_response.id
$file_path = $package.FullName
$file_name = $package.Name
$upload_url = "https://uploads.github.com/repos/$owner/$repo/releases/$release_id/assets?name=$file_name"

Write-Host 
Read-Host "Press Enter to upload $file_name"

# GitHub REST API:  Upload a release asset
Write-Host "REST API: Upload $file_name" -fore Green
$upload_response = Invoke-RestMethod -Uri $upload_url -Method Post -Headers @{
    Accept         = "application/vnd.github+json"
    Authorization  = "token $token"
    "Content-Type" = "application/zip"
} -InFile $file_path

$upload_response | Format-List

Write-Host 
Write-Host "done. "

# NOTE: Content-Type
# .zip -> application/zip
# .msi -> application/x-msi or application/octet-stream

