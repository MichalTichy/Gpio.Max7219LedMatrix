$oldSlnName = Read-Host "Please enter a old solution name"
$newSlnName = Read-Host "Please enter a new solution name"

Write-Host "Cleanup" -ForegroundColor Green  
Get-ChildItem .\ -include bin,obj,packages -Recurse | %{ remove-item $_.fullname -Force -Recurse }
Get-ChildItem -Hidden .\ -include .vs -Recurse | %{ remove-item $_.fullname -Force -Recurse }
Write-Host "Cleanup sucessfull" -ForegroundColor Green

Write-Host "Rename files from " $oldSlnName " to " $newSlnName -ForegroundColor Green   
$projectpath = $(get-location)
$files = Get-ChildItem $projectpath -include *.cs, *.csproj, *.sln, * -Filter *$oldSlnName* -Recurse 

$files |
    Sort-Object -Descending -Property { $_.FullName } |
    Rename-Item -newname { $_.name -replace $oldSlnName, $newSlnName } -force

Write-Host "Rename files sucessfull" -ForegroundColor Green

Write-Host "Replace content in files" -ForegroundColor Green
$files = Get-ChildItem $projectpath -File -include *.cs, *.csproj, *.sln, * -Recurse 

foreach($file in $files) 
{ 
    ((Get-Content $file.fullname) -creplace $oldSlnName, $newSlnName) | set-content $file.fullname 
}

Write-Host "Done!" -ForegroundColor Green
