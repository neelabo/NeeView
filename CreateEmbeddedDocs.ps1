# error to break
trap { break }
$ErrorActionPreference = "stop"


function Get-JekyllSource {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [object[]]$Content,
        [string]$Title
    )

    begin {
        $phase = 0

        Write-Output "---"
        Write-Output "layout: default"
        Write-Output "title: $Title"
        Write-Output "---"
        Write-Output ""
    }

    process {
        foreach ($line in $Content) {
            if ($phase -eq 0) {
                if ($line -match "<body>(.*)$") {
                    Write-Output $Matches[1]
                    $phase ++
                }
            }
            elseif ($phase -eq 1) {
                if ($line -match "^(.*)</body>") {
                    Write-Output $Matches[1]
                    $phase ++
                }
                else {
                    Write-Output $line
                }
            }
        }
    }
}

function Get-Title {
    param (
        $Content
    )

    foreach ($line in $Content) {
        if ($line -match "<h1>\s*(.+)\s*</h1>") {
            return $Matches[1]
        }
    }
    return "no title"
}

function Get-TrimmedNeeViewHeading {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [object[]]$Content
    )

    process {
        $regex = "<h1>\s*(NeeView\s*)?(.+)</h1>"
        foreach ($line in $Content) {
            if ($line -match $regex) {
                $title = $Matches[2].Trim()
                $title = $title.Substring(0, 1).ToUpper() + $title.Substring(1)
                $line -replace $regex, "<h1>$title</h1>"
            }
            else {
                $line
            }
        }
    }
}

function Get-FixedCommandList {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $True, ValueFromPipeline = $True)]
        [object[]]$Content,
        [string]$Culture
    )

    begin {
        $phase = 0
    }

    process {
        foreach ($line in $Content) {
            if ($phase -eq 0) {
                if ($line -match "^<!-- section:\s*([^\s]+)\s*-->") {
                    $section = $Matches[1]
                    if ($section -eq "note") {
                        $phase = 1
                        if ($Culture -eq "ja-jp") {
                            Write-Output "<p>このリストのキー設定はプリセット TypeA です。</p>"
                        }
                        else {
                            Write-Output "<p>The key setting for this listing is preset TypeA.</p>"
                        }
                        continue
                    }
                }
            }
            elseif ($phase -eq 1) {
                if ($line -match "^<!-- section_end:\s*([^\s]+)\s*-->") {
                    $phase = 2
                }
                continue
            }
            Write-Output $line
        }
    }
}

function Write-CultureDocs {
    param ([string]$culture)
    
    $output = "docs\$culture"

    Copy-Item "$neeview_profile\CommandLineOptions.$culture.md" "docs\$culture\commandline-options.md"

    $content = Get-Content "$neeview_profile\CommandList.$culture.html" | Get-TrimmedNeeViewHeading | Get-FixedCommandList -Culture $culture
    $title = Get-Title $content
    $content | Get-JekyllSource -Title $title > "$output\command-list.html"

    $content = Get-Content "$neeview_profile\MainMenu.$culture.html" | Get-TrimmedNeeViewHeading
    $title = Get-Title $content
    $content | Get-JekyllSource -Title $title > "$output\main-menu.html"

    $content = Get-Content "$neeview_profile\ScriptManual.$culture.html" | Get-TrimmedNeeViewHeading
    $title = Get-Title $content
    $content | Get-JekyllSource -Title $title > "$output\script-manual.html"

    $content = Get-Content "$neeview_profile\SearchOptionManual.$culture.html" | Get-TrimmedNeeViewHeading
    $title = Get-Title $content
    $content | Get-JekyllSource -Title $title > "$output\search-options.html"
}

$neeview = "NeeView\bin\x64\Debug\net9.0-windows"
$neeview_profile = "$neeview\Profile"

Write-Host "Create en-us Embedded Documents..."
Start-Process -FilePath "$neeview\NeeView.exe" -Wait -WindowStyle Minimized -ArgumentList "--debug=export-docs -l en -n"

Write-Host "Create ja-jp Embedded Documents..."
Start-Process -FilePath "$neeview\NeeView.exe" -Wait -WindowStyle Minimized -ArgumentList "--debug=export-docs -l ja -n"

Write-Host "Export for docs."
Write-CultureDocs "en-us"
Write-CultureDocs "ja-jp"

