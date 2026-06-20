# CLI Startup Performance Benchmark Script
# This script measures the launch time of the tsqlanalyze CLI tool

$ErrorActionPreference = "Stop"

Write-Host "=== T-SQL Analyzer CLI Launch Performance Benchmark ===" -ForegroundColor Cyan
Write-Host ""

# Number of iterations for averaging
$iterations = 10
$warmupIterations = 3

Write-Host "Benchmark Configuration:" -ForegroundColor Yellow
Write-Host "  - Warmup iterations: $warmupIterations"
Write-Host "  - Measured iterations: $iterations"
Write-Host ""

# Function to measure command execution time
function Measure-CommandStartup {
    param(
        [string]$Command,
        [string]$Arguments
    )

    $measure = Measure-Command {
        $process = Start-Process -FilePath $Command -ArgumentList $Arguments -NoNewWindow -Wait -PassThru
        $null = $process
    }

    return $measure.TotalMilliseconds
}

# Check if tool is installed
$toolInstalled = $false
try {
    $null = Get-Command tsqlanalyze -ErrorAction Stop
    $toolInstalled = $true
    Write-Host "✓ tsqlanalyze tool found in PATH" -ForegroundColor Green
}
catch {
    Write-Host "✗ tsqlanalyze tool NOT found in PATH" -ForegroundColor Red
    Write-Host "  Install with: dotnet tool install -g ErikEJ.DacFX.TSQLAnalyzer.Cli" -ForegroundColor Yellow
}

Write-Host ""

# Test 1: Using global tool (if installed)
if ($toolInstalled) {
    Write-Host "Test 1: Global Tool - tsqlanalyze --version" -ForegroundColor Cyan
    Write-Host "  Running warmup iterations..." -ForegroundColor Gray

    for ($i = 1; $i -le $warmupIterations; $i++) {
        $null = Measure-CommandStartup -Command "tsqlanalyze" -Arguments "--version"
    }

    Write-Host "  Running measured iterations..." -ForegroundColor Gray
    $times = @()
    for ($i = 1; $i -le $iterations; $i++) {
        $time = Measure-CommandStartup -Command "tsqlanalyze" -Arguments "--version"
        $times += $time
        Write-Host "    Iteration $i : $([math]::Round($time, 2)) ms" -ForegroundColor Gray
    }

    $avgTime = ($times | Measure-Object -Average).Average
    $minTime = ($times | Measure-Object -Minimum).Minimum
    $maxTime = ($times | Measure-Object -Maximum).Maximum

    Write-Host ""
    Write-Host "  Results:" -ForegroundColor Yellow
    Write-Host "    Average: $([math]::Round($avgTime, 2)) ms"
    Write-Host "    Min:     $([math]::Round($minTime, 2)) ms"
    Write-Host "    Max:     $([math]::Round($maxTime, 2)) ms"
    Write-Host ""
}

# Test 2: Using dnx (simulates VS/SSMS extension usage)
Write-Host "Test 2: DNX Execution - dnx ErikEJ.DacFX.TSQLAnalyzer.Cli --yes -- --version" -ForegroundColor Cyan

# Check if the local package exists
$localNupkg = Get-ChildItem "tools/SqlAnalyzerCli/bin/Release/*.nupkg" -ErrorAction SilentlyContinue | Select-Object -First 1

if ($localNupkg) {
    Write-Host "  Found local package: $($localNupkg.Name)" -ForegroundColor Green
    Write-Host "  Testing with local package..." -ForegroundColor Gray

    # For dnx testing, we need to use the installed version or provide a source
    # This is a placeholder for actual dnx testing
    Write-Host "  Note: Full dnx testing requires package installation" -ForegroundColor Yellow
}
else {
    Write-Host "  No local package found. Run 'dotnet pack' first." -ForegroundColor Yellow
}

Write-Host ""

# Test 3: Direct executable test (if built locally)
$localExeNet10 = "tools/SqlAnalyzerCli/bin/Release/net10.0/ErikEJ.TSQLAnalyzerCli.exe"
$localExeNet8 = "tools/SqlAnalyzerCli/bin/Release/net8.0/ErikEJ.TSQLAnalyzerCli.exe"

$testExe = $null
if (Test-Path $localExeNet10) {
    $testExe = $localExeNet10
    $framework = "net10.0"
}
elseif (Test-Path $localExeNet8) {
    $testExe = $localExeNet8
    $framework = "net8.0"
}

if ($testExe) {
    Write-Host "Test 3: Local Build - $framework" -ForegroundColor Cyan
    Write-Host "  Running warmup iterations..." -ForegroundColor Gray

    for ($i = 1; $i -le $warmupIterations; $i++) {
        $null = Measure-CommandStartup -Command $testExe -Arguments "--version"
    }

    Write-Host "  Running measured iterations..." -ForegroundColor Gray
    $times = @()
    for ($i = 1; $i -le $iterations; $i++) {
        $time = Measure-CommandStartup -Command $testExe -Arguments "--version"
        $times += $time
        Write-Host "    Iteration $i : $([math]::Round($time, 2)) ms" -ForegroundColor Gray
    }

    $avgTime = ($times | Measure-Object -Average).Average
    $minTime = ($times | Measure-Object -Minimum).Minimum
    $maxTime = ($times | Measure-Object -Maximum).Maximum

    Write-Host ""
    Write-Host "  Results:" -ForegroundColor Yellow
    Write-Host "    Average: $([math]::Round($avgTime, 2)) ms"
    Write-Host "    Min:     $([math]::Round($minTime, 2)) ms"
    Write-Host "    Max:     $([math]::Round($maxTime, 2)) ms"
    Write-Host ""
}
else {
    Write-Host "Test 3: Local Build - SKIPPED (no executable found)" -ForegroundColor Yellow
    Write-Host "  Build the project first with: dotnet build tools/SqlAnalyzerCli/SqlAnalyzerCli.csproj -c Release" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "=== Benchmark Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Performance Tips:" -ForegroundColor Yellow
Write-Host "  - ReadyToRun: Adds native pre-compiled code for 20-40% faster startup"
Write-Host "  - Workstation GC: Optimized for short-lived processes like CLI tools"
Write-Host "  - Warm start: OS caching improves subsequent executions"
Write-Host ""
Write-Host "To compare with baseline (without optimizations):" -ForegroundColor Yellow
Write-Host "  1. Remove <PublishReadyToRun> from .csproj"
Write-Host "  2. Rebuild and repackage"
Write-Host "  3. Reinstall tool"
Write-Host "  4. Run benchmark again"
Write-Host ""
