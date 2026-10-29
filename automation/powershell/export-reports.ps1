<#
.SYNOPSIS
	Executes the BudgetFlowFinancialExport SSIS package to produce CSV reporting exports.
.DESCRIPTION
	Uses dtexec (SQL Server Integration Services command-line utility) to run the package
	built from BudgetFlow.ETL\BudgetFlowFinancialExport.dtsx. Supports running against the
	project's .dtproj-built package file directly, with connection/variable overrides
	supplied via parameters or environment variables (no hardcoded credentials).
.PARAMETER RepoRoot
	Root path of the BudgetFlow repository.
.PARAMETER PackagePath
	Path to the compiled .dtsx package. Defaults to BudgetFlow.ETL\BudgetFlowFinancialExport.dtsx.
.PARAMETER ServerInstance
	SQL Server instance hosting BudgetFlowDB.
.PARAMETER ExportFolder
	Override for the package's ExportFolder variable.
.EXAMPLE
	.\export-reports.ps1 -ServerInstance "localhost\SQLEXPRESS"
#>
[CmdletBinding()]
param(
	[string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
	[string]$PackagePath,
	[string]$ServerInstance,
	[string]$ExportFolder
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Initialize-Logging -ScriptName 'export-reports' | Out-Null

try {
	$PackagePath = Get-ConfigValue -Name 'PackagePath' -ParameterValue $PackagePath -EnvironmentVariableName 'BUDGETFLOW_SSIS_PACKAGE' `
		-DefaultValue (Join-Path $RepoRoot 'BudgetFlow.ETL\BudgetFlowFinancialExport.dtsx')
	$ServerInstance = Get-ConfigValue -Name 'ServerInstance' -ParameterValue $ServerInstance -EnvironmentVariableName 'BUDGETFLOW_DB_SERVER' -DefaultValue '(localdb)\MSSQLLocalDB'
	$ExportFolder = Get-ConfigValue -Name 'ExportFolder' -ParameterValue $ExportFolder -EnvironmentVariableName 'BUDGETFLOW_EXPORT_FOLDER' -DefaultValue 'C:\BudgetFlow\Exports'

	Invoke-Step -StepName 'Verify dtexec is available' -Action {
		Assert-CommandExists -CommandName 'dtexec'
	}

	Invoke-Step -StepName 'Verify SSIS package exists' -Action {
		if (-not (Test-Path -LiteralPath $PackagePath)) {
			throw "SSIS package not found at $PackagePath. Build the BudgetFlow.ETL project first."
		}
	}

	Invoke-Step -StepName 'Ensure export folder exists' -Action {
		if (-not (Test-Path -LiteralPath $ExportFolder)) {
			New-Item -ItemType Directory -Path $ExportFolder -Force | Out-Null
			Write-Log -Message "Created export folder: $ExportFolder"
		}
		else {
			Write-Log -Message "Export folder already exists: $ExportFolder"
		}
	}

	Invoke-Step -StepName 'Execute SSIS package' -Action {
		$connectionString = Get-SqlConnectionString -ServerInstance $ServerInstance -Database 'BudgetFlowDB' -UseIntegratedSecurity

		$dtexecArgs = @(
			'/File', $PackagePath,
			'/Set', "\Package.Connections[cm_BudgetFlowDB].Properties[ConnectionString];$connectionString",
			'/Set', "\Package.Variables[User::ExportFolder].Properties[Value];$ExportFolder",
			'/Reporting', 'E'
		)

		Write-Log -Message "Running: dtexec $($dtexecArgs -join ' ')" 
		& dtexec @dtexecArgs
		$dtexecExitCode = $LASTEXITCODE

		# dtexec exit codes: 0 = success, 1 = failure, 3 = cancelled, 4/5 = cannot load
		if ($dtexecExitCode -ne 0) {
			throw "dtexec reported failure (exit code $dtexecExitCode). Check SSIS logging output above."
		}
	}

	Invoke-Step -StepName 'Verify expected CSV files were produced' -Action {
		$expectedFiles = @(
			'monthly_income_expenses.csv',
			'category_expenses.csv',
			'budget_variance.csv',
			'account_balances.csv',
			'upcoming_bills.csv',
			'savings_progress.csv'
		)

		$missing = $expectedFiles | Where-Object { -not (Test-Path -LiteralPath (Join-Path $ExportFolder $_)) }
		if ($missing.Count -gt 0) {
			throw "Missing expected export files: $($missing -join ', ')"
		}
	}

	Write-Log -Message "Report export completed successfully. Files written to $ExportFolder" -Level SUCCESS
	exit $script:ExitCodeSuccess
}
catch {
	Write-Log -Message "Report export failed: $($_.Exception.Message)" -Level ERROR
	exit $script:ExitCodeGeneralError
}
