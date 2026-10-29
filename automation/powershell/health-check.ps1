<#
.SYNOPSIS
	Checks the health of the BudgetFlow application stack: API, database connectivity, and (optionally) frontend.
.PARAMETER ApiHealthUrl
	URL of the API's /health endpoint.
.PARAMETER ServerInstance / Database
	SQL Server connectivity check target.
.PARAMETER FrontendUrl
	Optional URL to check the React app is serving (e.g. http://localhost:5173).
.EXAMPLE
	.\health-check.ps1 -ApiHealthUrl "https://localhost:7000/health"
#>
[CmdletBinding()]
param(
	[string]$ApiHealthUrl,
	[string]$ServerInstance,
	[string]$Database,
	[string]$FrontendUrl,
	[switch]$SkipFrontend
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Initialize-Logging -ScriptName 'health-check' | Out-Null

$ApiHealthUrl = Get-ConfigValue -Name 'ApiHealthUrl' -ParameterValue $ApiHealthUrl -EnvironmentVariableName 'BUDGETFLOW_HEALTH_URL' -DefaultValue 'https://localhost:7000/health'
$ServerInstance = Get-ConfigValue -Name 'ServerInstance' -ParameterValue $ServerInstance -EnvironmentVariableName 'BUDGETFLOW_DB_SERVER' -DefaultValue '(localdb)\MSSQLLocalDB'
$Database = Get-ConfigValue -Name 'Database' -ParameterValue $Database -EnvironmentVariableName 'BUDGETFLOW_DB_NAME' -DefaultValue 'BudgetFlowDB'
$FrontendUrl = Get-ConfigValue -Name 'FrontendUrl' -ParameterValue $FrontendUrl -EnvironmentVariableName 'BUDGETFLOW_FRONTEND_URL' -DefaultValue 'http://localhost:5173'

$overallHealthy = $true

try {
	Invoke-Step -StepName 'Check API /health endpoint' -Action {
		try {
			$response = Invoke-WebRequest -Uri $ApiHealthUrl -UseBasicParsing -SkipCertificateCheck -TimeoutSec 10 -ErrorAction Stop
			if ($response.StatusCode -eq 200) {
				Write-Log -Message "API healthy: $ApiHealthUrl returned 200. Body: $($response.Content)"
			}
			else {
				$script:overallHealthy = $false
				Write-Log -Message "API returned non-200 status: $($response.StatusCode)" -Level WARN
			}
		}
		catch {
			$script:overallHealthy = $false
			Write-Log -Message "API health check failed: $($_.Exception.Message)" -Level ERROR
		}
	}

	Invoke-Step -StepName 'Check SQL Server connectivity' -Action {
		try {
			Assert-CommandExists -CommandName 'sqlcmd'
			$connectivityCheck = sqlcmd -S $ServerInstance -d $Database -Q "SELECT 1" -E -b 2>&1
			if ($LASTEXITCODE -eq 0) {
				Write-Log -Message "Database reachable: [$Database] on [$ServerInstance]"
			}
			else {
				$script:overallHealthy = $false
				Write-Log -Message "Database connectivity check failed: $connectivityCheck" -Level ERROR
			}
		}
		catch {
			$script:overallHealthy = $false
			Write-Log -Message "Database connectivity check failed: $($_.Exception.Message)" -Level ERROR
		}
	}

	if (-not $SkipFrontend) {
		Invoke-Step -StepName 'Check frontend availability' -Action {
			try {
				$response = Invoke-WebRequest -Uri $FrontendUrl -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
				if ($response.StatusCode -eq 200) {
					Write-Log -Message "Frontend reachable: $FrontendUrl returned 200."
				}
				else {
					$script:overallHealthy = $false
					Write-Log -Message "Frontend returned non-200 status: $($response.StatusCode)" -Level WARN
				}
			}
			catch {
				Write-Log -Message "Frontend health check skipped/failed (may not be running): $($_.Exception.Message)" -Level WARN
			}
		}
	}

	if ($overallHealthy) {
		Write-Log -Message 'All BudgetFlow health checks passed.' -Level SUCCESS
		exit $script:ExitCodeSuccess
	}
	else {
		Write-Log -Message 'One or more BudgetFlow health checks failed. See log for details.' -Level ERROR
		exit $script:ExitCodeValidation
	}
}
catch {
	Write-Log -Message "Health check run encountered an unexpected error: $($_.Exception.Message)" -Level ERROR
	exit $script:ExitCodeGeneralError
}
