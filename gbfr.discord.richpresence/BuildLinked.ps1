# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/gbfr.discord.richpresence/*" -Force -Recurse
dotnet publish "./gbfr.discord.richpresence.csproj" -c Release -o "$env:RELOADEDIIMODS/gbfr.discord.richpresence" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location