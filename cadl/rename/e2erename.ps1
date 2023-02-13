function NotifyNextStep($msg) {
  Write-Host "`r`n$msg`r`n" -BackgroundColor White -ForegroundColor Black
  $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

Push-Location

.\reset.ps1

NotifyNextStep "Reset completed. About to Start Step 1. Press any key to continue..."

.\rename-cadl.ps1 -Step 1
Set-Location c:\Github\cadl
git add .
git commit -m "1/2 TypeSpec: Find/Replace Content"

Set-Location c:\Github\cadl\core
git add .
git commit -m "1/2 TypeSpec: Find/Replace Content"

NotifyNextStep "Step 1 completed and committed. About to Start step 2. Press any key to continue..."

.\rename-cadl.ps1 -Step 2
Set-Location c:\Github\cadl
git add .
#git commit -m "1/2 TypeSpec: Find/Replace Content"

Set-Location c:\Github\cadl\core
git add .
#git commit -m "1/2 TypeSpec: Find/Replace Content"

NotifyNextStep "Press any key to run rush update/build TypeSpec Core"
Set-Location c:\Github\cadl\core
rush update
rush rebuild

NotifyNextStep "Press any key to run rush rebuild TypeSpec Azure"
Set-Location c:\Github\cadl
rush update
rush rebuild

NotifyNextStep "Press any key to run rush format"
Set-Location c:\Github\cadl
rush format
git add .
Set-Location C:\Github\cadl\core
git add .

NotifyNextStep "All done. You can review and commit last batch of changed files"

Pop-Location