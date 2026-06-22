# Comprehensive protocol validation test for CLI server mode
# Tests various scenarios: success, errors, multiple requests, shutdown

Write-Host "CLI Server Mode - Protocol Validation Test" -ForegroundColor Cyan
Write-Host "==========================================`n" -ForegroundColor Cyan

$testsPassed = 0
$testsFailed = 0

function Test-ServerMode {
    param(
        [string]$TestName,
        [string]$Requests,
        [int]$ExpectedResponses,
        [scriptblock]$Validator
    )

    Write-Host "Test: $TestName" -ForegroundColor Yellow

    try {
        $output = $Requests | & "tools\SqlAnalyzerCli\bin\Release\net10.0\ErikEJ.TSQLAnalyzerCli.exe" --server-mode 2>&1
        $jsonLines = $output -split "`n" | Where-Object { $_.StartsWith("{") }

        if ($jsonLines.Count -ne $ExpectedResponses) {
            Write-Host "  FAILED: Expected $ExpectedResponses responses, got $($jsonLines.Count)" -ForegroundColor Red
            $script:testsFailed++
            return
        }

        $responses = $jsonLines | ForEach-Object { $_ | ConvertFrom-Json }

        if (& $Validator $responses) {
            Write-Host "  PASSED" -ForegroundColor Green
            $script:testsPassed++
        }
        else {
            Write-Host "  FAILED: Validation failed" -ForegroundColor Red
            $script:testsFailed++
        }
    }
    catch {
        Write-Host "  FAILED: Exception - $_" -ForegroundColor Red
        $script:testsFailed++
    }

    Write-Host ""
}

# Test 1: Basic analyze request
$simpleFile = (Resolve-Path "tools\SqlAnalyzerCli\testfiles\simple.sql").Path -replace '\\', '\\'
$test1 = @"
{"id":"test-001","command":"analyze","path":"$simpleFile"}
{"id":"shutdown","command":"shutdown"}
"@

Test-ServerMode -TestName "Basic analyze with .sql file" -Requests $test1 -ExpectedResponses 2 -Validator {
    param($responses)
    $responses[0].status -eq "success" -and 
    $responses[0].id -eq "test-001" -and 
    $responses[0].problems.Count -gt 0 -and
    $responses[1].status -eq "shutdown"
}

# Test 2: Analyze with rules filter
$test2 = @"
{"id":"test-002","command":"analyze","path":"$simpleFile","rules":"Rules:-SRD*"}
{"id":"shutdown","command":"shutdown"}
"@

Test-ServerMode -TestName "Analyze with rules filter" -Requests $test2 -ExpectedResponses 2 -Validator {
    param($responses)
    $responses[0].status -eq "success" -and 
    $responses[0].id -eq "test-002" -and
    # Rules filter is applied, result should still be successful
    $responses[0].problems -ne $null
}

# Test 3: Analyze DACPAC file
$dacpacFile = (Resolve-Path "tools\SqlAnalyzerCli\testfiles\Chinook.dacpac").Path -replace '\\', '\\'
$test3 = @"
{"id":"test-003","command":"analyze","path":"$dacpacFile"}
{"id":"shutdown","command":"shutdown"}
"@

Test-ServerMode -TestName "Analyze DACPAC file" -Requests $test3 -ExpectedResponses 2 -Validator {
    param($responses)
    $responses[0].status -eq "success" -and 
    $responses[0].id -eq "test-003" -and 
    $responses[0].problems.Count -eq 45  # Expected baseline
}

# Test 4: Error - invalid file path
$test4 = @"
{"id":"test-004","command":"analyze","path":"C:\\\\nonexistent\\\\file.sql"}
{"id":"shutdown","command":"shutdown"}
"@

Test-ServerMode -TestName "Error handling - nonexistent file" -Requests $test4 -ExpectedResponses 2 -Validator {
    param($responses)
    $responses[0].status -eq "error" -and 
    $responses[0].id -eq "test-004" -and 
    $responses[0].error -like "*not found*"
}

# Test 5: Error - missing path
$test5 = @"
{"id":"test-005","command":"analyze"}
{"id":"shutdown","command":"shutdown"}
"@

Test-ServerMode -TestName "Error handling - missing path" -Requests $test5 -ExpectedResponses 2 -Validator {
    param($responses)
    $responses[0].status -eq "error" -and 
    $responses[0].id -eq "test-005" -and 
    $responses[0].error -like "*required*"
}

# Test 6: Unknown command
$test6 = @"
{"id":"test-006","command":"unknown"}
{"id":"shutdown","command":"shutdown"}
"@

Test-ServerMode -TestName "Error handling - unknown command" -Requests $test6 -ExpectedResponses 2 -Validator {
    param($responses)
    $responses[0].status -eq "error" -and 
    $responses[0].id -eq "test-006" -and 
    $responses[0].error -like "*Unknown command*"
}

# Test 7: Multiple sequential analyses
$test7 = @"
{"id":"test-007a","command":"analyze","path":"$simpleFile"}
{"id":"test-007b","command":"analyze","path":"$simpleFile"}
{"id":"test-007c","command":"analyze","path":"$simpleFile"}
{"id":"shutdown","command":"shutdown"}
"@

Test-ServerMode -TestName "Multiple sequential analyses" -Requests $test7 -ExpectedResponses 4 -Validator {
    param($responses)
    $responses[0].status -eq "success" -and 
    $responses[0].id -eq "test-007a" -and 
    $responses[1].status -eq "success" -and 
    $responses[1].id -eq "test-007b" -and 
    $responses[2].status -eq "success" -and 
    $responses[2].id -eq "test-007c" -and
    $responses[3].status -eq "shutdown"
}

# Test 8: SQL version parameter
$test8 = @"
{"id":"test-008","command":"analyze","path":"$simpleFile","sqlVersion":"SqlAzure"}
{"id":"shutdown","command":"shutdown"}
"@

Test-ServerMode -TestName "SQL version parameter (SqlAzure)" -Requests $test8 -ExpectedResponses 2 -Validator {
    param($responses)
    $responses[0].status -eq "success" -and 
    $responses[0].id -eq "test-008" -and 
    $responses[0].problems.Count -gt 0
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Results:" -ForegroundColor Cyan
Write-Host "  Passed: $testsPassed" -ForegroundColor Green
Write-Host "  Failed: $testsFailed" -ForegroundColor $(if ($testsFailed -eq 0) { "Green" } else { "Red" })
Write-Host "========================================" -ForegroundColor Cyan

if ($testsFailed -eq 0) {
    Write-Host "`nAll tests passed! Server mode protocol is working correctly." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "`nSome tests failed. Please review the output above." -ForegroundColor Red
    exit 1
}
