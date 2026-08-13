[CmdletBinding()]
param(
    [ValidateSet('api', 'web', 'android', 'all')]
    [string]$Preset,

    [ValidateSet('https', 'http')]
    [string]$LaunchProfile = 'https'
)

$ErrorActionPreference = 'Stop'

$presetFile = Join-Path $PSScriptRoot '.mediavault-preset'
$validPresets = @('api', 'web', 'android', 'all')
$lastPreset = if (Test-Path -LiteralPath $presetFile) {
    (Get-Content -LiteralPath $presetFile -Raw).Trim().ToLowerInvariant()
}

if ($lastPreset -notin $validPresets) {
    $lastPreset = $null
}

if (-not $Preset) {
    Write-Host ''
    Write-Host 'MediaVault development launcher' -ForegroundColor Cyan
    Write-Host '  1. API only'
    Write-Host '  2. API + web'
    Write-Host '  3. API + Android'
    Write-Host '  4. API + web + Android'
    if ($lastPreset) {
        Write-Host "  L. Last preset ($lastPreset)"
    }
    Write-Host '  Q. Quit'
    Write-Host ''

    $selection = (Read-Host 'Choose a preset').Trim().ToLowerInvariant()
    $Preset = switch ($selection) {
        '1' { 'api' }
        '2' { 'web' }
        '3' { 'android' }
        '4' { 'all' }
        'l' {
            if (-not $lastPreset) { throw 'No previous preset has been saved yet.' }
            $lastPreset
        }
        'q' { return }
        default { throw "Unknown selection '$selection'." }
    }
}

$requiredPaths = @(
    (Join-Path $PSScriptRoot 'MediaVault.Api\media-vault-app.API\media-vault-app.API.csproj'),
    (Join-Path $PSScriptRoot 'MediaVault.Clients\package.json')
)

if ($requiredPaths.Where({ -not (Test-Path -LiteralPath $_) }).Count -gt 0) {
    throw 'The API or client repository is missing. Run ./setup.ps1 first.'
}

Set-Content -LiteralPath $presetFile -Value $Preset -NoNewline
$env:MEDIAVAULT_PRESET = $Preset

Write-Host "Starting the '$Preset' preset with Aspire..." -ForegroundColor Green
& dotnet run --project (Join-Path $PSScriptRoot 'MediaVault.AppHost') --launch-profile $LaunchProfile
exit $LASTEXITCODE
