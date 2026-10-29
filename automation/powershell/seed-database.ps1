<#
.SYNOPSIS
	Seeds development data for BudgetFlow by briefly launching the API in Development mode.
.DESCRIPTION
	BudgetFlow.API\Program.cs runs `DevelopmentDataSeeder.SeedAsync` automatically on startup
	when ASPNETCORE_ENVIRONMENT is 'Development'. The seeder is idempotent (guarded by an
	AuditLog marker 'DevSeed:v1'), so this script is safe to run repeatedly.
	This script starts the API with that environment set, waits for the /health endpoint
	to report healthy (which confirms seeding ran during startup), then stops the process.
.PARAMETER RepoRoot
	Root path of the BudgetFlow repository.
.PARAMETER HealthCheckUrl
	URL of the API health endpoint. Defaults to https://localhost:7000/health (adjust to your launch profile).
.PARAMETER TimeoutSeconds
	Maximum time to wait for the API to start and report healthy.
.EXAMPLE
	.\seed-database.ps1
.EXAMPLE
	.\seed-database.ps1 -HealthCheckUrl 'https://localhost:7211/health'
#>
[CmdletBinding()]
param(
	[string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path,
	[string]$HealthCheckUrl,
	[int]$TimeoutSeconds = 60
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Initialize-Logging -ScriptName 'seed-database' | Out-Null

$HealthCheckUrl = Get-ConfigValue -Name 'HealthCheckUrl' -ParameterValue $HealthCheckUrl -EnvironmentVariableName 'BUDGETFLOW_HEALTH_URL' -DefaultValue 'https://localhost:7000/health'

$apiProject = Join-Path $RepoRoot 'BudgetFlow.API\BudgetFlow.API.csproj'
$process = $null

try {
	Invoke-Step -StepName 'Validate API project exists' -Action {
		if (-not (Test-Path -LiteralPath $apiProject)) {
			throw "API project not found at $apiProject"
		}
	}

	Invoke-Step -StepName 'Launch API in Development mode to trigger seeding' -Action {
		$env:ASPNETCORE_ENVIRONMENT = 'Development'
		$process = Start-Process -FilePath 'dotnet' -ArgumentList @('run', '--project', $apiProject, '--no-launch-profile') `
			-PassThru -NoNewWindow -RedirectStandardOutput (Join-Path $PSScriptRoot 'logs\seed-api-stdout.log') `
			-RedirectStandardError (Join-Path $PSScriptRoot 'logs\seed-api-stderr.log')
		Write-Log -Message "Started API process (PID $($process.Id)) to run development seeding."
	}

	Invoke-Step -StepName 'Wait for API health endpoint (confirms seeding ran)' -Action {
		$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
		$healthy = $false

		while ((Get-Date) -lt $deadline) {
			try {
				$response = Invoke-WebRequest -Uri $HealthCheckUrl -UseBasicParsing -SkipCertificateCheck -TimeoutSec 5 -ErrorAction Stop
				if ($response.StatusCode -eq 200) {
					$healthy = $true
					break
				}
			}
			catch {
				Start-Sleep -Seconds 2
			}
		}

		if (-not $healthy) {
			throw "API did not report healthy at $HealthCheckUrl within $TimeoutSeconds seconds. Check logs\seed-api-stderr.log for details."
		}

		Write-Log -Message 'API reported healthy; development data seed has been applied (or already existed).'
	}

	Write-Log -Message 'Database seeding completed successfully.' -Level SUCCESS
	exit $script:ExitCodeSuccess
}
catch {
	Write-Log -Message "Database seeding failed: $($_.Exception.Message)" -Level ERROR
	exit $script:ExitCodeGeneralError
}
finally {
	if ($process -and -not $process.HasExited) {
		Write-Log -Message "Stopping API process (PID $($process.Id))."
		Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
	}
}
