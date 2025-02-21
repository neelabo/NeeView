<#
.SYNOPSIS
Converting Language Files and Json

.DESCRIPTION
Converts between language files ([culture].restext) and Json. This is a utility tool, not a required feature.

.EXAMPLE
> .\ConvertRestext.ps1 Convert -Sort
Generate language files from Language.json.
-Sort by key if specified.

.EXAMPLE
> .\ConvertRestext.ps1 Revert
Convert language files to Language.json.

.PARAMETER Mode
Convert mode 
    Convert: Convert Json file to language files
    Revert: Convert language files to Json file

.PARAMETER JsonFile
Json file name. Default is Language.json

.PARAMETER Sort
Sort data by key

.PARAMETER Clean
For Convert, clear if the text is the same as English.

.PARAMETER Trim
For Convert, trim empty data.

.PARAMETER Cultures
For Convert, Specify the culture to be processed.
Specify cultures separated by commas. If not specified, all cultures are processed.
e.g., -Cultures en,ja

#>

param (
    [Parameter(Mandatory = $true)]
    [ValidateSet("Convert", "Revert")]
    [string]$Mode,
    [string]$JsonFile = "Language.json",
    [switch]$Sort,
    [switch]$Clean,
    [switch]$Trim,
    [string[]]$Cultures = @()
)

$modeDetail = switch ($Mode) {
    "Convert" { "$JsonFile -> *.restext" }
    "Revert" { "*.restext -> $JsonFile" }
    default {"Unknown"}
}

Write-Host
Write-Host "[Properties] ..." -fore Cyan
Write-Host "Mode: $Mode" -NoNewline
Write-Host " ($modeDetail)" -fore Green
Write-Host "JsonFile: $JsonFile"
Write-Host "Sort: $Sort"
Write-Host "Clean: $Clean"
Write-Host "Trim: $Trim"
Write-Host "Cultures: $Cultures"
Write-Host
Read-Host "Press Enter to continue"


$Filter = $Cultures
$defaultCulture = "en"

# error to break
trap { break }
$ErrorActionPreference = "stop"


function Get-Restext {
    param ([string]$culture)
    $restext = "$culture.restext"
    Write-Host Read $restext
    $array = @()
    foreach ($line in Get-Content $restext) {
        if ([string]::IsNullOrWhitespace($line)) {
            continue
        }
        $tokens = $line -split "=", 2
        $key = $tokens[0]
        $value = $tokens[1]
        if (($culture -eq $defaultCulture) -or (-not [string]::IsNullOrEmpty($value))) {
            $array += [PSCustomObject]@{
                Key   = $key
                Value = $value
            }
        }
    }
    return $array
}

function ConvertTo-RestextMap {
    param ($array)
    $map = @{}
    foreach ($obj in $array) {
        $map[$obj.Key] = $obj.Value
    }
    return $map
}

function Test-AdditionalKey {
    param ([string]$key)
    return ($key.Contains(':') -or $key.StartsWith("Key.") -or $key.StartsWith("ModifierKeys.") -or $key.StartsWith("MouseAction.") -or $key.StartsWith("MouseButton.") -or $key.StartsWith("MouseDirection.") -or $key.StartsWith("TouchArea."))
}

function Add-RestextToRestextTable {
    param([PSCustomObject]$table, [string]$culture)
    $array = Get-Restext $culture
    $map = ConvertTo-RestextMap $array

    foreach ($entry in $map.GetEnumerator()) {
        $key = $entry.Key
        $value = $entry.Value
        if ($null -ne $table.$key) {
            $table.$key | Add-Member -MemberType NoteProperty -Name $culture -Value $value
        }
        elseif (Test-AdditionalKey $key) {
            $obj = [PSCustomObject]@{
                $culture = $value
            }
            $table | Add-Member -MemberType NoteProperty -Name $key -Value $obj
        }
    }
}

function ConvertTo-RestextFromRestextTable {
    param ([PSCustomObject]$table, [string]$culture)
    $lines = @()
    foreach ($property in $table.psobject.Properties) {
        $key = $property.Name
        #$key = [regex]::Replace($property.Name, "\.([a-z])", { $args.value.toUpper() })

        $value = $property.Value.$culture
        $defaultValue = $property.Value.$defaultCulture

        $isAdditionalKey = Test-AdditionalKey $key

        if ($Clean -and ($culture -ne $defaultCulture) -and ($value -eq $defaultValue)) {
            $value = $null
        }

        $isEmpty = $null -eq $value
        $isTrimEmpty = $Trim -and $isEmpty
        $isRequired = ($null -ne $property.Value.$defaultCulture) -and (-not $isAdditionalKey)

        if ($isAdditionalKey) {
            if (-not $isEmpty) {
                $lines += $key + "=" + $value
            }
        }
        else {
            if ($isRequired -and (-not $isTrimEmpty)) {
                $lines += $key + "=" + $value
            }
        }
    }
    return $lines
}

