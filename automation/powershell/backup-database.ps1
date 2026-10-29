<#
.SYNOPSIS
	Backs up the BudgetFlowDB SQL Server database to a timestamped .bak file.
.PARAMETER ServerInstance
	SQL Server instance hosting BudgetFlowDB.
.PARAMETER Database
	Database name to back up. Defaults to BudgetFlowDB.
.PARAMETER BackupFolder
	Destination folder for .bak files.
.PARAMETER SqlUser / SqlPassword
	Optional SQL authentication credentials. If omitted, Integrated Security is used.
	Never hardcode these - supply via parameters at call time or environment variables
	(BUDGETFLOW_DB_USER / BUDGETFLOW_DB_PASSWORD).
.PARAMETER RetentionDays
	Number of days of backups to retain; older .bak files are deleted.
.EXAMPLE
	.\backup-database.ps1 -ServerInstance "localhost\SQLEXPRESS" -BackupFolder "D:\Backups\BudgetFlow"
#>
[CmdletBinding()]
param(
	[string]$ServerInstance,
	[string]$Database,
	[string]$BackupFolder,
	[string]$SqlUser,
	[string]$SqlPassword,
	[int]$RetentionDays = 14
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Initialize-Logging -ScriptName 'backup-database' | Out-Null

try {
	$ServerInstance = Get-ConfigValue -Name 'ServerInstance' -ParameterValue $ServerInstance -EnvironmentVariableName 'BUDGETFLOW_DB_SERVER' -DefaultValue '(localdb)\MSSQLLocalDB' -Required
	$Database = Get-ConfigValue -Name 'Database' -ParameterValue $Database -EnvironmentVariableName 'BUDGETFLOW_DB_NAME' -DefaultValue 'BudgetFlowDB' -Required
	$BackupFolder = Get-ConfigValue -Name 'BackupFolder' -ParameterValue $BackupFolder -EnvironmentVariableName 'BUDGETFLOW_BACKUP_FOLDER' -DefaultValue 'C:\BudgetFlow\Backups' -Required

	Invoke-Step -StepName 'Verify sqlcmd is available' -Action {
		Assert-CommandExists -CommandName 'sqlcmd'
	}

	Invoke-Step -StepName 'Ensure backup folder exists' -Action {
		if (-not (Test-Path -LiteralPath $BackupFolder)) {
			New-Item -ItemType Directory -Path $BackupFolder -Force | Out-Null
			Write-Log -Message "Created backup folder: $BackupFolder"
		}
	}

	$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
	$backupFile = Join-Path $BackupFolder "$Database`_$timestamp.bak"

	Invoke-Step -StepName 'Run BACKUP DATABASE' -Action {
		$sqlUserResolved = Get-ConfigValue -Name 'SqlUser' -ParameterValue $SqlUser -EnvironmentVariableName 'BUDGETFLOW_DB_USER'
		$sqlPasswordResolved = Get-ConfigValue -Name 'SqlPassword' -ParameterValue $SqlPassword -EnvironmentVariableName 'BUDGETFLOW_DB_PASSWORD'

		$backupTsql = "BACKUP DATABASE [$Database] TO DISK = N'$backupFile' WITH INIT, COMPRESSION, STATS = 10;"

		$sqlcmdArgs = @('-S', $ServerInstance, '-Q', $backupTsql, '-b')

		if (-not [string]::IsNullOrWhiteSpace($sqlUserResolved) -and -not [string]::IsNullOrWhiteSpace($sqlPasswordResolved)) {
			$sqlcmdArgs += @('-U', $sqlUserResolved, '-P', $sqlPasswordResolved)
			Write-Log -Message 'Using SQL authentication for backup.'
		}
		else {
			$sqlcmdArgs += '-E'
			Write-Log -Message 'Using Integrated Security (Windows authentication) for backup.'
		}

		Write-Log -Message "Backing up [$Database] on [$ServerInstance] to $backupFile"
		& sqlcmd @sqlcmdArgs
		if ($LASTEXITCODE -ne 0) {
			throw "sqlcmd backup failed with exit code $LASTEXITCODE"
		}
	}

	Invoke-Step -StepName 'Verify backup file was created' -Action {
		if (-not (Test-Path -LiteralPath $backupFile)) {
			throw "Expected backup file was not found at $backupFile after BACKUP DATABASE completed."
		}
		$size = (Get-Item -LiteralPath $backupFile).Length
		Write-Log -Message "Backup file created: $backupFile ($([math]::Round($size / 1MB, 2)) MB)"
	}

	Invoke-Step -StepName "Apply retention policy ($RetentionDays days)" -Action {
		$cutoff = (Get-Date).AddDays(-$RetentionDays)
		$oldBackups = Get-ChildItem -LiteralPath $BackupFolder -Filter "$Database`_*.bak" |
			Where-Object { $_.LastWriteTime -lt $cutoff }

		foreach ($old in $oldBackups) {
			Write-Log -Message "Removing expired backup: $($old.FullName)"
			Remove-Item -LiteralPath $old.FullName -Force
		}

		if ($oldBackups.Count -eq 0) {
			Write-Log -Message 'No expired backups to remove.'
		}
	}

	Write-Log -Message "Database backup completed successfully: $backupFile" -Level SUCCESS
	exit $script:ExitCodeSuccess
}
catch {
	Write-Log -Message "Database backup failed: $($_.Exception.Message)" -Level ERROR
	exit $script:ExitCodeGeneralError
}
