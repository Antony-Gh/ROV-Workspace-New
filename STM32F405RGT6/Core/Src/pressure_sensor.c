#include "pressure_sensor.h"
#include "math.h"
#include "stdio.h"

SPI_HandleTypeDef *MS5540C_SPI;
TIM_HandleTypeDef *MS5540C_MCLK_TIM;
static uint16_t D1 = 0, D2 = 0;
static uint16_t c1 = 0, c2 = 0, c3 = 0, c4 = 0, c5 = 0, c6 = 0;
volatile float temperature;
volatile float pressure;
volatile float depth;

void Pressure_sensor_Setup(SPI_HandleTypeDef *spi, TIM_HandleTypeDef *htim)
{
	MS5540C_SPI = spi;
	MS5540C_MCLK_TIM = htim;
}
void Pressure_sensor_Init()
{
	  HAL_TIM_PWM_Start(MS5540C_MCLK_TIM, TIM_CHANNEL_1);
	  HAL_Delay(100);
	  HAL_SPI_Init(MS5540C_SPI);
	  c1 = Read_Calibration_Word(0x1D, 0x50);
	  c2 = Read_Calibration_Word(0x1D, 0x60);
	  c3 = Read_Calibration_Word(0x1D, 0x90);
	  c4 = Read_Calibration_Word(0x1D, 0xA0);
	  c1 = (c1 >> 1) & 0x7FFF;
	  c5 = ((c1 & 0x0001) << 10) | ((c2 >> 6) & 0x03FF);
	  c6 = c2 & 0x003F;
	  c2 = ((c3 & 0x003F) << 6) | (c4 & 0x003F);
	  c3 = (c4 >> 6) & 0x03FF;
	  c4 = (c3 >> 6) & 0x03FF;
}
void Sensor_Reset(void)
{
  uint8_t reset_cmd[] = {0x15, 0x55, 0x40};
  HAL_SPI_Transmit(MS5540C_SPI, reset_cmd, sizeof(reset_cmd), HAL_MAX_DELAY);
}
uint16_t Read_Calibration_Word(uint8_t cmd1, uint8_t cmd2)
{
  uint8_t tx_buf[2] = {cmd1, cmd2};
  uint8_t rx_buf[2] = {0};
  Sensor_Reset();
  HAL_SPI_Transmit(MS5540C_SPI, tx_buf, sizeof(tx_buf), HAL_MAX_DELAY);
  MS5540C_SPI->Init.CLKPolarity = SPI_POLARITY_LOW;
  MS5540C_SPI->Init.CLKPhase = SPI_PHASE_2EDGE;
  HAL_SPI_Init(MS5540C_SPI);
  HAL_SPI_Receive(MS5540C_SPI, rx_buf, sizeof(rx_buf), HAL_MAX_DELAY);
  MS5540C_SPI->Init.CLKPolarity = SPI_POLARITY_LOW;
  MS5540C_SPI->Init.CLKPhase = SPI_PHASE_1EDGE;
  HAL_SPI_Init(MS5540C_SPI);
  return (rx_buf[0] << 8) | rx_buf[1];
}
uint16_t Read_Pressure(void)
{
  uint8_t press_cmd[] = {0x0F, 0x40};
  uint8_t rx_buf[2] = {0};
  Sensor_Reset();
  HAL_SPI_Transmit(MS5540C_SPI, press_cmd, sizeof(press_cmd), HAL_MAX_DELAY);
  HAL_Delay(35);
  MS5540C_SPI->Init.CLKPolarity = SPI_POLARITY_LOW;
  MS5540C_SPI->Init.CLKPhase = SPI_PHASE_2EDGE;
  HAL_SPI_Init(MS5540C_SPI);
  HAL_SPI_Receive(MS5540C_SPI, rx_buf, sizeof(rx_buf), HAL_MAX_DELAY);
  MS5540C_SPI->Init.CLKPolarity = SPI_POLARITY_LOW;
  MS5540C_SPI->Init.CLKPhase = SPI_PHASE_1EDGE;
  HAL_SPI_Init(MS5540C_SPI);
  return (rx_buf[0] << 8) | rx_buf[1];
}
uint16_t Read_Temperature(void)
{
  uint8_t temp_cmd[] = {0x0F, 0x20};
  uint8_t rx_buf[2] = {0};
  //Sensor_Reset();
  HAL_SPI_Transmit(MS5540C_SPI, temp_cmd, sizeof(temp_cmd), HAL_MAX_DELAY);
  HAL_Delay(35);
  MS5540C_SPI->Init.CLKPolarity = SPI_POLARITY_LOW;
  MS5540C_SPI->Init.CLKPhase = SPI_PHASE_2EDGE;
  HAL_SPI_Init(MS5540C_SPI);
  HAL_SPI_Receive(MS5540C_SPI, rx_buf, sizeof(rx_buf), HAL_MAX_DELAY);
  MS5540C_SPI->Init.CLKPolarity = SPI_POLARITY_LOW;
  MS5540C_SPI->Init.CLKPhase = SPI_PHASE_1EDGE;
  HAL_SPI_Init(MS5540C_SPI);
  return (rx_buf[0] << 8) | rx_buf[1];
}
TPDData MS5540_Update()
{
	TPDData TPDdata = {0};
	D1 = Read_Pressure();
	D2 = Read_Temperature();
	long UT1 = (c5 << 3) + 20224;
	long dT = D2 - UT1;
	long TEMP = 200 + ((dT * (c6 + 50)) >> 10);
	long OFF = (c2 * 4) + (((c4 - 512) * dT) >> 12);
	long SENS = c1 + ((c3 * dT) >> 10) + 24576;
	long X = ((SENS * (D1 - 7168)) >> 14) - OFF;
	long PCOMP = ((X * 10) >> 5) + 2500;
	TPDdata.temperature = TEMP / 10.0f;
	TPDdata.pressure = PCOMP / 10.0f;
	float pressure_diff = pressure - 1009.0f;
	pressure_diff = roundf(pressure_diff*100.0f)/100.0f;
	TPDdata.depth = (pressure_diff > 0) ? pressure_diff * 1.02f : 0.0f;
	return TPDdata;
}
/**/
