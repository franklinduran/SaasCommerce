param(
  [Parameter(Mandatory = $true)]
  [string]$RepoRoot,

  [Parameter(Mandatory = $true)]
  [string]$OutputPath
)

$ErrorActionPreference = "Stop"

$repoPath = (Resolve-Path $RepoRoot).Path
$reports = Get-ChildItem -Path (Join-Path $repoPath "backend/tests") -Recurse -Filter "coverage.cobertura.xml"
$coverageByFile = @{}
$lineCountsByFile = @{}

foreach ($report in $reports) {
  [xml]$coverage = Get-Content -Path $report.FullName

  foreach ($class in $coverage.coverage.packages.package.classes.class) {
    $fileName = [string]$class.filename

    if ([string]::IsNullOrWhiteSpace($fileName)) {
      continue
    }

    $candidatePath = if ([System.IO.Path]::IsPathRooted($fileName)) {
      $fileName
    } else {
      Join-Path $repoPath $fileName
    }

    if (-not (Test-Path -LiteralPath $candidatePath)) {
      continue
    }

    $fullPath = (Resolve-Path $candidatePath).Path

    if (-not $fullPath.StartsWith((Join-Path $repoPath "backend\src"), [StringComparison]::OrdinalIgnoreCase)) {
      continue
    }

    if (-not $lineCountsByFile.ContainsKey($fullPath)) {
      $lineCountsByFile[$fullPath] = (Get-Content -LiteralPath $fullPath).Count
    }

    if (-not $coverageByFile.ContainsKey($fullPath)) {
      $coverageByFile[$fullPath] = @{}
    }

    foreach ($line in $class.lines.line) {
      $lineNumber = [int]$line.number

      if ($lineNumber -lt 1 -or $lineNumber -gt $lineCountsByFile[$fullPath]) {
        continue
      }

      $covered = [int]$line.hits -gt 0

      if (-not $coverageByFile[$fullPath].ContainsKey($lineNumber)) {
        $coverageByFile[$fullPath][$lineNumber] = $covered
        continue
      }

      $coverageByFile[$fullPath][$lineNumber] = $coverageByFile[$fullPath][$lineNumber] -or $covered
    }
  }
}

$outputDirectory = Split-Path -Path $OutputPath -Parent

if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
  New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$settings = [System.Xml.XmlWriterSettings]::new()
$settings.Indent = $true
$settings.Encoding = [System.Text.UTF8Encoding]::new($false)

$writer = [System.Xml.XmlWriter]::Create($OutputPath, $settings)

try {
  $writer.WriteStartDocument()
  $writer.WriteStartElement("coverage")
  $writer.WriteAttributeString("version", "1")

  foreach ($filePath in ($coverageByFile.Keys | Sort-Object)) {
    $writer.WriteStartElement("file")
    $writer.WriteAttributeString("path", $filePath)

    foreach ($lineNumber in ($coverageByFile[$filePath].Keys | Sort-Object)) {
      $writer.WriteStartElement("lineToCover")
      $writer.WriteAttributeString("lineNumber", [string]$lineNumber)
      $writer.WriteAttributeString("covered", $coverageByFile[$filePath][$lineNumber].ToString().ToLowerInvariant())
      $writer.WriteEndElement()
    }

    $writer.WriteEndElement()
  }

  $writer.WriteEndElement()
  $writer.WriteEndDocument()
} finally {
  $writer.Dispose()
}
