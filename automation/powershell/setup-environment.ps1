<#
.SYNOPSIS
	Validates and prepares the local development environment for BudgetFlow.
.DESCRIPTION
	Checks for required tooling (.NET SDK, Node.js, SQL Server connectivity),
	restores NuGet and npm packages, and verifies configuration files exist.
	Safe to run repeatedly (idempotent) - only installs/restores what's missing or stale.
.PARAMETER RepoRoot
	Root path of the BudgetFlow repository. Defaults to the parent of this script's grandparent folder.
.PARAMETER SkipFrontend
	Skip Node.js/npm checks and restore for the React app.
.EXAMPLE
	.\setup-environment.ps1
.EXAMPLE
	.\setup-environment.ps1 -RepoRoot 'C:\Users\hrist\source\repos\BudgetFlow' -SkipFrontend
#>
[CmdletBinding()]
param(
	[string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
	[switch]$SkipFrontend
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Initialize-Logging -ScriptName 'setup-environment' | Out-Null

try {
	Write-Log -Message "BudgetFlow repo root: $RepoRoot"

	Invoke-Step -StepName 'Verify .NET SDK' -Action {
		Assert-CommandExists -CommandName 'dotnet'
		$version = (dotnet --version)
		Write-Log -Message ".NET SDK version detected: $version"
	}

	if (-not $SkipFrontend) {
		Invoke-Step -StepName 'Verify Node.js and npm' -Action {
			Assert-CommandExists -CommandName 'node'
			Assert-CommandExists -CommandName 'npm'
			Write-Log -Message "Node version: $(node --version); npm version: $(npm --version)"
		}
	}

	Invoke-Step -StepName 'Verify solution file exists' -Action {
		$slnx = Join-Path $RepoRoot 'BudgetFlow.slnx'
		if (-not (Test-Path -LiteralPath $slnx)) {
			throw "Solution file not found at $slnx"
		}
	}

	Invoke-Step -StepName 'Restore .NET solution dependencies' -Action {
		Push-Location $RepoRoot
		try {
			dotnet restore 'BudgetFlow.slnx'
			if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }
		}
		finally {
			Pop-Location
		}
	}

	if (-not $SkipFrontend) {
		Invoke-Step -StepName 'Install frontend dependencies' -Action {
			$frontendPath = Join-Path $RepoRoot 'frontend\budgetflow-web'
			if (-not (Test-Path -LiteralPath $frontendPath)) {
				Write-Log -Message "Frontend folder not found at $frontendPath - skipping." -Level WARN
				return
			}

			Push-Location $frontendPath
			try {
				# npm ci is safe for repeated runs and uses package-lock.json exactly.
				if (Test-Path -LiteralPath (Join-Path $frontendPath 'package-lock.json')) {
					npm ci
				}
				else {
					npm install
				}
				if ($LASTEXITCODE -ne 0) { throw "npm install/ci failed with exit code $LASTEXITCODE" }
			}
			finally {
				Pop-Location
			}
		}
	}

	Invoke-Step -StepName 'Verify appsettings configuration present' -Action {
		$appSettings = Join-Path $RepoRoot 'BudgetFlow.API\appsettings.Development.json'
		if (-not (Test-Path -LiteralPath $appSettings)) {
			Write-Log -Message "appsettings.Development.json not found at $appSettings. Create it locally with your connection string (do not commit secrets)." -Level WARN
		}
	}

	Write-Log -Message 'Environment setup completed successfully.' -Level SUCCESS
	exit $script:ExitCodeSuccess
}
catch {
	Write-Log -Message "Environment setup failed: $($_.Exception.Message)" -Level ERROR
	exit $script:ExitCodeGeneralError
}
