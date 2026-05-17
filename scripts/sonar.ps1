param(
  [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

function Invoke-NativeCommand {
  param(
    [Parameter(Mandatory = $true)]
    [scriptblock]$Command,

    [Parameter(Mandatory = $true)]
    [string]$Description
  )

  & $Command

  if ($LASTEXITCODE -ne 0) {
    throw "$Description failed with exit code $LASTEXITCODE."
  }
}

function Assert-EnvironmentVariable {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Name
  )

  $value = [Environment]::GetEnvironmentVariable($Name, "Process")

  if ([string]::IsNullOrWhiteSpace($value)) {
    $value = [Environment]::GetEnvironmentVariable($Name, "User")
  }

  if ([string]::IsNullOrWhiteSpace($value)) {
    throw "Missing environment variable '$Name'. Configure it once with [Environment]::SetEnvironmentVariable('$Name', 'value', 'User') and open a new terminal."
  }

  return $value
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$solutionPath = Join-Path $repoRoot "SaasCommerce.slnx"
$coverageExclusions = @(
  "**/Contracts/**",
  "**/*Request.cs",
  "**/*Response.cs",
  "**/*Command.cs",
  "**/*Query.cs",
  "**/*Errors.cs",
  "**/*Marker.cs",
  "**/*AssemblyReference.cs",
  "**/Configurations/**",
  "**/Migrations/**",
  "**/DependencyInjection.cs",
  "**/Program.cs",
  "**/Worker.cs",
  "**/Endpoints/**",
  "backend/src/**/Contracts/**",
  "backend/src/**/*Request.cs",
  "backend/src/**/*Response.cs",
  "backend/src/**/*Command.cs",
  "backend/src/**/*Query.cs",
  "backend/src/**/*Errors.cs",
  "backend/src/**/*Marker.cs",
  "backend/src/**/*AssemblyReference.cs",
  "backend/src/**/Configurations/**",
  "backend/src/**/Migrations/**",
  "backend/src/**/DependencyInjection.cs",
  "backend/src/**/Program.cs",
  "backend/src/**/Worker.cs",
  "backend/src/**/Endpoints/**",
  "frontend/**"
) -join ","
$sourceExclusions = @(
  "**/Migrations/**"
) -join ","
$sonarHostUrl = Assert-EnvironmentVariable "SONAR_HOST_URL"
$sonarProjectKey = Assert-EnvironmentVariable "SONAR_PROJECT_KEY"
$sonarToken = Assert-EnvironmentVariable "SONAR_TOKEN"
$sonarOrganization = [Environment]::GetEnvironmentVariable("SONAR_ORGANIZATION", "Process")

if ([string]::IsNullOrWhiteSpace($sonarOrganization)) {
  $sonarOrganization = [Environment]::GetEnvironmentVariable("SONAR_ORGANIZATION", "User")
}

if ([string]::IsNullOrWhiteSpace($env:NUGET_PACKAGES) -and -not [string]::IsNullOrWhiteSpace($env:USERPROFILE)) {
  $defaultNuGetPackages = Join-Path $env:USERPROFILE ".nuget\packages"

  if (Test-Path $defaultNuGetPackages) {
    $env:NUGET_PACKAGES = $defaultNuGetPackages
  }
}

Set-Location $repoRoot

$beginArgs = @(
  "begin",
  "/k:$sonarProjectKey",
  "/d:sonar.host.url=$sonarHostUrl",
  "/d:sonar.token=$sonarToken",
  "/d:sonar.login=$sonarToken",
  "/d:sonar.exclusions=$sourceExclusions",
  "/d:sonar.cs.vscoveragexml.reportsPaths=coverage/dotnet-coverage.xml",
  "/d:sonar.coverage.exclusions=$coverageExclusions",
  "/d:sonar.typescript.tsconfigPath=frontend/tsconfig.sonar.json"
)

if (-not [string]::IsNullOrWhiteSpace($sonarOrganization)) {
  $beginArgs += "/o:$sonarOrganization"
}

Invoke-NativeCommand { dotnet-sonarscanner @beginArgs } "Sonar begin"
Invoke-NativeCommand { dotnet restore $solutionPath } "dotnet restore"
Invoke-NativeCommand { dotnet build $solutionPath --no-restore -m:1 /nr:false -v minimal } "dotnet build"

if (-not $SkipTests) {
  Invoke-NativeCommand {
    dotnet-coverage collect `
      -f xml `
      -o (Join-Path $repoRoot "coverage/dotnet-coverage.xml") `
      dotnet test $solutionPath --no-build -m:1 /nr:false -v minimal
  } "dotnet test with coverage"
}

Invoke-NativeCommand {
  dotnet-sonarscanner end "/d:sonar.token=$sonarToken" "/d:sonar.login=$sonarToken"
} "Sonar end"
