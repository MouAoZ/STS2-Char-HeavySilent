param(
    [ValidateSet('doctor', 'build', 'package', 'deploy', 'logs', 'verify', 'test-setup', 'test-start', 'test', 'test-stop')]
    [string]$Action = 'doctor',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [string]$LocalConfig = '',
    [switch]$CheckLatest,
    [ValidateSet('', 'smoke', 'regression')]
    [string]$Suite = '',
    [string]$TaskFile = '',
    [ValidateSet('', 'eng', 'zhs')]
    [string]$Language = '',
    [switch]$KeepOpen
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$modRoot = Join-Path $repoRoot 'mod'
$artifactRoot = Join-Path $repoRoot 'artifacts'
$logRoot = Join-Path $artifactRoot 'logs'
$packageRoot = Join-Path $artifactRoot 'package'
$packageInfoPath = Join-Path $packageRoot 'package-info.json'
$utf8 = New-Object System.Text.UTF8Encoding($false)
if (!$LocalConfig) { $LocalConfig = Join-Path $modRoot 'local.props' }

function Read-Json([string]$Path) {
    return [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8) | ConvertFrom-Json
}

function Write-Utf8([string]$Path, [string]$Text) {
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($Path)) | Out-Null
    [IO.File]::WriteAllText($Path, $Text, $utf8)
}

function Get-Sha256([string]$Path) {
    $stream = [IO.File]::OpenRead($Path)
    $hash = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($hash.ComputeHash($stream))).Replace('-', '').ToLowerInvariant() }
    finally { $stream.Dispose(); $hash.Dispose() }
}

