

function Get-HtmlSource {
    param (
        [parameter(mandatory = $true)][string]$Path
    )

    $html = Get-Content $Path

    $title = [System.IO.Path]::GetFileName($Path)
    foreach ($line in $html) {
        if ($line -match "<h1>(.+)</h1>") {
            $title = $Matches[1]
            break
        }
    }

    Write-Output "---"
    Write-Output "layout: default"
    Write-Output "---"
    Write-Output ""

    $phase = 0
    foreach ($line in $html) {
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


function Write-CultureDocs{
    param ([string]$culture)
    
    Copy-Item "$neeview_profile\CommandLineOptions.$culture.md" "docs\$culture\commandline-options.md"
    
    Get-HtmlSource "$neeview_profile\CommandList.$culture.html" > "docs\$culture\command-list.html"
    Get-HtmlSource "$neeview_profile\MainMenu.$culture.html" > "docs\$culture\main-menu.html"
    Get-HtmlSource "$neeview_profile\ScriptManual.$culture.html" > "docs\$culture\script-manual.html"
    Get-HtmlSource "$neeview_profile\SearchOptionManual.$culture.html" > "docs\$culture\search-options.html"
}

$neeview = "NeeView\bin\x64\Debug\net8.0-windows"
$neeview_profile = "$neeview\Profile"

Write-Host "Create en-us Embedded Documents..."
Start-Process -FilePath "$neeview\NeeView.exe" -Wait -WindowStyle Minimized -ArgumentList "--debug=export-docs -l en -n"

Write-Host "Create ja-jp Embedded Documents..."
Start-Process -FilePath "$neeview\NeeView.exe" -Wait -WindowStyle Minimized -ArgumentList "--debug=export-docs -l ja -n"

Write-Host "Export for docs."
Write-CultureDocs "en-us"
Write-CultureDocs "ja-jp"

