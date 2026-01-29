#ifndef PRESSURE_SENSOR_H_
#define PRESSURE_SENSOR_H_

#include "main.h"
#include "stm32f4xx.h"

extern SPI_HandleTypeDef *MS5540C_SPI;
extern TIM_HandleTypeDef *MS5540C_MCLK_TIM;

typedef struct {
    float temperature;
    float pressure;
    float depth;
} TPDData;

extern volatile float temperature;
extern volatile float pressure;
extern volatile float depth;

void Pressure_sensor_Setup(SPI_HandleTypeDef *spi, TIM_HandleTypeDef *htim);
void Pressure_sensor_Init();
void Sensor_Reset(void);
uint16_t Read_Calibration_Word(uint8_t cmd1, uint8_t cmd2);
uint16_t Read_Pressure(void);
uint16_t Read_Temperature(void);
TPDData MS5540_Update();

#endif /* PRESSURE_SENSOR_H_ */
/**/
