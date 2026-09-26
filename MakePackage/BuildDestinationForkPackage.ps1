param(
    [Parameter(Mandatory = $false)]
    [string]$DotNetPath = ""
)

$ErrorActionPreference = "Stop"

<#
.SYNOPSIS
解析可用于 NeeView 发布的 dotnet 命令路径。
.PARAMETER RequestedPath
调用者显式指定的 dotnet 路径；为空时按 PATH 和本机用户级 SDK 依次查找。
.OUTPUTS
可直接调用的 dotnet 可执行文件绝对路径或命令名。
#>
function Resolve-DotNetCommand {
    param(
        [Parameter(Mandatory = $false)]
        [string]$RequestedPath
    )

    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        if (-not (Test-Path -LiteralPath $RequestedPath -PathType Leaf)) {
            throw "找不到指定的 dotnet：$RequestedPath"
        }
        return (Resolve-Path -LiteralPath $RequestedPath).Path
    }

    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    # 官方用户级 SDK 是当前开发机的可移植回退，不改变系统 PATH。
    $userSdk = Join-Path $env:LOCALAPPDATA "dotnet-sdk-10\dotnet.exe"
    if (Test-Path -LiteralPath $userSdk -PathType Leaf) {
        return $userSdk
    }

    throw "未找到 dotnet SDK。请通过 -DotNetPath 指定 dotnet.exe。"
}

<#
.SYNOPSIS
验证九个快捷分类脚本的文件数量、快捷键和目标目录索引映射。
.PARAMETER ScriptsDirectory
包含 MoveToDestination1.nvjs 至 MoveToDestination9.nvjs 的目录。
#>
function Assert-DestinationScripts {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ScriptsDirectory
    )

    $scripts = @(Get-ChildItem -LiteralPath $ScriptsDirectory -Filter "MoveToDestination*.nvjs" -File)
    if ($scripts.Count -ne 9) {
        throw "快捷分类脚本数量应为 9，实际为 $($scripts.Count)。"
    }

    foreach ($index in 1..9) {
        $scriptPath = Join-Path $ScriptsDirectory "MoveToDestination$index.nvjs"
        if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
            throw "缺少脚本：$scriptPath"
        }

        $content = Get-Content -LiteralPath $scriptPath -Raw
        # 快捷键与 Destination Folders 索引必须一一对应，防止打包时发生错位。
        if ($content -notmatch "(?m)^//\s*@shortCutKey\s+$index\s*$" -or
            $content -notmatch ('"Index"\s*:\s*' + $index + '\b')) {
            throw "脚本映射无效：$scriptPath"
        }
    }
}

<#
.SYNOPSIS
发布 NeeView x64 薄 Fork，并生成便携 ZIP 与 SHA-256 校验文件。
.PARAMETER DotNetCommand
用于 restore 和 publish 的 dotnet 命令。
#>
function New-DestinationForkPackage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DotNetCommand
    )

    $repositoryRoot = Split-Path -Parent $PSScriptRoot
    $projectPath = Join-Path $repositoryRoot "NeeView\NeeView.csproj"
    $publishProfile = "FolderProfile-x64.pubxml"
    $publishDirectory = Join-Path $repositoryRoot "MakePackage\Publish\NeeView-x64"
    $sourceScripts = Join-Path $repositoryRoot "SampleScripts"
    $chineseGuide = Join-Path $repositoryRoot "docs\zh-cn\destination-folder-panel.md"
    $deliverablesDirectory = Join-Path $repositoryRoot "MakePackage\Deliverables"
    $archivePath = Join-Path $deliverablesDirectory "NeeView-46.3-DestinationFolderFork-x64.zip"
    $hashPath = "$archivePath.sha256"
    $stagingDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("NeeViewDestinationForkPackage-" + [Guid]::NewGuid().ToString("N"))

    Assert-DestinationScripts -ScriptsDirectory $sourceScripts

    # 使用项目既有发布目录和 x64 Profile，避免额外构建目录污染仓库。
    & $DotNetCommand restore $projectPath "-p:PublishProfile=$publishProfile"
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore 失败，退出码：$LASTEXITCODE"
    }

    & $DotNetCommand publish $projectPath -c Release "-p:PublishProfile=$publishProfile" --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish 失败，退出码：$LASTEXITCODE"
    }

    $revision = (& git -C $repositoryRoot rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($revision)) {
        throw "无法读取 Git revision。"
    }
    & git -C $repositoryRoot diff --quiet
    if ($LASTEXITCODE -ne 0) {
        $revision = "$revision-dirty"
    }

    # ZIP 包始终使用程序旁 Profile，关闭开发水印并与 Store Profile 隔离。
    $settings = [ordered]@{
        PackageType = "Zip"
        Revision = $revision
        SelfContained = $false
        UseLocalApplicationData = $false
        TemporaryFilesInProfileFolder = $false
        TrimSaveData = $true
        PathProcessGroup = $true
        SoftwareRendering = $false
        Watermark = $false
        LogFile = $null
    }
    try {
        New-Item -ItemType Directory -Path $stagingDirectory -Force | Out-Null

        # 发布目录可能被开发运行留下 Profile；暂存时明确排除，保证交付包从独立的干净配置启动。
        Get-ChildItem -LiteralPath $publishDirectory | Where-Object {
            $_.Name -notin @("Profile", "NeeView.settings.json", "README-zh-CN.md")
        } | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $stagingDirectory -Recurse -Force
        }

        # Fork 的 1～9 已注册为可编辑的原生命令；不再预装同快捷键脚本，避免用户启用脚本后发生冲突。
        Copy-Item -LiteralPath $chineseGuide -Destination (Join-Path $stagingDirectory "README-zh-CN.md") -Force
        $settings | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $stagingDirectory "NeeView.settings.json") -Encoding utf8

        New-Item -ItemType Directory -Path $deliverablesDirectory -Force | Out-Null
        if (Test-Path -LiteralPath $archivePath) {
            Remove-Item -LiteralPath $archivePath -Force
        }
        Compress-Archive -Path (Join-Path $stagingDirectory "*") -DestinationPath $archivePath -CompressionLevel Optimal
    }
    finally {
        if (Test-Path -LiteralPath $stagingDirectory -PathType Container) {
            $resolvedStaging = (Resolve-Path -LiteralPath $stagingDirectory).Path
            $expectedParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath()).TrimEnd([System.IO.Path]::DirectorySeparatorChar)

            # 只清理本次创建且位于系统临时目录直属层级的暂存目录。
            if ((Split-Path -Parent $resolvedStaging) -ne $expectedParent -or
                (Split-Path -Leaf $resolvedStaging) -notmatch '^NeeViewDestinationForkPackage-[0-9a-f]{32}$') {
                throw "拒绝清理非预期暂存目录：$resolvedStaging"
            }
            [System.IO.Directory]::Delete($resolvedStaging, $true)
        }
    }

    $hash = Get-FileHash -LiteralPath $archivePath -Algorithm SHA256
    "$($hash.Hash)  $($hash.Path | Split-Path -Leaf)" | Set-Content -LiteralPath $hashPath -Encoding ascii
    Write-Host "已生成：$archivePath"
    Write-Host "校验文件：$hashPath"
}

$resolvedDotNet = Resolve-DotNetCommand -RequestedPath $DotNetPath
New-DestinationForkPackage -DotNetCommand $resolvedDotNet
