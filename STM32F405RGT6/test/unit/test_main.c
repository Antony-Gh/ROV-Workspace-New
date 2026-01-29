/*
 * Main Test Runner for ROV Unit Tests
 */
#include <stdio.h>

#include "test_framework.h"

/* Define test counters (declared extern in test_framework.h) */
int tests_run = 0;
int tests_passed = 0;
int current_test_failed = 0;

/* External test runners */
extern void run_mapping_tests(void);
extern void run_pid_tests(void);

int main(void) {
  printf("===========================================\n");
  printf("     ROV Firmware Unit Tests\n");
  printf("===========================================\n");

  run_mapping_tests();
  run_pid_tests();

  TEST_SUMMARY();

  return (tests_run == tests_passed) ? 0 : 1;
}
