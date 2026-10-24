param(
    [ValidateSet('build', 'package')]
    [string]$Action = 'package',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$LocalConfig = '',
    [string]$Sts2Path = '',
    [string]$GodotPath = '',
    [string]$OutputRoot = ''
)

# 独立构建入口：只读取本目录和显式提供的工具链／游戏引用。
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$modRoot = [IO.Path]::GetFullPath($PSScriptRoot)
if (!$LocalConfig) { $LocalConfig = Join-Path $modRoot 'local.props' }
if (!$OutputRoot) { $OutputRoot = Join-Path $modRoot 'artifacts' }
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
$logRoot = Join-Path $OutputRoot 'logs'
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Invoke-BuildTool([string]$Executable, [string[]]$Arguments, [string]$LogName) {
    [IO.Directory]::CreateDirectory($logRoot) | Out-Null
    $logPath = Join-Path $logRoot $LogName
    $previousPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        & $Executable @Arguments > $logPath 2>&1
        $result = $LASTEXITCODE
    }
    finally { $ErrorActionPreference = $previousPreference }
    $output = Get-Content -LiteralPath $logPath -Raw
    if ($result -ne 0 -or $output -match '(?m)^\s*(ERROR:|SCRIPT ERROR:)') {
        Get-Content -LiteralPath $logPath -Tail 25 | Write-Host
        throw "Command failed (exit $result). Log: $logPath"
    }
    Write-Host "OK: $LogName"
}

try {
    # global.json、NuGet.Config 和 Directory.Build.props 都由本目录提供。
    if (Test-Path -LiteralPath $LocalConfig) {
        [xml]$config = Get-Content -LiteralPath $LocalConfig -Raw
        if (!$Sts2Path) {
            $node = $config.SelectSingleNode('/Project/PropertyGroup/Sts2Path')
            if ($node) { $Sts2Path = $node.InnerText }
        }
        if (!$GodotPath) {
            $node = $config.SelectSingleNode('/Project/PropertyGroup/GodotPath')
            if ($node) { $GodotPath = $node.InnerText }
        }
    }
    if (!$Sts2Path -or ![IO.Path]::IsPathRooted($Sts2Path)) {
        throw 'Set an absolute -Sts2Path, or copy local.props.example to local.props.'
    }
    $gameAssembly = Join-Path $Sts2Path 'data_sts2_windows_x86_64\sts2.dll'
    if (!(Test-Path -LiteralPath $gameAssembly -PathType Leaf)) { throw "Missing game reference: $gameAssembly" }
    if ($Action -eq 'package') {
        if (!$GodotPath -or ![IO.Path]::IsPathRooted($GodotPath) -or !(Test-Path -LiteralPath $GodotPath -PathType Leaf)) {
            throw 'Set an absolute -GodotPath to the matching MegaDot .NET console executable.'
        }
        [xml]$project = Get-Content -LiteralPath (Join-Path $modRoot 'HeavySilent.csproj') -Raw
        $engineBaseVersion = $project.Project.Sdk.Split('/')[1]
        $engineVersion = (& $GodotPath --version | Out-String).Trim()
        if ($LASTEXITCODE -ne 0 -or $engineVersion -notmatch ('^' + [regex]::Escape($engineBaseVersion) + '\.') -or $engineVersion -notmatch 'mono') {
            throw "Expected a $engineBaseVersion .NET engine; found '$engineVersion'."
        }
    }

    Push-Location $modRoot
    try {
        Invoke-BuildTool 'dotnet' @('restore', 'HeavySilent.csproj', '--locked-mode', '--configfile', 'NuGet.Config',
            "-p:Configuration=$Configuration", "-p:Sts2Path=$Sts2Path") 'restore.log'
        Invoke-BuildTool 'dotnet' @('build', 'HeavySilent.csproj', '--no-restore', '-c', $Configuration,
            "-p:Sts2Path=$Sts2Path") 'build.log'
    }
    finally { Pop-Location }

    $buildOutput = Join-Path $modRoot ".godot\mono\temp\bin\$Configuration"
    if ($Action -eq 'build') {
        Write-Host "Assembly ready: $(Join-Path $buildOutput 'HeavySilent.dll')"
    }
    else {
        # 只在临时资源工程导出，避免引擎再次发布 C# 或收集本机配置。
        $stage = Join-Path $modRoot ('.cache\package-' + [guid]::NewGuid().ToString('N'))
        [IO.Directory]::CreateDirectory($stage) | Out-Null
        Copy-Item -LiteralPath (Join-Path $modRoot 'HeavySilent') -Destination $stage -Recurse
        Copy-Item -LiteralPath (Join-Path $modRoot 'export_presets.cfg') -Destination $stage
        [IO.File]::WriteAllText((Join-Path $stage 'project.godot'),
            "config_version=5`n[application]`nconfig/name=`"HeavySilent assets`"`n[rendering]`nrenderer/rendering_method=`"gl_compatibility`"`n", $utf8)
        $stagePack = Join-Path $stage 'HeavySilent.pck'
        Invoke-BuildTool $GodotPath @('--headless', '--path', $stage, '--editor', '--import') 'import.log'
        Invoke-BuildTool $GodotPath @('--headless', '--path', $stage, '--export-pack', 'ModAssets', $stagePack) 'export.log'

        $inputs = @((Join-Path $buildOutput 'HeavySilent.dll'), $stagePack,
            (Join-Path $modRoot 'HeavySilent.json'), (Join-Path $modRoot 'THIRD_PARTY_NOTICES.md'))
        if ($Configuration -eq 'Debug') { $inputs += Join-Path $buildOutput 'HeavySilent.pdb' }
        foreach ($inputPath in $inputs) {
            if (!(Test-Path -LiteralPath $inputPath -PathType Leaf) -or (Get-Item -LiteralPath $inputPath).Length -eq 0) {
                throw "Build output is missing or empty: $inputPath"
            }
        }
        $destination = Join-Path $OutputRoot 'package\HeavySilent'
        [IO.Directory]::CreateDirectory($destination) | Out-Null
        foreach ($inputPath in $inputs) { Copy-Item -LiteralPath $inputPath -Destination $destination }
        $symbols = Join-Path $destination 'HeavySilent.pdb'
        if ($Configuration -eq 'Release' -and (Test-Path -LiteralPath $symbols)) { Remove-Item -LiteralPath $symbols }
        Write-Host "Package ready: $destination"
    }
}
catch { Write-Error $_ -ErrorAction Continue; exit 1 }
