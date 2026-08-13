[CmdletBinding()]
param(
    [switch]$InstallDependencies
)

$ErrorActionPreference = 'Stop'

$workspaceRoot = $PSScriptRoot
$repositories = @(
    @{
        Name = 'MediaVault.Api'
        Url = 'https://github.com/Megaraz/MediaVault.Api.git'
    },
    @{
        Name = 'MediaVault.Clients'
        Url = 'https://github.com/Megaraz/MediaVault.Clients.git'
    }
)

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    throw 'Git is required but was not found on PATH.'
}

foreach ($repository in $repositories) {
    $destination = Join-Path $workspaceRoot $repository.Name

    if (Test-Path -LiteralPath $destination) {
        $gitDirectory = Join-Path $destination '.git'
        if (-not (Test-Path -LiteralPath $gitDirectory)) {
            throw "'$destination' already exists but is not a Git repository."
        }

        Write-Host "$($repository.Name) already exists; skipping clone." -ForegroundColor DarkGray
        continue
    }

    Write-Host "Cloning $($repository.Name)..." -ForegroundColor Cyan
    & git clone $repository.Url $destination
    if ($LASTEXITCODE -ne 0) {
        throw "Git clone failed for $($repository.Name)."
    }
}

if ($InstallDependencies) {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw '.NET SDK is required for dependency installation but was not found on PATH.'
    }
    if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
        throw 'npm is required for dependency installation but was not found on PATH.'
    }

    Write-Host 'Restoring the API and AppHost...' -ForegroundColor Cyan
    & dotnet restore (Join-Path $workspaceRoot 'MediaVault.Api\media-vault-app.slnx')
    if ($LASTEXITCODE -ne 0) { throw 'API restore failed.' }

    & dotnet restore (Join-Path $workspaceRoot 'MediaVault.slnx')
    if ($LASTEXITCODE -ne 0) { throw 'AppHost restore failed.' }

    Write-Host 'Installing client dependencies...' -ForegroundColor Cyan
    & npm ci --prefix (Join-Path $workspaceRoot 'MediaVault.Clients')
    if ($LASTEXITCODE -ne 0) { throw 'Client dependency installation failed.' }
}

Write-Host 'MediaVault workspace is ready.' -ForegroundColor Green
Write-Host 'Run ./run.ps1 to choose what Aspire should start.'
