#ifndef MPU6050_H_
#define MPU6050_H_

#include "stm32f4xx_hal.h"
#include <math.h>
#include <stdbool.h>

typedef struct {
    float pitch;
    float roll;
    float yaw;
}IMUData;

#define MPU6050_ADDR_LOW  (0x68 << 1)
#define MPU6050_ADDR_HIGH (0x69 << 1)
#define MPU6050_ADDR MPU6050_ADDR_LOW
extern I2C_HandleTypeDef hi2c1;
extern volatile float roll;
extern volatile float pitch;
extern volatile float yaw;
bool MPU6050_Init(void);
bool MPU6050_ReadWhoAmI(uint8_t *whoami);
IMUData MPU6050_Update(float);

#endif
/**/
