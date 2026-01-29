#ifndef STM32F4XX_H
#define STM32F4XX_H

/* Mock STM32F4xx header for PC testing */

#include <stdbool.h>
#include <stdint.h>


/* Timer channel definitions */
#define TIM_CHANNEL_1 0x00000000U
#define TIM_CHANNEL_2 0x00000004U
#define TIM_CHANNEL_3 0x00000008U
#define TIM_CHANNEL_4 0x0000000CU

/* HAL Status */
typedef enum {
  HAL_OK = 0x00U,
  HAL_ERROR = 0x01U,
  HAL_BUSY = 0x02U,
  HAL_TIMEOUT = 0x03U
} HAL_StatusTypeDef;

/* Timer handle mock */
typedef struct {
  uint32_t Instance;
  uint32_t Channel;
} TIM_HandleTypeDef;

/* Mock HAL functions */
static inline HAL_StatusTypeDef HAL_TIM_PWM_Start(TIM_HandleTypeDef *htim,
                                                  uint32_t Channel) {
  (void)htim;
  (void)Channel;
  return HAL_OK;
}

static inline HAL_StatusTypeDef HAL_TIM_PWM_Stop(TIM_HandleTypeDef *htim,
                                                 uint32_t Channel) {
  (void)htim;
  (void)Channel;
  return HAL_OK;
}

static inline void HAL_Delay(uint32_t Delay) { (void)Delay; }

/* Mock timer compare macros */
#define __HAL_TIM_SET_COMPARE(__HANDLE__, __CHANNEL__, __COMPARE__) ((void)0)
#define __HAL_TIM_GET_COMPARE(__HANDLE__, __CHANNEL__) (1500U)

#endif /* STM32F4XX_H */
