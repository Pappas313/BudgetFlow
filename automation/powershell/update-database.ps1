<#
.SYNOPSIS
	Applies pending EF Core migrations to the BudgetFlow database.
.DESCRIPTION
	Uses `dotnet ef database update` against BudgetFlow.Infrastructure (DbContext project)
	and BudgetFlow.API (startup project). Idempotent - EF Core skips already-applied migrations.
.PARAMETER RepoRoot
	Root path of the BudgetFlow repository.
.PARAMETER ConnectionString
	Optional explicit connection string override. If not supplied, EF Core resolves it from
	BudgetFlow.API's configuration (appsettings / environment variables / user secrets) as normal.
.PARAMETER TargetMigration
	Optional specific migration to roll forward/back to. Defaults to the latest migration.
.EXAMPLE
	.\update-database.ps1
.EXAMPLE
	.\update-database.ps1 -TargetMigration 'AddBankAccountType'
#>
[CmdletBinding()]
param(
	[string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
	[string]$ConnectionString,
	[string]$TargetMigration
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Initialize-Logging -ScriptName 'update-database' | Out-Null

try {
	$infraProject = Join-Path $RepoRoot 'BudgetFlow.Infrastructure\BudgetFlow.Infrastructure.csproj'
	$apiProject   = Join-Path $RepoRoot 'BudgetFlow.API\BudgetFlow.API.csproj'

	Invoke-Step -StepName 'Validate projects exist' -Action {
		if (-not (Test-Path -LiteralPath $infraProject)) { throw "Infrastructure project not found at $infraProject" }
		if (-not (Test-Path -LiteralPath $apiProject)) { throw "API project not found at $apiProject" }
	}

	Invoke-Step -StepName 'Verify dotnet-ef tool is available' -Action {
		$efCheck = dotnet ef --version 2>$null
		if (-not $efCheck) {
			Write-Log -Message 'dotnet-ef not found globally; installing as a local tool is recommended. Attempting to continue via `dotnet tool run dotnet-ef` fallback is not configured; please run: dotnet tool install --global dotnet-ef' -Level WARN
			throw 'dotnet-ef tool is not installed. Install it with: dotnet tool install --global dotnet-ef'
		}
		Write-Log -Message "dotnet-ef version: $efCheck"
	}

	# Never hardcode a connection string in source - only pass one through if the caller explicitly supplied it.
	$connectionArgs = @()
	if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
		$connectionArgs += @('--connection', $ConnectionString)
		Write-Log -Message 'Using explicit connection string override supplied via -ConnectionString parameter.'
	}
	else {
		Write-Log -Message 'No explicit connection string supplied; EF Core will resolve it from BudgetFlow.API configuration/environment.'
	}

	Invoke-Step -StepName 'Apply EF Core migrations' -Action {
		Push-Location $RepoRoot
		try {
			$efArgs = @('ef', 'database', 'update')
			if ($TargetMigration) { $efArgs += $TargetMigration }
			$efArgs += @('--project', $infraProject, '--startup-project', $apiProject)
			$efArgs += $connectionArgs

			Write-Log -Message "Running: dotnet $($efArgs -join ' ')"
			& dotnet @efArgs
			if ($LASTEXITCODE -ne 0) { throw "dotnet ef database update failed with exit code $LASTEXITCODE" }
		}
		finally {
			Pop-Location
		}
	}

	Write-Log -Message 'Database migration completed successfully.' -Level SUCCESS
	exit $script:ExitCodeSuccess
}
catch {
	Write-Log -Message "Database migration failed: $($_.Exception.Message)" -Level ERROR
	exit $script:ExitCodeGeneralError
}
