@rem bat file to ease use of the script

@%~d0
@cd "%~dp0"

powershell.exe -ExecutionPolicy Bypass -NoLogo -NoProfile -file "%~dpn0.ps1" -solutionPath "%~dp0..\..\SqlServer.Rules.sln"

@echo Done.
@pause
