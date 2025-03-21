$product = 'NeeView'
$makepri = "C:\Program Files (x86)\Windows Kits\10\bin\x64\makepri.exe"

$param = Get-Content -Raw $env:CersPath/_$product.Parameter.json | ConvertFrom-Json
$appxName = $param.name

& $makepri new /pr Resources\ /cf priconfig.xml /of resources.pri /in $appxName  /o
& $makepri dump /if resources.pri /of resources.pri.xml /o