function Get-RestextCultures {
    $cultures = Get-ChildItem *.restext -Exclude shared.restext | ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_.Name) }
    return $cultures
}

function Get-DefaultRestextTable {
    param ([string]$culture)
    $array = Get-Restext $culture
    $table = [PSCustomObject]@{}
    foreach ($pair in $array) {
        $obj = [PSCustomObject]@{
            $culture = $pair.Value
        }
        $table | Add-Member -MemberType NoteProperty -Name $pair.Key -Value $obj
    }
    return $table
}

function Get-RestextTable {
    param([string[]]$cultures)

    $table = Get-DefaultRestextTable $defaultCulture
    foreach ($culture in $cultures) {
        if ($culture -ne $defaultCulture) {
            Add-RestextToRestextTable $table $culture
        }
    }
    return $table
}

function Set-RestextTable {
    param([PSCustomObject]$table, [string[]]$cultures)
    foreach ($culture in Get-FilteredCultures($cultures)) {
        $restext = "$culture.restext"
        Write-Host Write $restext 
        ConvertTo-RestextFromRestextTable $table $culture | Set-Content $restext -Encoding utf8
    }
}

function Get-FilteredCultures {
    param([string[]]$cultures)

    if ($Filter.Length -eq 0) {
        return $cultures
    }
    else {
        return $cultures | Where-Object { $Filter.Contains($_) }
    }
}


function Sort-RestextTable { 
    param([PSCustomObject]$table)
    $newTable = [PSCustomObject]@{}
    foreach ($property in $table.psobject.properties | Sort-Object -Property Name) {
        $key = $property.Name
        $value = $property.Value
        $newTable | Add-Member -MemberType NoteProperty -Name $key -Value $value
    }
    return $newTable
}

function Get-CulturesFromRestextTable {
    param([PSCustomObject]$table)
    $cultures = @()
    foreach ($property in $table.psobject.properties) {
        foreach ($culture in $property.Value.psobject.properties.name) {
            if (-not $cultures.Contains($culture)) {
                $cultures += $culture
            }
        }
    }
    return $cultures
}

# fix typo (no used)
function Get-ReplaceTypo {
    $transform = [PSCustomObject]@{
        "Confrict"   = "Conflict"
        "Deault"     = "Default"
        "Inclide"    = "Include"
        "Sceme"      = "Scheme"
        "Javascript" = "JavaScript"
        "Playlsit"   = "Playlist"
        "Rerset"     = "Reset"
        "Lastest"    = "Latest"
        "Vertival"   = "Vertical"
        "Scipt"      = "Script"
        "Manipurate" = "Manipulate"
        "Fodler"     = "Folder"
        "Visibled"   = "Visible"
        "Openbook"   = "OpenBook"
    }

    param([string]$s)
    foreach ($pair in $transform.psobject.properties) {
        $s = $s -creplace $pair.Name, $pair.Value
    }
    return $s
}

# fix typo (table)
function Get-ReplaceTypoTable { 
    param([PSCustomObject]$table)
    $newTable = [PSCustomObject]@{}
    foreach ($property in $table.psobject.properties) {
        $key = Get-ReplaceTypo $property.Name
        $value = $property.Value
        $newTable | Add-Member -MemberType NoteProperty -Name $key -Value $value
    }
    return $newTable
}

#
# MAIN
#

if ($Mode -eq "Convert") {
    Write-Host Read $JsonFile
    $table = Get-Content $JsonFile | ConvertFrom-Json   
    if ($Sort) {
        $table = Sort-RestextTable $table
    }
    $cultures = Get-CulturesFromRestextTable $table
    Set-ResTextTable $table $cultures
}
elseif ($Mode -eq "Revert") {
    $cultures = Get-RestextCultures
    $table = Get-RestextTable $cultures
    if ($Sort) {
        $table = Sort-RestextTable $table
    }
    Write-Host Write $JsonFile
    $table | ConvertTo-Json | Set-Content $JsonFile -Encoding utf8
}
else { 
    throw  "'$Mode' is an unknown mode. Specify 'Convert' or 'Revert' as the mode."
}

