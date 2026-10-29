<#
.SYNOPSIS
	Shared logging, error-handling, and configuration helpers for BudgetFlow automation scripts.
.DESCRIPTION
	Dot-sourced by every script in automation/powershell/. Provides:
	- Timestamped console + file logging (Write-Log)
	- Standard exit-code wrapper (Invoke-Step)
	- Configuration resolution helpers (Get-ConfigValue) with NO hardcoded credentials
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Standard exit codes used across all BudgetFlow automation scripts.
$script:ExitCodeSuccess       = 0
$script:ExitCodeGeneralError  = 1
$script:ExitCodeValidation    = 2
$script:ExitCodeDependency     = 3

function Initialize-Logging {
	param(
		[Parameter(Mandatory = $true)][string]$ScriptName,
		[string]$LogDirectory = (Join-Path $PSScriptRoot 'logs')
	)

	if (-not (Test-Path -LiteralPath $LogDirectory)) {
		New-Item -ItemType Directory -Path $LogDirectory -Force | Out-Null
	}

	$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
	$logFile = Join-Path $LogDirectory "$ScriptName.$timestamp.log"
	$global:BudgetFlowLogFile = $logFile
	return $logFile
}

function Write-Log {
	param(
		[Parameter(Mandatory = $true)][string]$Message,
		[ValidateSet('INFO', 'WARN', 'ERROR', 'SUCCESS')][string]$Level = 'INFO'
	)

	$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
	$line = "[$timestamp] [$Level] $Message"

	switch ($Level) {
		'ERROR'   { Write-Host $line -ForegroundColor Red }
		'WARN'    { Write-Host $line -ForegroundColor Yellow }
		'SUCCESS' { Write-Host $line -ForegroundColor Green }
		default   { Write-Host $line }
	}

	if ($global:BudgetFlowLogFile) {
		Add-Content -LiteralPath $global:BudgetFlowLogFile -Value $line
	}
}

function Invoke-Step {
	<#
	.SYNOPSIS
		Runs a script block as a named step, logging start/success/failure and
		translating any thrown exception into a controlled, logged failure.
	#>
	param(
		[Parameter(Mandatory = $true)][string]$StepName,
		[Parameter(Mandatory = $true)][scriptblock]$Action
	)

	Write-Log -Message "Starting step: $StepName"
	try {
		& $Action
		Write-Log -Message "Completed step: $StepName" -Level SUCCESS
	}
	catch {
		Write-Log -Message "Step failed: $StepName - $($_.Exception.Message)" -Level ERROR
		throw
	}
}

function Get-ConfigValue {
	<#
	.SYNOPSIS
		Resolves a configuration value with precedence:
		1. Explicit parameter value passed in
		2. Environment variable
		3. Default value
		Never reads credentials from source-controlled files or hardcodes them.
	#>
	param(
		[Parameter(Mandatory = $true)][string]$Name,
		[string]$ParameterValue,
		[string]$EnvironmentVariableName,
		[string]$DefaultValue,
		[switch]$Required
	)

	if (-not [string]::IsNullOrWhiteSpace($ParameterValue)) {
		return $ParameterValue
	}

	if ($EnvironmentVariableName) {
		$envValue = [System.Environment]::GetEnvironmentVariable($EnvironmentVariableName)
		if (-not [string]::IsNullOrWhiteSpace($envValue)) {
			return $envValue
		}
	}

	if (-not [string]::IsNullOrWhiteSpace($DefaultValue)) {
		return $DefaultValue
	}

	if ($Required) {
		throw "Required configuration value '$Name' was not provided. Set the -$Name parameter or the `$EnvironmentVariableName` environment variable."
	}

	return $null
}

function Get-SqlConnectionString {
	<#
	.SYNOPSIS
		Builds a SQL Server connection string from parameters/environment variables.
		Supports both SQL auth (user/password) and Windows/Integrated auth.
		Credentials are NEVER hardcoded; they must come from parameters or
		environment variables (e.g. set locally, or injected by a CI/CD secret store).
	#>
	param(
		[string]$ServerInstance,
		[string]$Database,
		[string]$SqlUser,
		[string]$SqlPassword,
		[switch]$UseIntegratedSecurity
	)

	$server = Get-ConfigValue -Name 'ServerInstance' -ParameterValue $ServerInstance -EnvironmentVariableName 'BUDGETFLOW_DB_SERVER' -DefaultValue '(localdb)\MSSQLLocalDB' -Required
	$db     = Get-ConfigValue -Name 'Database' -ParameterValue $Database -EnvironmentVariableName 'BUDGETFLOW_DB_NAME' -DefaultValue 'BudgetFlowDB' -Required

	if ($UseIntegratedSecurity) {
		return "Server=$server;Database=$db;Trusted_Connection=True;TrustServerCertificate=True;"
	}

	$user = Get-ConfigValue -Name 'SqlUser' -ParameterValue $SqlUser -EnvironmentVariableName 'BUDGETFLOW_DB_USER'
	$pwd  = Get-ConfigValue -Name 'SqlPassword' -ParameterValue $SqlPassword -EnvironmentVariableName 'BUDGETFLOW_DB_PASSWORD'

	if ([string]::IsNullOrWhiteSpace($user) -or [string]::IsNullOrWhiteSpace($pwd)) {
		Write-Log -Message 'No SQL credentials supplied; falling back to Integrated Security.' -Level WARN
		return "Server=$server;Database=$db;Trusted_Connection=True;TrustServerCertificate=True;"
	}

	return "Server=$server;Database=$db;User Id=$user;Password=$pwd;TrustServerCertificate=True;"
}

function Assert-CommandExists {
	param([Parameter(Mandatory = $true)][string]$CommandName)

	if (-not (Get-Command -Name $CommandName -ErrorAction SilentlyContinue)) {
		throw "Required command '$CommandName' was not found on PATH. Please install/configure it before re-running this script."
	}
}
