@echo off
cd /D %~dp0

call ..\.stbuild\build.cmd --Target Pack --Variant Release_x64

pause

EXIT /B %EXIT_CODE%
