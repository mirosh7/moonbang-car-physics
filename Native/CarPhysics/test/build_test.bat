@echo off
call "C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat" >nul
cl /nologo /EHsc /O2 /I "%~dp0..\include" "%~dp0test_spin.cpp" "%~dp0..\src\models.cpp" /Fe:"%~dp0test_spin.exe" /Fo:"%~dp0"
"%~dp0test_spin.exe"
