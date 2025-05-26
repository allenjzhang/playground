Param (
  [Parameter(Mandatory = $true)]
  [int]$Step
)

#this is set a global. if running individually, uncomment
#$rootFolder = "C:/github/cadl"

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

      if ($fileObject.Name.Contains("cadl-lang")) {
        Write-Host $fileObject.FullName -ForegroundColor Yellow
        $newFolderName = $fileObject.Name -replace "cadl-lang", "typespec"
        if ($newFolderName -ne $fileObject.Name) {
          $fileObject = Rename-Item -Path $fileObject.FullName -NewName $newFolderName
        }
      }
      elseif ($fileObject.Name.Contains("cadl")) {
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
      Rename-Item $fileObject.FullName "tspconfig.yaml"
      return
    }

    # Renaming cadl-server.js
    if ($fileObject.Name -eq "cadl-server.js") {
      Rename-Item $fileObject.FullName "tsp-server.js"
      return
    }

    # Renaming cadl.js
    if ($fileObject.Name -eq "cadl.js") {
      Rename-Item $fileObject.FullName "tsp.js"
      return
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
  $fileContent = Get-Content -Path $fileName -Raw

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

  Set-Content -Path $fileName -Value $fileContent -NoNewline
}

function SpecificSearchReplace([string]$fileName, [string]$searchStr, [string]$replaceStr) {
  $fileContent = Get-Content -Path $fileName -Raw

  $fileContent = $fileContent -replace $searchStr, $replaceStr

  Set-Content -Path $fileName -Value $fileContent -NoNewline
}

function ConvertCRLFtoLF($fileName) {
  $text = [IO.File]::ReadAllText($fileName) -replace "`r`n", "`n"
  [IO.File]::WriteAllText($fileName, $text)
}

$currentScript = $MyInvocation.MyCommand.Definition
$currentScriptDirectory = Split-Path $currentScript
$ignoreFolderList = Get-Content $currentScriptDirectory/rename-folder-ignore.txt
$ignoreFilesList = Get-Content $currentScriptDirectory/rename-file-ignore.txt

# Note, following list values are ordered
$replacements = New-Object System.Collections.Specialized.OrderedDictionary
$replacements.Add("(?-i)``````cadl", "``````typespec");
$replacements.Add("(?-i)cadl-server", "tsp-server");
$replacements.Add("(?-i)cadl.js", "tsp.js");
$replacements.Add("(?-i)cadl-project\.yaml", "tspconfig.yaml");
$replacements.Add("(?-i)\.cadlMain", ".typespecMain");
$replacements.Add("(?-i)files.cadl", "files.typespec");
$replacements.Add("(?-i)init.cadlFileFolder", "init.typespecFileFolder");
$replacements.Add("(?-i)microsoft\.cadl\.", "microsoft.typespec.");
$replacements.Add("(?-i)@cadl-lang", "@typespec");
$replacements.Add("(?-i)cadl init", "tsp init");
$replacements.Add("(?-i)cadl vs", "tsp vs");
$replacements.Add("(?-i)cadl code", "tsp code");
$replacements.Add("(?-i)cadl install", "tsp install");
$replacements.Add("(?-i)cadl compile", "tsp compile");
$replacements.Add("(?-i)cadl format", "tsp format");
$replacements.Add("(?-i)cadl --help", "tsp --help");
$replacements.Add("(?-i)cadl <comp", "tsp <comp");
$replacements.Add("(?-i)cadl info", "tsp info");
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
  SpecificSearchReplace "$rootFolder/core/packages/migrate/package.json" "npm:@typespec/compiler@" "npm:@cadl-lang/compiler@"
  SpecificSearchReplace "$rootFolder/core/packages/migrate/src/migrations/v0.38/model-to-scalars.ts" "TypeSpecScriptNode" "CadlScriptNode"
  SpecificSearchReplace "$rootFolder/rush.json" "microsoft//.typespec//.providerhub" "microsoft.typespec.providerhub"
  SpecificSearchReplace "$rootFolder/packages/website/.scripts/clone-emitter-packages.ps1" "src/TYPESPEC.Extension" "src/CADL.Extension"
  SpecificSearchReplace "$rootFolder/packages/website/.scripts/regen-ref-docs.mjs" "src/TYPESPEC.Extension" "src/CADL.Extension"
  ProcessFileItem( Get-Item "$rootFolder/core/packages/compiler/test/libraries/simple/node_modules/MyLib")
  ProcessFileItem( Get-Item "$rootFolder/core/packages/compiler/test/libraries/simple/node_modules/CustomCadlMain")
  ProcessFileItem( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/emitter-require-import/node_modules/`@cadl-lang")
  ProcessFileItem( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/emitter-throw-error/node_modules/`@cadl-lang")
  ProcessFileItem( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/import-library-invalid/node_modules/my-lib")
  ProcessFileItem( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/same-library-diff-version/node_modules/`@cadl-lang")
  ProcessFileItem( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/same-library-same-version/node_modules/`@cadl-lang")

  SpecificSearchReplace "$rootFolder/core/packages/compiler/package.json" "typespec"": ""cmd/" "tsp"": ""cmd/"
  SpecificSearchReplace "$rootFolder/core/packages/compiler/package.json" "typespec-server"": ""cmd/" "tsp-server"": ""cmd/"
  SpecificSearchReplace "$rootFolder/core/.github/workflows/format-pr.yml" "typespeceng format" "tspeng format"
  SpecificSearchReplace "$rootFolder/core/.prettierignore" ".typespec" ".tsp"
}
elseif ($Step -eq 2) {
  Write-Host "Step 2: Perform files, folders rename" -BackgroundColor Yellow  -ForegroundColor Black
  Get-ChildItem -Path $rootFolder | foreach-object { RenamingFileItems($_) }

  RenamingFileItems( Get-Item "$rootFolder/core/packages/compiler/test/libraries/simple/node_modules/MyLib")
  RenamingFileItems( Get-Item "$rootFolder/core/packages/compiler/test/libraries/simple/node_modules/CustomCadlMain")
  RenamingFileItems( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/emitter-require-import/node_modules/`@cadl-lang")
  RenamingFileItems( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/emitter-throw-error/node_modules/`@cadl-lang")
  RenamingFileItems( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/same-library-diff-version/node_modules/`@cadl-lang")
  RenamingFileItems( Get-Item "$rootFolder/core/packages/compiler/test/e2e/scenarios/same-library-same-version/node_modules/`@cadl-lang")

  #Rename-Item -Path "$rootFolder/packages/cadl-autorest" -NewName "$rootFolder/packages/typespec-autorest"
  # # Formatter test are sensitive to line return
  # Get-ChildItem -Path "$rootFolder/core/packages/compiler/test/formatter/scenarios/outputs" | foreach-object { ConvertCRLFtoLF $_ }

  # Add back x mod for executable
  git update-index --chmod=+x $rootFolder/core/packages/compiler/cmd/tsp.js
  git update-index --chmod=+x $rootFolder/core/packages/compiler/cmd/tsp-server.js

  SpecificSearchReplace "$rootFolder/core/packages/compiler/package.json" "typespec"": ""cmd/" "tsp"": ""cmd/"
  SpecificSearchReplace "$rootFolder/packages/typespec-msbuild-target/TypeSpecCompile.cs" """typespec.cmd"" : ""typespec""" """tsp.cmd"" : ""tsp"""
}
else {
  Write-Host "Error: Only 2 steps supported. Please execute in order" -BackgroundColor DarkRed
}
