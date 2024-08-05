@echo off
setlocal enabledelayedexpansion

set "vs_path=C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"

set "build_bat_path=.\Debug.bat"

call "%vs_path%" && call "%build_bat_path%"

endlocal