param(
    [Parameter(Mandatory = $true)]
    [string]$MigrationName
)

$project = "$PSScriptRoot/../../src/CashFlow.Transactions.Infrastructure"
$startupProject = "$PSScriptRoot/../../src/CashFlow.Transactions.Web"
$context = "ApplicationDbContext"

dotnet ef migrations add $MigrationName `
    --project $project `
    --startup-project $startupProject `
    --context $context