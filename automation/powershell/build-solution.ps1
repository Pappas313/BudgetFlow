<#
.SYNOPSIS
	Builds the BudgetFlow .NET solution and (optionally) the React frontend.
.PARAMETER RepoRoot
	Root path of the BudgetFlow repository.
.PARAMETER Configuration
	Build configuration (Debug/Release). Defaults to Release.
.PARAMETER SkipFrontend
	Skip building the React app.
.PARAMETER SkipTests
	Skip building the test project (still part of solution build unless excluded via -Configuration filters).
.EXAMPLE
	.\build-solution.ps1 -Configuration Release
#>
[CmdletBinding()]
param(
	[string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
	[string]$Configuration = 'Release',
	[switch]$SkipFrontend
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Initialize-Logging -ScriptName 'build-solution' | Out-Null

try {
	$slnx = Join-Path $RepoRoot 'BudgetFlow.slnx'

	Invoke-Step -StepName 'Validate solution file' -Action {
		if (-not (Test-Path -LiteralPath $slnx)) {
			throw "Solution file not found at $slnx"
		}
	}

	Invoke-Step -StepName "Build .NET solution ($Configuration)" -Action {
		Push-Location $RepoRoot
		try {
			dotnet build $slnx --configuration $Configuration --nologo
			if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }
		}
		finally {
			Pop-Location
		}
	}

	if (-not $SkipFrontend) {
		Invoke-Step -StepName 'Build React frontend' -Action {
			$frontendPath = Join-Path $RepoRoot 'frontend\budgetflow-web'
			if (-not (Test-Path -LiteralPath $frontendPath)) {
				Write-Log -Message "Frontend folder not found at $frontendPath - skipping." -Level WARN
				return
			}

			Push-Location $frontendPath
			try {
				npm run build
				if ($LASTEXITCODE -ne 0) { throw "npm run build failed with exit code $LASTEXITCODE" }
			}
			finally {
				Pop-Location
			}
		}
	}

	Write-Log -Message 'Build completed successfully.' -Level SUCCESS
	exit $script:ExitCodeSuccess
}
catch {
	Write-Log -Message "Build failed: $($_.Exception.Message)" -Level ERROR
	exit $script:ExitCodeGeneralError
}
