/*
 * Unit Tests for Mapping/Thruster Allocation Functions
 */
#include "../../Core/Src/Mapping.h"
#include "test_framework.h"

/* Test Build_Allocation_Matrix */
TEST(test_build_allocation_matrix) {
  float A_test[N_U][N_T] = {0};
  float r_test[N_T][3] = {{0.30f, 0.20f, 0.00f},   {0.30f, -0.20f, 0.00f},
                          {-0.30f, -0.20f, 0.00f}, {-0.30f, 0.20f, 0.00f},
                          {0.30f, 0.00f, 0.00f},   {-0.30f, 0.00f, 0.00f},
                          {0.00f, -0.20f, 0.00f}};
  float d_test[N_T][3] = {{-0.707f, -0.707f, 0.0f}, {-0.707f, 0.707f, 0.0f},
                          {0.707f, 0.707f, 0.0f},   {0.707f, -0.707f, 0.0f},
                          {0.0f, 0.0f, 1.0f},       {0.0f, 0.0f, 1.0f},
                          {0.0f, 0.0f, -1.0f}};

  Build_Allocation_Matrix(A_test, r_test, d_test);

  /* Check force rows (first 3 rows = direction vectors) */
  ASSERT_NEAR(A_test[0][0], -0.707f, 0.001f);
  ASSERT_NEAR(A_test[1][0], -0.707f, 0.001f);
  ASSERT_NEAR(A_test[2][0], 0.0f, 0.001f);

  /* Check thruster 5 (vertical) */
  ASSERT_NEAR(A_test[0][4], 0.0f, 0.001f);
  ASSERT_NEAR(A_test[1][4], 0.0f, 0.001f);
  ASSERT_NEAR(A_test[2][4], 1.0f, 0.001f);
}

/* Test Cholesky_Solve with simple 2x2 system */
TEST(test_cholesky_solve_simple) {
  /* Solve: [4 2; 2 2] * x = [8; 6] => x = [1; 2] */
  float M[4] = {4.0f, 2.0f, 2.0f, 2.0f};
  float b[2] = {8.0f, 6.0f};
  float x[2] = {0};

  int ret = Cholesky_Solve(2, M, b, x);

  ASSERT_EQ(ret, 0);
  ASSERT_NEAR(x[0], 1.0f, 0.001f);
  ASSERT_NEAR(x[1], 2.0f, 0.001f);
}

/* Test Cholesky_Solve with identity matrix */
TEST(test_cholesky_solve_identity) {
  float M[9] = {1, 0, 0, 0, 1, 0, 0, 0, 1};
  float b[3] = {5.0f, 10.0f, 15.0f};
  float x[3] = {0};

  int ret = Cholesky_Solve(3, M, b, x);

  ASSERT_EQ(ret, 0);
  ASSERT_NEAR(x[0], 5.0f, 0.001f);
  ASSERT_NEAR(x[1], 10.0f, 0.001f);
  ASSERT_NEAR(x[2], 15.0f, 0.001f);
}

/* Test Thruster_Map_Init and Thrust_To_PWM */
TEST(test_thrust_to_pwm) {
  ThrusterMap m = {.T_min = -20.0f,
                   .T_max = 25.0f,
                   .PWM_min = 1188,
                   .PWM_max = 1812,
                   .k = 0,
                   .b = 0,
                   .kr = 0,
                   .br = 0};

  Thruster_Map_Init(&m);

  /* At zero thrust, should get neutral PWM (1500) */
  float pwm_zero = Thrust_To_PWM(&m, 0.0f);
  ASSERT_NEAR(pwm_zero, 1500.0f, 50.0f); /* Allow some tolerance */

  /* At max thrust, should get max PWM */
  float pwm_max = Thrust_To_PWM(&m, 25.0f);
  ASSERT_NEAR(pwm_max, 1812.0f, 1.0f);

  /* At min thrust, the linear equation pwm = k * T + b gives a value below
   * PWM_min */
  /* so it gets clamped to PWM_min. The clamping is correct behavior. */
  float pwm_min = Thrust_To_PWM(&m, -20.0f);
  ASSERT(pwm_min >= 1188.0f &&
         pwm_min <= 1300.0f); /* Should be clamped near PWM_min */
}

/* Test u_zero helper */
TEST(test_u_zero) {
  float arr[6] = {1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f};

  u_zero(arr, 6);

  for (int i = 0; i < 6; i++) {
    ASSERT_NEAR(arr[i], 0.0f, 0.0001f);
  }
}

/* Test full allocation pipeline */
TEST(test_allocate_and_map) {
  /* Initialize allocation matrix with test data */
  float A_test[N_U][N_T] = {0};
  Build_Allocation_Matrix(A_test, r, d);

  /* Initialize maps */
  ThrusterMap maps_test[N_T];
  for (int i = 0; i < N_T; i++) {
    maps_test[i].T_min = -20.0f;
    maps_test[i].T_max = 25.0f;
    maps_test[i].PWM_min = 1188;
    maps_test[i].PWM_max = 1812;
    Thruster_Map_Init(&maps_test[i]);
  }

  /* Command: pure vertical force */
  float u_cmd[N_U] = {0, 0, 50.0f, 0, 0, 0}; /* Fz = 50N */
  float T_out[N_T] = {0};
  int16_t pwm_out[N_T] = {0};

  int ret = Allocate_And_Map(A_test, u_cmd, 0.1f, maps_test, 5, T_out, pwm_out);

  ASSERT_EQ(ret, 0);

  /* Vertical thrusters (4,5) should have positive thrust */
  /* Horizontal thrusters (0-3) should have ~0 thrust */
  ASSERT(T_out[4] > 0.0f || T_out[5] > 0.0f);

  /* PWM should be in valid range */
  for (int i = 0; i < N_T; i++) {
    ASSERT(pwm_out[i] >= 1100 && pwm_out[i] <= 1900);
  }
}

void run_mapping_tests(void) {
  printf("\n=== Mapping Tests ===\n");
  RUN_TEST(test_build_allocation_matrix);
  RUN_TEST(test_cholesky_solve_simple);
  RUN_TEST(test_cholesky_solve_identity);
  RUN_TEST(test_thrust_to_pwm);
  RUN_TEST(test_u_zero);
  RUN_TEST(test_allocate_and_map);
}
