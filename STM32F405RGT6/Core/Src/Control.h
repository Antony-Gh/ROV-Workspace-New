

#ifndef SRC_CONTROL_H_
#define SRC_CONTROL_H_

#include "FreeRTOS.h"
#include "cmsis_os.h"
#include "mavlink_messages.h"
#include "mpu6050.h"
#include "pressure_sensor.h"
#include "semphr.h"
#include "stm32f4xx.h"


void System_Init();

#ifndef TASK_LOOP
#define TASK_LOOP for (;;)
#endif

#endif /* SRC_CONTROL_H_ */
/**/
