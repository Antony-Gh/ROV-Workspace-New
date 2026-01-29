#ifndef MAVLINK_MESSAGES_H
#define MAVLINK_MESSAGES_H

#include "main.h"
#include "stm32f4xx.h"
#include "mavlink/all/mavlink.h"
#include <stdbool.h>
#include <stdint.h>
#include <string.h>
#include "motor_interface.h"
#include "PID.h"


#define MAVLINK_BUFFER_SIZE 256
extern uint8_t Mavlink_rx_buffer[];
extern uint8_t Mavlink_tx_buffer[];
extern int BasicMov[3][3][3];
typedef struct
{
	UART_HandleTypeDef *huart;
	uint8_t SystemID;
	uint8_t ComponentID;
} MavLink;
/////////////////////////////////////////
void MavlinkTx_Process(void);
void UpdateAttitude(float roll, float pitch, float yaw);
void UpdateVfrHud(float speed, float heading, float depth);
//////////////////////////

void Set_mavlink(UART_HandleTypeDef *huart, uint8_t SYSID, uint8_t COMPID);
void Start_UART_DMA_Receive();
void send_heartbeat_msg(void);
void Send_status_msg(uint8_t SYSID, uint8_t COMPID, const char* text, uint8_t severity);
//void Send_vfrhud_MSH(float speed,float heading, float depth);
//void Send_attitude_msg(float roll, float pitch, float yaw);
void Send_Water_msg(float pressure, float temp);
void Send_tube_msg(float pressure, float temp);

void Msg_processing(mavlink_message_t* msg);
#endif
/**/
