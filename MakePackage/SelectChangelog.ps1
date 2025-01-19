param (
    [string]$Path = "changelog.md",
    [string]$Version,
    [string]$Culture
)

# increment section depth
function Get-IndentLine($line) {
    if ($line.StartsWith("#")) {
        $line = "#" + $line
    }
    return $line
}

$versions = [ordered]@{ header = @() }
$current = "header"
$latestVersion = 1

# collect version logs
$lines = Get-Content $Path
foreach ($line in $lines) {
    if ($line.StartsWith("#")) {
        if ($line -match "^## (\d+)\.(\d+)") {
            $current = $Matches[1] + '.' + $Matches[2]
            $versions.add($current, @())
            $number = [int]$Matches[1]
            if ($number -gt $latestVersion) {
                $latestVersion = $number
            }
        }
    }
    $fixLine = Get-IndentLine $line
    $versions[$current] += $fixLine
}

if ([string]::IsNullOrEmpty($Version)) {
    # latest verssion series
    $versionRegex = "^$latestVersion\.";
}
elseif ($Version.Contains('.')) {
    # specified version only
    $versionRegex = "^" + $Version.Replace(".", "\.") + "$";
}
else {
    # specified verssion series
    $versionRegex = "^" + $Version + "\.";
}

# header
Write-Output $versions.header

# logs
foreach ($item in $versions.GetEnumerator()) {
    if ($item.key -match $versionRegex) {
        Write-Output $item.value
    }
}

# footer
Write-Output ""
Write-Output "----"
Write-Output ""
if ($Culture -eq "ja-jp") {
    Write-Output "これ以前の更新履歴は [こちら](https://neelabo.github.io/NeeView/ja-jp/changelog.html) を参照してください。"
}
else {
    Write-Output "Please see [here](https://neelabo.github.io/NeeView/en-us/changelog.html) for the previous change log."
}

