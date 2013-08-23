@echo off

:: *NOTE* :	On 64bit systems, we need to invoke the 32-bit powershell executable
::	        as certain vendor dlls (e.g. SQLite) only work from a 32-bit process

set ps=powershell
			
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
	set ps=%SystemRoot%\syswow64\WindowsPowerShell\v1.0\powershell.exe
)

call %ps% -NoProfile -ExecutionPolicy unrestricted -Command "& { .\vendor\psake.4.2.0.1\tools\psake.ps1 .\build\build.ps1 %*; if ($lastexitcode -ne 0) {write-host "ERROR: $lastexitcode" -fore RED; exit $lastexitcode} }"