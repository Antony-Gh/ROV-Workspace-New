#ifndef CMSIS_OS_H
#define CMSIS_OS_H

/* Mock CMSIS-RTOS header for PC testing */

#include <stdint.h>

/* FreeRTOS type mocks */
typedef void *SemaphoreHandle_t;
typedef void *TaskHandle_t;
typedef int32_t BaseType_t;
typedef uint32_t TickType_t;

#define pdTRUE 1
#define pdFALSE 0
#define pdPASS 1
#define pdFAIL 0
#define portMAX_DELAY 0xFFFFFFFFUL

/* Priority definitions */
typedef enum {
  osPriorityNone = 0,
  osPriorityIdle = 1,
  osPriorityLow = 8,
  osPriorityBelowNormal = 16,
  osPriorityNormal = 24,
  osPriorityAboveNormal = 32,
  osPriorityHigh = 40,
  osPriorityRealtime = 48
} osPriority_t;

/* Mock FreeRTOS functions */
static inline SemaphoreHandle_t xSemaphoreCreateMutex(void) {
  static int dummy_sem;
  return (SemaphoreHandle_t)&dummy_sem;
}

static inline BaseType_t xSemaphoreTake(SemaphoreHandle_t sem,
                                        TickType_t timeout) {
  (void)sem;
  (void)timeout;
  return pdTRUE;
}

static inline BaseType_t xSemaphoreGive(SemaphoreHandle_t sem) {
  (void)sem;
  return pdTRUE;
}

static inline void vTaskDelay(TickType_t ticks) { (void)ticks; }

static inline TickType_t pdMS_TO_TICKS(uint32_t ms) { return ms; }

static inline BaseType_t xTaskCreate(void (*pvTaskCode)(void *),
                                     const char *pcName, uint16_t usStackDepth,
                                     void *pvParameters,
                                     osPriority_t uxPriority,
                                     TaskHandle_t *pxCreatedTask) {
  (void)pvTaskCode;
  (void)pcName;
  (void)usStackDepth;
  (void)pvParameters;
  (void)uxPriority;
  (void)pxCreatedTask;
  return pdPASS;
}

#endif /* CMSIS_OS_H */
