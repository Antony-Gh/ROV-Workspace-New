/*
 * Minimal Test Framework for ROV Unit Tests
 */
#ifndef TEST_FRAMEWORK_H
#define TEST_FRAMEWORK_H

#include <math.h>
#include <stdio.h>


/* Test counters - defined in test_main.c */
extern int tests_run;
extern int tests_passed;
extern int current_test_failed;

#define TEST(name) static void name(void)

#define ASSERT(cond)                                                           \
  do {                                                                         \
    if (!(cond)) {                                                             \
      printf("  FAIL: %s:%d: %s\n", __FILE__, __LINE__, #cond);                \
      current_test_failed = 1;                                                 \
      return;                                                                  \
    }                                                                          \
  } while (0)

#define ASSERT_EQ(a, b) ASSERT((a) == (b))

#define ASSERT_NEAR(a, b, tol)                                                 \
  do {                                                                         \
    float _a = (float)(a), _b = (float)(b), _t = (float)(tol);                 \
    if (fabsf(_a - _b) > _t) {                                                 \
      printf("  FAIL: %s:%d: |%.6f - %.6f| > %.6f\n", __FILE__, __LINE__, _a,  \
             _b, _t);                                                          \
      current_test_failed = 1;                                                 \
      return;                                                                  \
    }                                                                          \
  } while (0)

#define RUN_TEST(test)                                                         \
  do {                                                                         \
    tests_run++;                                                               \
    current_test_failed = 0;                                                   \
    printf("Running %s...\n", #test);                                          \
    test();                                                                    \
    if (!current_test_failed) {                                                \
      tests_passed++;                                                          \
      printf("  PASS\n");                                                      \
    }                                                                          \
  } while (0)

#define TEST_SUMMARY()                                                         \
  do {                                                                         \
    printf("\n========================================\n");                    \
    printf("Tests: %d | Passed: %d | Failed: %d\n", tests_run, tests_passed,   \
           tests_run - tests_passed);                                          \
    printf("========================================\n");                      \
  } while (0)

#endif /* TEST_FRAMEWORK_H */
