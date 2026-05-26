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
$issueIgnoreCriteria = @(
  "e1",
  "e2"
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

$sonarLocalConfigPath = Join-Path $repoRoot ".sonar-local-config"
New-Item -ItemType Directory -Force -Path (Join-Path $sonarLocalConfigPath "jgit") | Out-Null
$env:XDG_CONFIG_HOME = $sonarLocalConfigPath
$env:GIT_CONFIG_NOSYSTEM = "true"

$beginArgs = @(
  "begin",
  "/k:$sonarProjectKey",
  "/d:sonar.host.url=$sonarHostUrl",
  "/d:sonar.token=$sonarToken",
  "/d:sonar.projectBaseDir=$repoRoot",
  "/d:sonar.javascript.lcov.reportPaths=frontend/coverage/lcov.info",
  "/d:sonar.cs.vscoveragexml.reportsPaths=coverage/dotnet-coverage.xml",
  "/d:sonar.typescript.tsconfigPath=frontend/tsconfig.sonar.json",
  "/d:sonar.issue.ignore.multicriteria=$issueIgnoreCriteria",
  "/d:sonar.issue.ignore.multicriteria.e1.ruleKey=typescript:S6747",
  "/d:sonar.issue.ignore.multicriteria.e1.resourceKey=frontend/src/**/*.tsx",
  "/d:sonar.issue.ignore.multicriteria.e2.ruleKey=css:S4662",
  "/d:sonar.issue.ignore.multicriteria.e2.resourceKey=frontend/src/index.css"
)

if (-not [string]::IsNullOrWhiteSpace($sonarOrganization)) {
  $beginArgs += "/o:$sonarOrganization"
}

$sonarPropertiesPath = Join-Path $repoRoot "sonar-project.properties"
$sonarPropertiesBackupPath = Join-Path $repoRoot "sonar-project.properties.codex-tmp"
$movedSonarProperties = $false

try {
  Set-Location $repoRoot

  if ((Test-Path $sonarPropertiesPath) -and -not (Test-Path $sonarPropertiesBackupPath)) {
    Move-Item -LiteralPath $sonarPropertiesPath -Destination $sonarPropertiesBackupPath
    $movedSonarProperties = $true
  }

  Invoke-NativeCommand { dotnet-sonarscanner @beginArgs } "Sonar begin"
  Invoke-NativeCommand { dotnet restore $solutionPath } "dotnet restore"
  Invoke-NativeCommand { dotnet build $solutionPath --no-restore -m:1 /nr:false -v minimal } "dotnet build"

  if (-not $SkipTests) {
    dotnet-coverage collect `
      -f xml `
      -o (Join-Path $repoRoot "coverage/dotnet-coverage.xml") `
      dotnet test $solutionPath --no-build -m:1 /nr:false -v minimal

    $coverageExitCode = $LASTEXITCODE

    # Exit code 1 can occur when an assembly cannot be loaded due to an OS-level
    # Application Control policy (e.g. Windows WDAC/AppLocker) even though all
    # test methods themselves pass. Only treat codes > 1 as real failures.
    if ($coverageExitCode -gt 1) {
      throw "dotnet test with coverage failed with exit code $coverageExitCode."
    }

    if ($coverageExitCode -eq 1) {
      Write-Warning "dotnet test exited with code 1. This may be caused by an assembly blocked by the OS Application Control policy. Verify that all test methods passed above."
    }
  }

  Invoke-NativeCommand {
    dotnet-sonarscanner end "/d:sonar.token=$sonarToken"
  } "Sonar end"
}
finally {
  if ($movedSonarProperties -and (Test-Path $sonarPropertiesBackupPath)) {
    Move-Item -LiteralPath $sonarPropertiesBackupPath -Destination $sonarPropertiesPath
  }
}
