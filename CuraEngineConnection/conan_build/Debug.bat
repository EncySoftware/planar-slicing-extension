cd /D %~dp0../
rem rd /s /q build\Debug
conan remote remove cura
conan remove --locks
conan install . --build=missing --update -s build_type=Debug

cmake --preset debug
cmake --build --preset debug

EXIT /B %EXIT_CODE%