$global:rootFolder = "/Users/allenzhang/github/cadl"

function NotifyNextStep($msg) {
  Write-Host "`r`n$msg`r`n" -BackgroundColor White -ForegroundColor Black
  $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

Push-Location

& "$PSScriptRoot/reset.ps1"

NotifyNextStep "Reset completed. About to Start Step 1. -->$PSScriptRoot/rename-cadl.ps1 -Step 1<--Press any key to continue..."

& "$PSScriptRoot/rename-cadl.ps1" -Step 1

Set-Location $rootFolder
git add .
git commit -m "1/3 TypeSpec: Find/Replace Content"

Set-Location $rootFolder/core
git add .
git commit -m "1/3 TypeSpec: Find/Replace Content"

NotifyNextStep "Step 1 completed and committed. About to Start step 2. Press any key to continue..."

& "$PSScriptRoot/rename-cadl.ps1" -Step 2
Set-Location $rootFolder
git add .
git commit -m "2/3 TypeSpec: Renaming files"

Set-Location $rootFolder/core
git add .
git commit -m "2/3 TypeSpec: Renaming files"

NotifyNextStep "Press any key to run rush update/build TypeSpec Core"
Set-Location $rootFolder/core
rush update
rush rebuild
rush format
git add .
git commit -m "3/3 Rush update, rebuild, and format successful"

NotifyNextStep "Press any key to run rush rebuild TypeSpec Azure"
Set-Location $rootFolder
rush update
rush rebuild
rush format
git add .
git commit -m "3/3 Rush update, rebuild, and format successful"


NotifyNextStep "All done. You can review and commit last batch of changed files"

Pop-Location