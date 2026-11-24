@echo off
chcp 65001 >nul
cd /d "%~dp0.."
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist\win-x64
pause
