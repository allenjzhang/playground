Push-Location

Set-Location $rootFolder
git checkout TypeSpecRename
rush unlink
git reset --hard upstream/main
git clean -xdf .

Set-Location $rootFolder/core
git checkout TypeSpecRename
rush unlink
git reset --hard upstream/main
git clean -xdf .

Pop-Location