function Invoke-Logged([string]$Executable, [string[]]$Arguments, [string]$LogName) {
    [IO.Directory]::CreateDirectory($logRoot) | Out-Null
    $logPath = Join-Path $logRoot $LogName
    $previousPreference = $ErrorActionPreference
    try {
        # Windows PowerShell wraps native stderr in ErrorRecord, even for harmless diagnostics.
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

function Get-Environment {
    if (!(Test-Path -LiteralPath $LocalConfig)) {
        throw 'Missing mod/local.props. Copy mod/local.props.example and set the three absolute paths.'
    }
    [xml]$config = [IO.File]::ReadAllText($LocalConfig)
    $settings = @{}
    foreach ($key in @('Sts2Path', 'GodotPath', 'BaseLibPath')) {
        $node = $config.SelectSingleNode("/Project/PropertyGroup/$key")
        if (!$node -or ![IO.Path]::IsPathRooted($node.InnerText)) { throw "$key must be an absolute path in local.props." }
        $settings[$key] = [IO.Path]::GetFullPath($node.InnerText)
    }
    foreach ($path in @(
        $settings.GodotPath,
        (Join-Path $settings.Sts2Path 'release_info.json'),
        (Join-Path $settings.Sts2Path 'data_sts2_windows_x86_64\sts2.dll'),
        (Join-Path $settings.Sts2Path 'SlayTheSpire2.pck'),
        (Join-Path $settings.BaseLibPath 'BaseLib.json'),
        (Join-Path $settings.BaseLibPath 'BaseLib.dll'),
        (Join-Path $settings.BaseLibPath 'BaseLib.pck')
    )) {
        if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing dependency: $path" }
    }
    $baseline = Read-Json (Join-Path $repoRoot 'docs\development\baseline.json')
    $release = Read-Json (Join-Path $settings.Sts2Path 'release_info.json')
    $baseManifest = Read-Json (Join-Path $settings.BaseLibPath 'BaseLib.json')
    [xml]$props = Get-Content -LiteralPath (Join-Path $modRoot 'Directory.Build.props') -Raw
    $expectedBase = $props.Project.PropertyGroup.BaseLibVersion
    if ($baseManifest.version.TrimStart('v') -ne $expectedBase) {
        throw "Installed BaseLib $($baseManifest.version) differs from compile dependency $expectedBase. Align versions and refresh the lock file."
    }
    Push-Location $modRoot
    try {
        $sdk = (& dotnet --version | Out-String).Trim()
        if ($LASTEXITCODE -ne 0) { throw 'The SDK pinned by mod/global.json is unavailable.' }
    }
    finally { Pop-Location }
    $engineVersion = (& $settings.GodotPath --version | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or $engineVersion -notmatch ('^' + [regex]::Escape($baseline.godot_version) + '\.')) {
        throw "Godot must match $($baseline.godot_version); found '$engineVersion'."
    }
    if ($engineVersion -notmatch 'mono') { throw 'Use the .NET/mono engine distribution.' }
    $gameHash = Get-Sha256 (Join-Path $settings.Sts2Path 'data_sts2_windows_x86_64\sts2.dll')
    $baseHash = Get-Sha256 (Join-Path $settings.BaseLibPath 'BaseLib.dll')
    if ($gameHash -ne $baseline.game_assembly_sha256) {
        Write-Warning "Game changed to $($release.version). Review API compatibility and update docs/development/baseline.json after validation."
    }
    $settings.GameVersion = $release.version
    $settings.GameHash = $gameHash
    $settings.BaseLibVersion = $baseManifest.version.TrimStart('v')
    $settings.BaseLibHash = $baseHash
    $settings.Sdk = $sdk
    $settings.GodotVersion = $engineVersion
    $settings.PowerShellVersion = $PSVersionTable.PSVersion.ToString()
    $settings.PowerShellPath = (Get-Process -Id $PID).Path
    $manifestPath = Join-Path $settings.Sts2Path '..\..\appmanifest_2868840.acf'
    $settings.SteamBuildId = ''
    $settings.SteamBranch = ''
    if (Test-Path -LiteralPath $manifestPath) {
        $steamManifest = [IO.File]::ReadAllText($manifestPath)
        $settings.SteamBuildId = [regex]::Match($steamManifest, '"buildid"\s+"(\d+)"').Groups[1].Value
        $settings.SteamBranch = [regex]::Match($steamManifest, '"BetaKey"\s+"([^"]+)"').Groups[1].Value
        if ($settings.SteamBranch -ne 'public-beta') { Write-Warning "Expected public-beta; Steam branch is '$($settings.SteamBranch)'." }
    }
    return $settings
}

function Get-SourceFingerprint {
    $paths = @('HeavySilent.csproj', 'HeavySilent.json', 'Directory.Build.props', 'global.json', 'NuGet.Config',
        'packages.Debug.lock.json', 'packages.Release.lock.json', 'project.godot', 'export_presets.cfg',
        'THIRD_PARTY_NOTICES.md', 'build.ps1') | ForEach-Object { 'mod/' + $_ }
    $paths += Get-ChildItem -LiteralPath (Join-Path $modRoot 'HeavySilentCode') -Recurse -File |
        Where-Object { $_.Extension -eq '.cs' } | ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1) }
    $paths += Get-ChildItem -LiteralPath (Join-Path $modRoot 'HeavySilent') -Recurse -File |
        ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1) }
    $paths += Get-ChildItem -LiteralPath (Join-Path $repoRoot 'scripts') -File |
        Where-Object { $_.Extension -in @('.ps1', '.py', '.gd') } |
        ForEach-Object { $_.FullName.Substring($repoRoot.Length + 1) }
    $lines = foreach ($path in ($paths | Sort-Object)) {
        $path.Replace('\', '/') + ':' + (Get-Sha256 (Join-Path $repoRoot $path))
    }
    $hash = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($hash.ComputeHash($utf8.GetBytes(($lines -join "`n"))))).Replace('-', '').ToLowerInvariant() }
    finally { $hash.Dispose() }
}

