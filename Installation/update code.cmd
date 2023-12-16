@echo off
C:
CD \Programs\AeonLaboratories\Cegs12X
if [%1]==[] %0 C:\Data\Source\DevStudio\Cegs12X\bin\Release\netcoreapp3.1
copy "%*\*.exe" > nul
copy "%*\*.dll" > nul
copy "%*\*.config" > nul
copy "%*\*.deps.json" > nul
copy "%*\*.runtimeconfig.json" > nul
robocopy "%*\runtimes" "runtimes" /E > nul
echo *** System software updated *** >> "log\Event log.txt"
exit
