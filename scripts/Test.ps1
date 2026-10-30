<#
.SYNOPSIS
    Runs every test project with coverage, builds the coverage report and enforces the threshold.

.DESCRIPTION
    The same script runs locally (Windows PowerShell 5.1 or pwsh) and in .github/workflows/ci.yml
    (pwsh on ubuntu-latest), so "green on my machine" and "green in CI" mean the same thing.

    Test projects are run one by one instead of via the solution: building the solution would
    also build the app for Android/iOS, which the tests do not need.

.PARAMETER MinimumLineCoverage
    Fails if the line coverage of the measured code (see coverage.runsettings) is lower.

.PARAMETER Filter
    VSTest filter. By default the contract tests against a real PHP backend are excluded; they run
    in the CI job "backend-contract" (or locally with -Filter "Category=PhpBackend").

.EXAMPLE
    ./scripts/Test.ps1
.EXAMPLE
    ./scripts/Test.ps1 -Configuration Release -MinimumLineCoverage 90
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Debug',
    [double] $MinimumLineCoverage = 90,
    [string] $Filter = 'Category!=PhpBackend'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$results = Join-Path $root 'TestResults'
$report = Join-Path $root 'coverage'

Push-Location $root
try {
    Remove-Item $results, $report -Recurse -Force -ErrorAction SilentlyContinue

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore failed.' }

    $projects = Get-ChildItem (Join-Path $root 'tests') -Recurse -Filter '*.Tests.csproj'
    $projects += Get-ChildItem (Join-Path $root 'tests') -Recurse -Filter '*.IntegrationTests.csproj'
    $projects += Get-ChildItem (Join-Path $root 'tests') -Recurse -Filter '*.ArchitectureTests.csproj'

    $failed = @()
    foreach ($project in $projects) {
        Write-Host "==> $($project.BaseName)" -ForegroundColor Cyan
        dotnet test $project.FullName `
            --configuration $Configuration `
            --settings (Join-Path $root 'coverage.runsettings') `
            --collect 'XPlat Code Coverage' `
            --results-directory $results `
            --logger 'trx' `
            --filter $Filter
        if ($LASTEXITCODE -ne 0) { $failed += $project.BaseName }
    }

    # Report even when tests failed: that is when it is most useful.
    dotnet reportgenerator `
        "-reports:$results/**/coverage.cobertura.xml" `
        "-targetdir:$report" `
        '-reporttypes:Html;Cobertura;JsonSummary;MarkdownSummaryGithub;TextSummary' `
        '-title:BurgerTasters'
    if ($LASTEXITCODE -ne 0) { throw 'reportgenerator failed.' }

    Get-Content (Join-Path $report 'Summary.txt') -TotalCount 12 | Write-Host

    if ($failed.Count -gt 0) {
        throw "Tests failed in: $($failed -join ', ')"
    }

    $summary = Get-Content (Join-Path $report 'Summary.json') -Raw | ConvertFrom-Json
    $lineCoverage = [double]$summary.summary.linecoverage
    if ($lineCoverage -lt $MinimumLineCoverage) {
        throw ("Line coverage {0:N1} % is below the minimum of {1:N1} %. Report: {2}" -f $lineCoverage, $MinimumLineCoverage, (Join-Path $report 'index.html'))
    }

    Write-Host ("Line coverage {0:N1} % (minimum {1:N1} %). Report: {2}" -f $lineCoverage, $MinimumLineCoverage, (Join-Path $report 'index.html')) -ForegroundColor Green
}
finally {
    Pop-Location
}
