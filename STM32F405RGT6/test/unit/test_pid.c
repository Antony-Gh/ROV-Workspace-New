/*
 * Unit Tests for PID Controller
 */
#include "../../Core/Src/PID.h"
#include "test_framework.h"


/* Test clampf function */
TEST(test_clampf_in_range) {
  float result = clampf(5.0f, 0.0f, 10.0f);
  ASSERT_NEAR(result, 5.0f, 0.001f);
}

TEST(test_clampf_below_min) {
  float result = clampf(-5.0f, 0.0f, 10.0f);
  ASSERT_NEAR(result, 0.0f, 0.001f);
}

TEST(test_clampf_above_max) {
  float result = clampf(15.0f, 0.0f, 10.0f);
  ASSERT_NEAR(result, 10.0f, 0.001f);
}

/* Test basic PID response */
TEST(test_pid_proportional) {
  PID pid = {.Kp = 2.0f,
             .Ki = 0.0f,
             .Kd = 0.0f,
             .D_alpha = 0.1f,
             .out_min = -100.0f,
             .out_max = 100.0f,
             .anti_windup_beta = 0.5f,
             .integrator = 0.0f,
             .prev_error = 0.0f,
             .D_lpf = 0.0f};

  /* Pure proportional: output = Kp * error = 2.0 * 10 = 20 */
  float output = pid_update(&pid, 10.0f, 0.01f);
  ASSERT_NEAR(output, 20.0f, 1.0f); /* Allow tolerance for D term noise */
}

/* Test PID with integral action */
TEST(test_pid_integral) {
  PID pid = {.Kp = 0.0f,
             .Ki = 1.0f,
             .Kd = 0.0f,
             .D_alpha = 0.1f,
             .out_min = -100.0f,
             .out_max = 100.0f,
             .anti_windup_beta = 0.0f, /* No anti-windup for this test */
             .integrator = 0.0f,
             .prev_error = 0.0f,
             .D_lpf = 0.0f};

  float dt = 0.1f;
  float error = 5.0f;

  /* After one step: integrator += Ki * error * dt = 1.0 * 5.0 * 0.1 = 0.5 */
  float out1 = pid_update(&pid, error, dt);
  ASSERT_NEAR(out1, 0.5f, 0.01f);

  /* After second step: integrator += 0.5 => 1.0 */
  float out2 = pid_update(&pid, error, dt);
  ASSERT_NEAR(out2, 1.0f, 0.01f);
}

/* Test PID output saturation */
TEST(test_pid_saturation) {
  PID pid = {.Kp = 10.0f,
             .Ki = 0.0f,
             .Kd = 0.0f,
             .D_alpha = 0.1f,
             .out_min = -50.0f,
             .out_max = 50.0f,
             .anti_windup_beta = 0.5f,
             .integrator = 0.0f,
             .prev_error = 0.0f,
             .D_lpf = 0.0f};

  /* Kp * error = 10 * 100 = 1000, but should be clamped to 50 */
  float output = pid_update(&pid, 100.0f, 0.01f);
  ASSERT_NEAR(output, 50.0f, 0.01f);
}

/* Test SetPID function */
TEST(test_set_pid) {
  PID pid = {0};

  SetPID(&pid, 1.0f, 2.0f, 3.0f, 0.5f, 100.0f, -100.0f, 0.8f);

  ASSERT_NEAR(pid.Kp, 1.0f, 0.001f);
  ASSERT_NEAR(pid.Ki, 2.0f, 0.001f);
  ASSERT_NEAR(pid.Kd, 3.0f, 0.001f);
  ASSERT_NEAR(pid.D_alpha, 0.5f, 0.001f);
  ASSERT_NEAR(pid.out_max, 100.0f, 0.001f);
  ASSERT_NEAR(pid.out_min, -100.0f, 0.001f);
  ASSERT_NEAR(pid.anti_windup_beta, 0.8f, 0.001f);
}

/* Test anti-windup */
TEST(test_pid_anti_windup) {
  PID pid = {.Kp = 1.0f,
             .Ki = 10.0f,
             .Kd = 0.0f,
             .D_alpha = 0.1f,
             .out_min = -10.0f,
             .out_max = 10.0f,
             .anti_windup_beta = 1.0f, /* Full anti-windup */
             .integrator = 0.0f,
             .prev_error = 0.0f,
             .D_lpf = 0.0f};

  /* Large error should saturate and anti-windup should prevent integrator
   * runaway */
  for (int i = 0; i < 100; i++) {
    pid_update(&pid, 100.0f, 0.01f);
  }

  /* Integrator should be bounded (not run away to huge values) */
  ASSERT(pid.integrator < 100.0f);
}

void run_pid_tests(void) {
  printf("\n=== PID Tests ===\n");
  RUN_TEST(test_clampf_in_range);
  RUN_TEST(test_clampf_below_min);
  RUN_TEST(test_clampf_above_max);
  RUN_TEST(test_pid_proportional);
  RUN_TEST(test_pid_integral);
  RUN_TEST(test_pid_saturation);
  RUN_TEST(test_set_pid);
  RUN_TEST(test_pid_anti_windup);
}
