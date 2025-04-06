# コミット数をビルドバージョンにして _Version.props を作成する

param (
    [string]$baseVersion = "43.1",
    [string]$suffix = ""
)

# 現在のスクリプトの場所を得る
$path = Split-Path $MyInvocation.MyCommand.Path -Parent

# 現在のブランチのコミットカウントを得る
$branch = git -C $path rev-parse --abbrev-ref HEAD
$commitCount = git -C $path rev-list --count $branch

# バージョン文字列生成
$version = "$baseVersion.$commitCount"

if ([string]::IsNullOrEmpty($suffix)) {
    Write-Host $version
}
else {
    Write-Host "$version-$suffix"
}

# _Version.props を作成
$xml = [xml]"<Project><PropertyGroup><VersionPrefix/><VersionSuffix/></PropertyGroup></Project>"
$xml.Project.PropertyGroup.VersionPrefix = $version
$xml.Project.PropertyGroup.VersionSuffix = $suffix
$xml.Save("$path\_Version.props")
