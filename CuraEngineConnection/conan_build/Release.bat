cd /D %~dp0../
rem rd /s /q build\Release

conan remove --locks
conan install . --build=missing --update

cmake --preset release
cmake --build --preset release

EXIT /B %EXIT_CODE%