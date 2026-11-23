<#
.SYNOPSIS
    Runs all tests with code coverage and creates an HTML report in TestResults\coverage-report.
.PARAMETER NoOpen
    Do not open the report in the browser afterwards.
#>
param(
    [switch]$NoOpen
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$coverageDir = Join-Path $PSScriptRoot 'TestResults\coverage'
$reportDir = Join-Path $PSScriptRoot 'TestResults\coverage-report'

# Remove results from earlier runs, otherwise they are merged into the report
foreach ($dir in $coverageDir, $reportDir) {
    if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
}

dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed' }

dotnet test UspApp.sln --settings coverage.runsettings --collect:'XPlat Code Coverage' --results-directory $coverageDir
$testExitCode = $LASTEXITCODE

dotnet reportgenerator "-reports:$coverageDir\**\coverage.cobertura.xml" "-targetdir:$reportDir" '-reporttypes:Html;TextSummary'
if ($LASTEXITCODE -ne 0) { throw 'reportgenerator failed' }

Get-Content (Join-Path $reportDir 'Summary.txt') | Select-Object -First 20

if (-not $NoOpen) { Start-Process (Join-Path $reportDir 'index.html') }

if ($testExitCode -ne 0) {
    Write-Warning 'Some tests failed, coverage may be incomplete'
    exit $testExitCode
}
