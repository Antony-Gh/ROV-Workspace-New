cmake_minimum_required(VERSION 3.16)
project(rov_tests C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)

# Add mock headers first (to override real STM32 headers)
include_directories(
    ${CMAKE_SOURCE_DIR}/mocks
    ${CMAKE_SOURCE_DIR}/../Core/Src
)

# Source files from main project (logic only, no HAL dependencies)
set(SRC_FILES
    ${CMAKE_SOURCE_DIR}/../Core/Src/Mapping.c
    ${CMAKE_SOURCE_DIR}/../Core/Src/PID.c
)

# Test files
set(TEST_FILES
    ${CMAKE_SOURCE_DIR}/unit/test_main.c
    ${CMAKE_SOURCE_DIR}/unit/test_mapping.c
    ${CMAKE_SOURCE_DIR}/unit/test_pid.c
)

# Build test executable
add_executable(test_rov
    ${SRC_FILES}
    ${TEST_FILES}
)

# Link math library on Unix-like systems
if(UNIX)
    target_link_libraries(test_rov m)
endif()

# Compiler warnings
if(MSVC)
    target_compile_options(test_rov PRIVATE /W4)
else()
    target_compile_options(test_rov PRIVATE -Wall -Wextra -Wpedantic)
endif()

# Enable testing
enable_testing()
add_test(NAME rov_unit_tests COMMAND test_rov)

# Custom target for running tests
add_custom_target(run_tests
    COMMAND test_rov
    DEPENDS test_rov
    COMMENT "Running ROV unit tests..."
)
