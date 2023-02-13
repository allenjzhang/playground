Push-Location

Set-Location C:\Github\cadl
git checkout TypeSpecRename
rush unlink
git reset --hard upstream/main
git clean -xdf .

Set-Location C:\Github\cadl\core
git checkout TypeSpecRename
rush unlink
git reset --hard upstream/main
git clean -xdf .

Pop-Location