@echo off
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "SportField_WinForms_Merged_Final.sln" /t:Rebuild /p:Configuration=Debug /m
exit /b %ERRORLEVEL%
