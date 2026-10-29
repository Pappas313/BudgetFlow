<#
.SYNOPSIS
	Runs the BudgetFlow.Tests test suite and publishes a summary.
.PARAMETER RepoRoot
	Root path of the BudgetFlow repository.
.PARAMETER Configuration
	Build configuration to test. Defaults to Release.
.PARAMETER Filter
	Optional `dotnet test --filter` expression to run a subset of tests.
.EXAMPLE
	.\run-tests.ps1
.EXAMPLE
	.\run-tests.ps1 -Filter "FullyQualifiedName~BudgetVariance"
#>
[CmdletBinding()]
param(
	[string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
	[string]$Configuration = 'Release',
	[string]$Filter
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Initialize-Logging -ScriptName 'run-tests' | Out-Null

try {
	$testProject = Join-Path $RepoRoot 'BudgetFlow.Tests\BudgetFlow.Tests.csproj'
	$resultsDir = Join-Path $PSScriptRoot 'logs\test-results'

	Invoke-Step -StepName 'Validate test project exists' -Action {
		if (-not (Test-Path -LiteralPath $testProject)) {
			throw "Test project not found at $testProject"
		}
	}

	Invoke-Step -StepName 'Run test suite' -Action {
		if (-not (Test-Path -LiteralPath $resultsDir)) {
			New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null
		}

		Push-Location $RepoRoot
		try {
			$testArgs = @(
				'test', $testProject,
				'--configuration', $Configuration,
				'--logger', 'trx;LogFileName=BudgetFlow.Tests.trx',
				'--results-directory', $resultsDir,
				'--nologo'
			)
			if ($Filter) {
				$testArgs += @('--filter', $Filter)
			}

			Write-Log -Message "Running: dotnet $($testArgs -join ' ')"
			& dotnet @testArgs
			$testExitCode = $LASTEXITCODE

			if ($testExitCode -ne 0) {
				throw "dotnet test reported failures (exit code $testExitCode). See results in $resultsDir"
			}
		}
		finally {
			Pop-Location
		}
	}

	Write-Log -Message "Test run completed successfully. Results: $resultsDir" -Level SUCCESS
	exit $script:ExitCodeSuccess
}
catch {
	Write-Log -Message "Test run failed: $($_.Exception.Message)" -Level ERROR
	exit $script:ExitCodeGeneralError
}
