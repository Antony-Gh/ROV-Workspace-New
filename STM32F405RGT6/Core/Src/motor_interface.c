#include "motor_interface.h"
#include "stm32f4xx.h"

Motor motors[MAX_MOTORS];
volatile bool IsCalibratedESC = false;
volatile bool thrusters_enabled = false;

void Motors_Init_Values() {
  for (uint8_t i = 0; i < MAX_MOTORS; i++) {
    motors[i].id = i;
    motors[i].htim = 0;
    motors[i].channel = 0;
    motors[i].speed = MOTOR_SPEED_MIN;
    motors[i].direction = MOTOR_DIRECTION_STOP;
    motors[i].enabled = 0;
    motors[i].ramp_rate = 0;
    motors[i].state = MOTOR_STATUS_OK;
  }
}

void Motor_Set_timer(uint8_t motor_id, TIM_HandleTypeDef *htim,
                     uint32_t channel) {
  motors[motor_id].htim = htim;
  motors[motor_id].channel = channel;
}

void Motor_CalibrateESC(uint8_t motor_id) {
  if (motor_id == 0xFF) {
    for (uint8_t i = 0; i < MAX_MOTORS; i++) {

      HAL_TIM_PWM_Start(motors[i].htim, GetTimChannel(motors[i].channel));
      __HAL_TIM_SET_COMPARE(motors[i].htim, GetTimChannel(motors[i].channel),
                            PWM_NEUTRAL);
      motors[i].enabled = 1;
    }
  } else if (motor_id >= 0 && motor_id <= 6) {
    HAL_TIM_PWM_Start(motors[motor_id].htim,
                      GetTimChannel(motors[motor_id].channel));
    __HAL_TIM_SET_COMPARE(motors[motor_id].htim,
                          GetTimChannel(motors[motor_id].channel), PWM_NEUTRAL);
    motors[motor_id].enabled = 1;
  }
  osDelay(10000); /* Use osDelay instead of HAL_Delay for FreeRTOS compatibility */
  IsCalibratedESC = true;
}

void Motor_Enable(uint8_t motor_id) {
  if (motor_id == 0xFF) {
    if (!IsCalibratedESC) {
      Motor_CalibrateESC(0xFF);
      thrusters_enabled = true;
    } else if (!thrusters_enabled) {
      for (uint8_t i = 0; i < MAX_MOTORS; i++) {
        HAL_TIM_PWM_Start(motors[i].htim, GetTimChannel(motors[i].channel));
        __HAL_TIM_SET_COMPARE(motors[i].htim, GetTimChannel(motors[i].channel),
                              PWM_NEUTRAL);
        motors[i].enabled = 1;
      }
      thrusters_enabled = true;
    }
  } else if (motor_id >= 0 && motor_id <= 6) {
    Motor_CalibrateESC(motor_id);
  }
}

void Motor_SetSpeed(uint8_t motor_id, int16_t speed) {
  if (motor_id >= MAX_MOTORS)
    return; /* Bounds check */
  if (motors[motor_id].htim == NULL)
    return; /* NULL check */
  if (thrusters_enabled) {
    __HAL_TIM_SET_COMPARE(motors[motor_id].htim,
                          GetTimChannel(motors[motor_id].channel), speed);
  }
}

void SetSpeed(int16_t *PWM) {
  for (uint8_t i = 0; i < MAX_MOTORS; i++) {
    Motor_SetSpeed(i, PWM[i]);
  }
}

void Motor_SetDirection(uint8_t motor_id, MotorDirection direction) {
  motors[motor_id].direction = direction;
}

void Motor_Stop(uint8_t motor_id) {
  if (motor_id == 0xFF) {
    for (uint8_t i = 0; i < MAX_MOTORS; i++) {
      Motor_SetSpeed(i, PWM_NEUTRAL);
    }
  } else if (motor_id >= 0 && motor_id <= MAX_MOTORS) {
    Motor_SetSpeed(motor_id, PWM_NEUTRAL);
  }
}

void Motor_Disable(uint8_t motor_id) {
  if (motor_id == 0xFF) {
    for (uint8_t i = 0; i < MAX_MOTORS; i++) {
      Motor_Stop(i);
      HAL_TIM_PWM_Stop(motors[i].htim, GetTimChannel(motors[i].channel));
      motors[i].enabled = 0;
    }
    thrusters_enabled = false;
  } else if (motor_id >= 0 && motor_id <= 6) {
    Motor_Stop(motor_id);
    HAL_TIM_PWM_Stop(motors[motor_id].htim,
                     GetTimChannel(motors[motor_id].channel));
    motors[motor_id].enabled = 0;
  }
}
uint32_t GetTimChannel(uint8_t ch) {
  switch (ch) {
  case 1:
    return TIM_CHANNEL_1;
  case 2:
    return TIM_CHANNEL_2;
  case 3:
    return TIM_CHANNEL_3;
  case 4:
    return TIM_CHANNEL_4;
  default:
    return 0;
  }
}
uint8_t Motor_GetSpeed(uint8_t motor_id) { return motors[motor_id].speed; }

MotorDirection Motor_GetDirection(uint8_t motor_id) {
  return motors[motor_id].direction;
}

MotorStatus Motor_CheckStatus(uint8_t motor_id) {
  /*if(Move.move)
  {
          if(motors[motor_id].enabled)
          {
                  if(HAL_TIM_PWM_Start(motors[motor_id].htim,
  GetTimChannel(motors[motor_id].channel)) == HAL_OK)
                  {
                          if(motors[motor_id].direction)
                          {
                          //if(motors[motor_id].current)
                          motors[motor_id].state = MOTOR_STATUS_RUNNIG;
                          //else
                          motors[motor_id].state = MOTOR_STATUS_FAULT;
                          }
                          else
                          {
                                  if
  (__HAL_TIM_GET_COMPARE(motors[motor_id].htim,
  GetTimChannel(motors[motor_id].channel)) == 1500)
                                  {
                                          //status = MOTOR_STOPPED;
                                  }
                          }
                  }
                  else
                          motors[motor_id].state = MOTOR_STATUS_FAULT;
          }
          else
                  motors[motor_id].state = MOTOR_STATUS_FAULT;
  }
  else
  {
  }*/
  return motors[motor_id].state;
}

void Motor_HandleFault(uint8_t motor_id) {
  Motor_Disable(motor_id);
  Motor_Enable(motor_id);
}
void Move_Update(float Fx, float Fy, float Fz, float Mz) {
  u_zero(u, 6);
  if (fabsf(Fz) >= 0) { /* Use fabsf for float */
    u[2] = Fz;
    int ok = Allocate_And_Map(A, u, lambda, maps, 5, T, PWM);
    if (!ok) {
      SetSpeed(PWM);
    }
  } else {
    u[0] = Fx;
    u[1] = Fy;
    u[5] = Mz;
    int ok = Allocate_And_Map(A, u, lambda, maps, 5, T, PWM);
    if (!ok) {
      SetSpeed(PWM);
    }
  }
}
/**/
