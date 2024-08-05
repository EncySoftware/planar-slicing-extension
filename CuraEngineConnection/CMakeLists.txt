# Copyright (c) 2022 Ultimaker B.V.
# CuraEngine is released under the terms of the AGPLv3 or higher.

cmake_policy(SET CMP0091 NEW)  # For MSVC flags, will be ignored on non-Windows OS's
cmake_minimum_required(VERSION 3.20)
project(CuraEngineConnect)
find_package(standardprojectsettings REQUIRED)  # imports the cmake module https://github.com/Ultimaker/conan-ultimaker-index/recipes/standardprojectsettings
AssureOutOfSourceBuilds()

option(ENABLE_ARCUS "Enable support for ARCUS" OFF)
option(EXTENSIVE_WARNINGS "Build with all warnings" OFF)
option(ENABLE_PLUGINS "Build with plugins" OFF)
option(ENABLE_MORE_COMPILER_OPTIMIZATION_FLAGS "Enable more optimization flags" OFF)

# Generate the plugin types
find_package(protobuf REQUIRED)
find_package(asio-grpc REQUIRED)
find_package(gRPC REQUIRED)
find_package(curaengine_grpc_definitions REQUIRED)

MESSAGE(STATUS "Compiling with plugins support: ${ENABLE_PLUGINS}")
if (${ENABLE_PLUGINS})
    MESSAGE(STATUS "Plugin secure remotes allowed: ${ENABLE_REMOTE_PLUGINS}")
endif ()

if (ENABLE_ARCUS)
    message(STATUS "Building with Arcus")
    find_package(arcus REQUIRED)
    protobuf_generate_cpp(engine_PB_SRCS engine_PB_HEADERS Cura.proto)
endif ()

### Compiling CuraEngine ###
# First compile all of CuraEngine as library.

set(engine_SRCS # Except main.cpp.
        src/Application.cpp
        src/communication/ArcusCommunication.cpp
        src/communication/ArcusCommunicationPrivate.cpp
        src/communication/Listener.cpp  
        src/Exports.cpp
        )

#set (CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS OFF)
add_library(CuraEngineConnection SHARED ${engine_SRCS} ${engine_PB_SRCS}  "include/CuraEngineControl.h" "include/utils/CEMesh.h")
use_threads(CuraEngineConnection)

target_include_directories(CuraEngineConnection
        PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
        $<INSTALL_INTERFACE:${CMAKE_INSTALL_INCLUDEDIR}>
        PRIVATE
        $<BUILD_INTERFACE:${CMAKE_CURRENT_BINARY_DIR}> # Include Cura.pb.h
        )

target_compile_definitions(CuraEngineConnection
        PUBLIC
        $<$<BOOL:${ENABLE_ARCUS}>:ARCUS>
        $<$<BOOL:${ENABLE_PLUGINS}>:ENABLE_PLUGINS>
        $<$<AND:$<BOOL:${ENABLE_PLUGINS}>,$<BOOL:${ENABLE_REMOTE_PLUGINS}>>:ENABLE_REMOTE_PLUGINS>
        $<$<BOOL:${OLDER_APPLE_CLANG}>:OLDER_APPLE_CLANG>
        CURA_ENGINE_VERSION=\"${CURA_ENGINE_VERSION}\"
        $<$<BOOL:${ENABLE_TESTING}>:BUILD_TESTS>
        PRIVATE
        $<$<BOOL:${WIN32}>:NOMINMAX>
        $<$<CONFIG:Debug>:ASSERT_INSANE_OUTPUT>
        $<$<CONFIG:Debug>:USE_CPU_TIME>
        $<$<CONFIG:Debug>:DEBUG>
        $<$<CONFIG:RelWithDebInfo>:ASSERT_INSANE_OUTPUT>
        $<$<CONFIG:RelWithDebInfo>:USE_CPU_TIME>
        $<$<CONFIG:RelWithDebInfo>:DEBUG>
        )
enable_sanitizers(CuraEngineConnection)

if (${EXTENSIVE_WARNINGS})
    set_project_warnings(CuraEngineConnection)
endif ()

if (ENABLE_ARCUS)
    target_link_libraries(CuraEngineConnection PUBLIC arcus::arcus )
endif ()

find_package(clipper REQUIRED)
find_package(RapidJSON REQUIRED)
find_package(stb REQUIRED)
find_package(Boost REQUIRED)
find_package(spdlog REQUIRED)
find_package(fmt REQUIRED)
find_package(range-v3 REQUIRED)
find_package(scripta REQUIRED)

target_link_libraries(CuraEngineConnection
        PUBLIC
        spdlog::spdlog
        range-v3::range-v3
        fmt::fmt
        clipper::clipper
        rapidjson
        stb::stb
        boost::boost
        scripta::scripta
        curaengine_grpc_definitions::curaengine_grpc_definitions
        asio-grpc::asio-grpc
        grpc::grpc
        protobuf::libprotobuf
        )
