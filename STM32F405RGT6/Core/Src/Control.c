
#include "Control.h"

/**/

static IMUData imudata;
static TPDData tpddata;
static float depthSP;
static SemaphoreHandle_t IMUMutex;
static SemaphoreHandle_t TPDMutex;
static float Fz_cmd4;
static float Fz_cmd5;
static float error1;
static float error2;

PID depth_pi = {.Kp = 2.0f,
                .Ki = 0.3f,
                .Kd = 0.0f,
                .out_min = -60.0f,
                .out_max = 60.0f,
                .anti_windup_beta = 0.5f};

void IMUTask(void *argument) {
  TASK_LOOP {
    IMUData read = MPU6050_Update(0.02f);
    if (xSemaphoreTake(IMUMutex, portMAX_DELAY)) {
      imudata = read;
      UpdateAttitude(imudata.roll, imudata.pitch, imudata.yaw);
      xSemaphoreGive(IMUMutex);
    }
    MavlinkTx_Process();

    vTaskDelay(pdMS_TO_TICKS(20));
  }
}
void TPDTask(void *argument) {
  TASK_LOOP {
    TPDData read = MS5540_Update(0.02f);
    if (xSemaphoreTake(TPDMutex, portMAX_DELAY)) {
      tpddata = read;
      UpdateVfrHud(0, 0, read.depth);
      xSemaphoreGive(TPDMutex);
    }
    vTaskDelay(pdMS_TO_TICKS(20));
  }
}
void PIDTask(void *argument) {
  TASK_LOOP {
    if (xSemaphoreTake(TPDMutex, portMAX_DELAY)) {
      error1 =
          (tpddata.depth - 50 * sin(imudata.pitch * M_PI / 180.0)) - depthSP;
      error2 = tpddata.depth - depthSP;
      Fz_cmd4 = pid_update(&depth_pi, error1, 0.02);
      Fz_cmd5 = pid_update(&depth_pi, error2, 0.02);
      float u4[6] = {0, 0, Fz_cmd4, 0.0f, 0.0f, 0};
      float u5[6] = {0, 0, Fz_cmd5, 0.0f, 0.0f, 0};
      int ok4 = Allocate_And_Map(AT4, u4, lambda, maps, 5, T, PWM4);
      int ok5 = Allocate_And_Map(AT5, u5, lambda, maps, 5, T, PWM5);
      if (!ok4 & !ok5) {
        Motor_SetSpeed(4, PWM4[4]);
        Motor_SetSpeed(5, PWM5[5]);
      }
    }
    vTaskDelay(pdMS_TO_TICKS(100));
  }
}
void VPID(float e1, float e2) {
  /*
  Fz_cmd4 = pid_update(&depth_pi, e1, 0.01);
  Fz_cmd5 = pid_update(&depth_pi, e2, 0.01);
  float u4[6] = {0, 0, Fz_cmd4, 0.0f, 0.0f, 0};
  float u5[6] = {0, 0, Fz_cmd5, 0.0f, 0.0f, 0};
  */
}
void build_allocation_matrix0(int rows, int cols, float A[rows][cols]) {}
void System_Init() {
  depthSP = 0;

  Build_Allocation_Matrix(AT4, r, dT4);
  Build_Allocation_Matrix(AT5, r, dT5);
  Build_Allocation_Matrix(A, r, d);

  IMUMutex = xSemaphoreCreateMutex();
  TPDMutex = xSemaphoreCreateMutex();
  xTaskCreate(IMUTask, "IMU", 512, NULL, osPriorityNormal, NULL);
  xTaskCreate(TPDTask, "TPD", 384, NULL, osPriorityNormal, NULL);
  xTaskCreate(PIDTask, "PID", 384, NULL, osPriorityNormal, NULL);
  // xTaskCreate(ControlTask,  "Control",  512, NULL, osPriorityHigh,   NULL);
}
