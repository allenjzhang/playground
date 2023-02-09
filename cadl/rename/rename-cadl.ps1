function ProcessFileItem($fileObject) {
  if ($fileObject.PSIsContainer) {
    if ($ignoreFolderList.Contains($fileObject.Name)) {
      Write-Host $fileObject.FullName -ForegroundColor DarkGray
    }
    else {
      Write-Host $fileObject.FullName -ForegroundColor Magenta
      Get-ChildItem -Path $fileObject.FullName | foreach-object { ProcessFileItem($_); };
    };
  }
  else {
    Write-Host $fileObject.FullName -ForegroundColor Green
    SearchReplace($fileObject.FullName)
  }
}

function RenamingFileItems($fileObject) {
  if ($fileObject.PSIsContainer) {
    if ($ignoreFolderList.Contains($fileObject.Name)) {
      Write-Host $fileObject.FullName -ForegroundColor DarkGray
    }
    else {
      Get-ChildItem -Path $fileObject.FullName | foreach-object { RenamingFileItems($_); };

      if ($fileObject.Name.Contains("cadl")) {
        Write-Host $fileObject.FullName -ForegroundColor Yellow
        $newFolderName = $fileObject.Name -replace "cadl", "typespec"
        if ($newFolderName -ne $fileObject.Name) {
          $fileObject = Rename-Item -Path $fileObject.FullName -NewName $newFolderName
        }
      }
    };
  }
  else {
    # Renaming cadl-project.yaml
    if ($fileObject.Name -eq "cadl-project.yaml") {
      Rename-Item $fileObject.FullName "tsp-project.yaml"
    }

    # Renaming .cadl extension
    $fileExtension = [System.IO.Path]::GetExtension($fileObject.FullName)
    if ($fileExtension -eq "cadl") {
      $newFileName = [System.IO.Path]::ChangeExtension($fileObject.FullName, "cadl")
      Rename-Item $fileObject.FullName $newFileName
      Write-Host $newFileName -ForegroundColor Green
    }
  }
}

function SearchReplace($fileName) {
  $fileContent = Get-Content -Path $fileName

  # Skip empty files
  if ($fileContent.Length -le 0) {
    return
  }

  # Use enumerator for case-sensitive replacement
  $enumerator = $replacements.GetEnumerator()
  while ($enumerator.MoveNext()) {
    # $regex = [System.Text.RegularExpressions.Regex]::new($enumerator.Key, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline)
    # $fileContent = $regex.Replace($fileContent, $enumerator.Value)
    # $fileContent | ForEach-Object { $regex.Replace($_, $enumerator.Value) }
    $fileContent = $fileContent -replace $enumerator.Key, $enumerator.Value
  }

  Set-Content -Path $fileName -Value $fileContent
}

$currentScript = $MyInvocation.MyCommand.Definition
$currentScriptDirectory = Split-Path $currentScript
$ignoreFolderList = Get-Content $currentScriptDirectory\rename-folder-ignore.txt
$rootFolder = "C:\Github\cadl"

# Note, following list values are ordered
$replacements = New-Object System.Collections.Specialized.OrderedDictionary
$replacements.Add("(?-i)\(files.cadl\)", "\(files.typespec\)");
$replacements.Add("(?-i)init.cadlFileFolder", "init.typespecFileFolder");
$replacements.Add("(?-i)microsoft\.cadl\.", "microsoft\.typespec\.");
$replacements.Add("(?-i)@cadl-lang", "@typespec");
$replacements.Add("(?-i)cadl init", "tsp init");
$replacements.Add("(?-i)npx cadl", "npx tsp");
$replacements.Add("(?-i)\.cadl", ".tsp");
$replacements.Add("(?-i)cadl-", "typespec-");
$replacements.Add("(?-i)cadl", "typespec");
$replacements.Add("(?-i)Cadl", "TypeSpec");

ProcessFileItem( Get-Item $rootFolder);
Write-Host "---------------------------------------------------------------------------"
Get-ChildItem -Path $rootFolder | foreach-object { RenamingFileItems($_) }