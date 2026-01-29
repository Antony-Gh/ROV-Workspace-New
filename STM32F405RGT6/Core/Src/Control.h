

#ifndef SRC_CONTROL_H_
#define SRC_CONTROL_H_

#include "stm32f4xx.h"
#include "cmsis_os.h"
#include "FreeRTOS.h"
#include "semphr.h"
#include "mpu6050.h"
#include "pressure_sensor.h"
#include "mavlink_messages.h"

void System_Init();

#endif /* SRC_CONTROL_H_ */
/**/
