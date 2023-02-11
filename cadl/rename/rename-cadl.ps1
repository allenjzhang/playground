Param (
  [Parameter(Mandatory = $true)]
  [int]$Step
)

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
    if ($ignoreFilesList.Contains($fileObject.Name)) {
      Write-Host $fileObject.FullName -ForegroundColor DarkGray
      return
    }

    Write-Host $fileObject.FullName -ForegroundColor Green
    SearchReplace($fileObject.FullName)
  }
}

function RenamingFileItems($fileObject) {
  if ($null -eq $fileObject) {
    return;
  }

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
      elseif ($fileObject.Name.Contains("Cadl")) {
        Write-Host $fileObject.FullName -ForegroundColor Yellow
        $newFolderName = $fileObject.Name -replace "Cadl", "TypeSpec"
        if ($newFolderName -ne $fileObject.Name) {
          $fileObject = Rename-Item -Path $fileObject.FullName -NewName $newFolderName
        }
      }
    };
  }
  else {
    if ($ignoreFilesList.Contains($fileObject.Name)) {
      Write-Host $fileObject.FullName -ForegroundColor DarkGray
      return
    }

    # Renaming cadl-project.yaml
    if ($fileObject.Name -eq "cadl-project.yaml") {
      $fileObject = Rename-Item $fileObject.FullName "tspconfig.yaml"
    }

    # Renaming .cadl extension
    $fileExtension = [System.IO.Path]::GetExtension($fileObject.FullName)
    if ($fileExtension -eq ".cadl") {
      $newFileName = [System.IO.Path]::ChangeExtension($fileObject.FullName, "tsp")
      Rename-Item $fileObject.FullName $newFileName

      $fileObject = Get-Item $newFileName
      Write-Host $newFileName -ForegroundColor Green
    }

    if ($null -eq $fileObject) {
      Write-Host "ANORMALY DETECTED:"+$newFileName -ForegroundColor Red -BackgroundColor Yellow
      return;
    }

    # rename file contains cadl
    if ($fileObject.Name.Contains("cadl")) {
      $newFileName = $fileObject.Name -replace "cadl", "typespec"
      Rename-Item -Path $fileObject.FullName -NewName $newFileName
      Write-Host $newFileName -ForegroundColor Green
    }
    # rename file contains cadl
    elseif ($fileObject.Name.Contains("Cadl")) {
      $newFileName = $fileObject.Name -replace "Cadl", "TypeSpec"
      $fileObject = Rename-Item -Path $fileObject.FullName -NewName $newFileName
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

function SpecificSearchReplace([string]$fileName, [string]$searchStr, [string]$replaceStr) {
  $fileContent = Get-Content -Path $fileName
  
  $fileContent = $fileContent -replace $searchStr, $replaceStr

  Set-Content -Path $fileName -Value $fileContent
}

$currentScript = $MyInvocation.MyCommand.Definition
$currentScriptDirectory = Split-Path $currentScript
$ignoreFolderList = Get-Content $currentScriptDirectory\rename-folder-ignore.txt
$ignoreFilesList = Get-Content $currentScriptDirectory\rename-file-ignore.txt
$rootFolder = "C:\Github\cadl"

# Note, following list values are ordered
$replacements = New-Object System.Collections.Specialized.OrderedDictionary
$replacements.Add("(?-i)cadl-project\.yaml", "tspconfig.yaml");
$replacements.Add("(?-i)\.cadlMain", ".typespecMain");
$replacements.Add("(?-i)files.cadl", "files.typespec");
$replacements.Add("(?-i)init.cadlFileFolder", "init.typespecFileFolder");
$replacements.Add("(?-i)microsoft\.cadl\.", "microsoft.typespec.");
$replacements.Add("(?-i)@cadl-lang", "@typespec");
$replacements.Add("(?-i)cadl init", "tsp init");
$replacements.Add("(?-i)npx cadl", "npx tsp");
$replacements.Add("(?-i)\.cadl(?=[^a-zA-Z0-9])", ".tsp");
$replacements.Add("(?-i)cadl-", "typespec-");
$replacements.Add("(?-i)cadlMain", "typespecMain");
# -- Core replacement token, case sensitive should be last
$replacements.Add("(?-i)cadl", "typespec");
$replacements.Add("(?-i)Cadl", "TypeSpec");
$replacements.Add("(?-i)CADL", "TYPESPEC");

if ($Step -eq 1) {
  Write-Host "Step 1: Perform content replacement Cadl -> TypeSpec" -BackgroundColor Yellow -ForegroundColor Black

  # -- Main replacements
  ProcessFileItem( Get-Item $rootFolder);

  # -- Post replace correction
  SpecificSearchReplace "C:\Github\cadl\core\packages\migrate\package.json" "npm:@typespec/compiler@" "npm:@cadl-lang/compiler@"
  SpecificSearchReplace C:\Github\cadl\core\packages\migrate\src\migrations\v0.38\model-to-scalars.ts "TypeSpecScriptNode" "CadlScriptNode"
  SpecificSearchReplace C:\Github\cadl\rush.json "microsoft\\.typespec\\.providerhub" "microsoft.typespec.providerhub"
  SpecificSearchReplace C:\Github\cadl\packages\website\.scripts\clone-emitter-packages.ps1 "src/TYPESPEC.Extension" "src/CADL.Extension"
  SpecificSearchReplace C:\Github\cadl\packages\website\.scripts\regen-ref-docs.mjs "src/TYPESPEC.Extension" "src/CADL.Extension"
  SpecificSearchReplace C:\Github\cadl\core\packages\compiler\test\libraries\simple\main.cadl "CustomTypeSpecMain" "CustomCadlMain"
  ProcessFileItem( Get-Item C:\Github\cadl\core\packages\compiler\test\libraries\simple\node_modules\MyLib)
  ProcessFileItem( Get-Item C:\Github\cadl\core\packages\compiler\test\libraries\simple\node_modules\CustomCadlMain)
}
elseif ($Step -eq 2) {
  Write-Host "Step 2: Perform files, folders rename" -BackgroundColor Yellow  -ForegroundColor Black
  Get-ChildItem -Path $rootFolder | foreach-object { RenamingFileItems($_) }

  RenamingFileItems( Get-Item "C:\Github\cadl\core\packages\compiler\test\libraries\simple\node_modules\MyLib")
  RenamingFileItems( Get-Item "C:\Github\cadl\core\packages\compiler\test\libraries\simple\node_modules\CustomCadlMain")
  RenamingFileItems( Get-Item "C:\Github\cadl\core\packages\compiler\test\e2e\scenarios\same-library-same-version\node_modules\`@cadl-lang")
  RenamingFileItems( Get-Item "C:\Github\cadl\core\packages\compiler\test\e2e\scenarios\same-library-diff-version\node_modules\`@cadl-lang")
  RenamingFileItems( Get-Item "C:\Github\cadl\core\packages\compiler\test\e2e\scenarios\emitter-require-import\node_modules\@cadl-lang")
}
else {
  Write-Host "Error: Only 2 steps supported. Please execute in order" -BackgroundColor DarkRed
}