function Build-Mod($Environment, [string]$BuildAction = 'build') {
    Invoke-Logged $Environment.PowerShellPath @('-NoProfile', '-File', (Join-Path $modRoot 'build.ps1'),
        '-Action', $BuildAction, '-Configuration', $Configuration, '-Sts2Path', $Environment.Sts2Path,
        '-GodotPath', $Environment.GodotPath, '-OutputRoot', $artifactRoot) 'mod-build.log'
}

function Test-PackInEngine($Environment, [string]$PackPath) {
    $checkDir = Join-Path $repoRoot '.cache\pack-check'
    Write-Utf8 (Join-Path $checkDir 'project.godot') "config_version=5`n"
    Invoke-Logged $Environment.GodotPath @('--headless', '--path', $checkDir, '--script',
        (Join-Path $repoRoot 'scripts\verify_pack.gd'), '--', $PackPath) 'verify-pack.log'
}

try {
    Push-Location $repoRoot
    switch ($Action) {
        { $_ -in @('test-setup', 'test-start', 'test-stop', 'test') } {
            $testTools = Join-Path $repoRoot 'tools\game-testing'
            $testPython = Join-Path $testTools '.venv\Scripts\python.exe'
            if ($Action -eq 'test-setup') { $testPython = 'python' }
            elseif (!(Test-Path -LiteralPath $testPython)) { throw 'Run test-setup once to prepare the test tools.' }
            if ($Action -eq 'test') {
                if ($TaskFile -and $Suite) { throw 'Choose either -TaskFile (Agent task) or -Suite (optional regression).' }
                if ($TaskFile) {
                    $arguments = @((Join-Path $testTools 'tasks.py'), '--file', $TaskFile, '--configuration', $Configuration, '--local-config', $LocalConfig)
                }
                elseif ($Suite) {
                    $arguments = @((Join-Path $testTools 'runner.py'), '--suite', $Suite, '--configuration', $Configuration, '--local-config', $LocalConfig)
                    if ($KeepOpen) { $arguments += '--keep-open' }
                }
                else { throw 'Use test -TaskFile <task.json> for Agent testing, or explicitly select -Suite smoke|regression.' }
            }
            else {
                $arguments = @((Join-Path $testTools 'session.py'), $Action.Substring(5), '--configuration', $Configuration, '--local-config', $LocalConfig)
            }
            if ($Language) { $arguments += @('--language', $Language) }
            & $testPython @arguments
            if ($LASTEXITCODE -ne 0) { throw 'Game testing command failed. See artifacts/game-tests in the primary worktree for evidence.' }
        }
        'doctor' {
            $environment = Get-Environment
            [pscustomobject]$environment | Format-List
            if ($CheckLatest) {
                # Public SteamCMD metadata mirror; local Steam remains the update authority.
                & python (Join-Path $PSScriptRoot 'check_branch.py') $environment.SteamBuildId
                if ($LASTEXITCODE -ne 0) { throw 'Branch metadata check failed; see output. No installed files were changed.' }
            }
        }
        'build' { Build-Mod (Get-Environment) }
        'package' {
            [IO.Directory]::CreateDirectory($packageRoot) | Out-Null
            # Absence of this completion record prevents deploying an interrupted package.
            if (Test-Path -LiteralPath $packageInfoPath) { Remove-Item -LiteralPath $packageInfoPath }
            $environment = Get-Environment
            & python (Join-Path $PSScriptRoot 'verify.py')
            if ($LASTEXITCODE -ne 0) { throw 'Source verification failed.' }
            $before = Get-SourceFingerprint
            Build-Mod $environment 'package'
            $modOutput = Join-Path $packageRoot 'HeavySilent'
            $packPath = Join-Path $modOutput 'HeavySilent.pck'
            & python (Join-Path $PSScriptRoot 'verify.py') --pack $packPath
            if ($LASTEXITCODE -ne 0) { throw 'Pack verification failed.' }
            Test-PackInEngine $environment $packPath
            if ((Get-SourceFingerprint) -ne $before) { throw 'Sources changed during packaging; run package again.' }
            $files = @{}
            foreach ($file in (Get-ChildItem -LiteralPath $modOutput -File)) { $files[$file.Name] = Get-Sha256 $file.FullName }
            $info = [ordered]@{
                packaged_at_utc = [DateTime]::UtcNow.ToString('o'); configuration = $Configuration
                source_fingerprint = $before; game_version = $environment.GameVersion
                steam_build_id = $environment.SteamBuildId; game_sha256 = $environment.GameHash
                baselib_version = $environment.BaseLibVersion; baselib_sha256 = $environment.BaseLibHash
                sdk = $environment.Sdk; godot = $environment.GodotVersion
                powershell = $environment.PowerShellVersion; files = $files
            }
            Write-Utf8 $packageInfoPath ($info | ConvertTo-Json -Depth 5)
            Write-Host "Package ready: $modOutput"
        }
        'deploy' {
            $environment = Get-Environment
            if (!(Test-Path -LiteralPath $packageInfoPath)) { throw 'No completed package. Run package first.' }
            $info = Read-Json $packageInfoPath
            if ($info.source_fingerprint -ne (Get-SourceFingerprint)) { throw 'Package is stale. Run package again.' }
            if ($info.game_sha256 -ne $environment.GameHash -or $info.baselib_sha256 -ne $environment.BaseLibHash) {
                throw 'Game or BaseLib changed after packaging. Rebuild, revalidate and package again.'
            }
            if (Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue) { throw 'Close Slay the Spire 2 before deploying.' }
            $source = Join-Path $packageRoot 'HeavySilent'
            foreach ($required in @('HeavySilent.dll', 'HeavySilent.pck', 'HeavySilent.json')) {
                if ($required -notin $info.files.PSObject.Properties.Name) { throw "Package record is incomplete: $required" }
            }
            foreach ($property in $info.files.PSObject.Properties) {
                if ($property.Name -notin @('HeavySilent.dll', 'HeavySilent.pck', 'HeavySilent.json', 'HeavySilent.pdb', 'THIRD_PARTY_NOTICES.md')) {
                    throw "Unexpected package file: $($property.Name)"
                }
                if ((Get-Sha256 (Join-Path $source $property.Name)) -ne $property.Value) { throw "Package hash mismatch: $($property.Name)" }
            }
            $destination = [IO.Path]::GetFullPath((Join-Path $environment.Sts2Path 'mods\HeavySilent'))
            [IO.Directory]::CreateDirectory($destination) | Out-Null
            foreach ($property in $info.files.PSObject.Properties) {
                Copy-Item -LiteralPath (Join-Path $source $property.Name) -Destination (Join-Path $destination $property.Name)
            }
            if ($info.configuration -eq 'Release' -and (Test-Path -LiteralPath (Join-Path $destination 'HeavySilent.pdb'))) {
                Remove-Item -LiteralPath (Join-Path $destination 'HeavySilent.pdb')
            }
            Write-Host "Deployed $($info.configuration) package to $destination"
        }
        'logs' {
            $gameLog = Join-Path ([Environment]::GetFolderPath('ApplicationData')) 'SlayTheSpire2\logs\godot.log'
            if (!(Test-Path -LiteralPath $gameLog)) { throw "No game log at $gameLog. In the game console, use: open logs" }
            Get-Content -LiteralPath $gameLog -Tail 200
        }
        'verify' {
            & python (Join-Path $PSScriptRoot 'verify.py')
            if ($LASTEXITCODE -ne 0) { throw 'Source verification failed.' }
            Build-Mod (Get-Environment)
            Write-Host 'Static checks and compilation passed. This does not assert in-game acceptance.'
        }
    }
}
catch { Write-Error $_ -ErrorAction Continue; exit 1 }
finally { Pop-Location }
