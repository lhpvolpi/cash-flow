$project = "$PSScriptRoot/../../src/CashFlow.Transactions.Infrastructure"
$startupProject = "$PSScriptRoot/../../src/CashFlow.Transactions.Web"
$context = "ApplicationDbContext"

dotnet ef migrations remove `
    --project $project `
    --startup-project $startupProject `
    --context $context