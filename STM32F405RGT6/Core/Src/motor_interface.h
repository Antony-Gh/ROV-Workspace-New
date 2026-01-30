#ifndef MOTOR_INTERFACE_H
#define MOTOR_INTERFACE_H

#include "Mapping.h"
#include "cmsis_os.h"
#include "stm32f4xx.h"
#include <stdbool.h>
#include <stdint.h>
#include <stdlib.h>


#define MAX_MOTORS 6
#define PWM_MAX 20000
#define MOTOR_SPEED_MIN 0
#define MOTOR_SPEED_MAX 100
#define PWM_NEUTRAL 1500
extern volatile bool IsCalibratedESC;
extern volatile bool thrusters_enabled;

typedef enum { move = 1, off = 0, stop = -1 } movement_state;

typedef struct {
  movement_state move;
} Movement_State;

typedef enum {
  MOTOR_DIRECTION_REVERSE = -1,
  MOTOR_DIRECTION_STOP = 0,
  MOTOR_DIRECTION_FORWARD = 1
} MotorDirection;

typedef enum {
  MOTOR_STATUS_OK,
  MOTOR_STATUS_FAULT,
  MOTOR_STATUS_RUNNIG,
  MOTOR_STATUS_STOPPED,
  MOTOR_STATUS_UNKNOWN
} MotorStatus;

typedef struct {
  uint8_t id;
  TIM_HandleTypeDef *htim;  // Timer handle for PWM
  uint32_t channel;         // Timer channel for PWM
  uint8_t speed;            // Current speed (0-100%)
  MotorDirection direction; // Current direction
  uint8_t enabled;          // Motor enabled/disabled
  uint8_t ramp_rate;        // Ramp rate for speed changes
  uint32_t current_value;
  MotorStatus state;
} Motor;

void Motors_Init_Values();

void Motor_Set_timer(uint8_t motor_id, TIM_HandleTypeDef *htim,
                     uint32_t channel);

void Motor_CalibrateESC(uint8_t);

void Motor_Enable(uint8_t);

void Motor_Disable(uint8_t);

void SetSpeed(int16_t *);

void Motor_SetSpeed(uint8_t, int16_t);

void Motor_SetDirection(uint8_t, MotorDirection);

void Motor_Stop(uint8_t);

uint8_t Motor_GetSpeed(uint8_t);

uint32_t GetTimChannel(uint8_t);

MotorDirection Motor_GetDirection(uint8_t);

void Move_Update(float, float, float, float);

#endif
/**/
