#include "mavlink_messages.h"

static volatile bool uart_tx_busy = false;

static mavlink_message_t latest_attitude_msg;
static bool attitude_msg_ready = false;

static mavlink_message_t latest_vfrhud_msg;
static bool vfrhud_msg_ready = false;

static uint8_t tx_buffer[MAVLINK_MAX_PACKET_LEN];
static mavlink_status_t status;
static mavlink_message_t msg;

MavLink mavlink;
uint8_t Mavlink_rx_buffer[MAVLINK_BUFFER_SIZE];
uint8_t Mavlink_tx_buffer[MAVLINK_BUFFER_SIZE];
int BasicMov[3][3][3] =
{
    {
        { 5, 5, 5 },
        { 3, 3, 3 },
        { 7, 7, 7 }
    },
    {
        { 0, 0, 0 },
        { 9,-1, 8 },
        { 1, 1, 1 }
    },
    {
        { 4, 4, 4 },
        { 2, 2, 2 },
        { 6, 6, 6 }
    }
};
void Set_mavlink(UART_HandleTypeDef *huart, uint8_t SYSID, uint8_t COMPID)
{
	mavlink.huart = huart;
	mavlink.SystemID = SYSID;
	mavlink.ComponentID = COMPID;
}
void Start_UART_DMA_Receive()
{
    HAL_UARTEx_ReceiveToIdle_DMA(mavlink.huart, Mavlink_rx_buffer, MAVLINK_BUFFER_SIZE);
}
void HAL_UART_TxCpltCallback(UART_HandleTypeDef *huart)
{
    if (huart == mavlink.huart) {
        uart_tx_busy = false;
    }
}
void HAL_UARTEx_RxEventCallback(UART_HandleTypeDef *huart, uint16_t Size)
{
    if (huart == mavlink.huart)
    {
        for (uint16_t i = 0; i < Size; i++)
        {
            if (mavlink_parse_char(MAVLINK_COMM_0, Mavlink_rx_buffer[i], &msg, &status))
            {
                Msg_processing(&msg);
            }
        }
        if (HAL_UARTEx_ReceiveToIdle_DMA(huart, Mavlink_rx_buffer, MAVLINK_BUFFER_SIZE) != HAL_OK)
	    {
                   // Handle error
	    }
    }
}
void HAL_UART_ErrorCallback(UART_HandleTypeDef *huart)
{
    if (huart == mavlink.huart)
    {
        HAL_UARTEx_ReceiveToIdle_DMA(huart, Mavlink_rx_buffer, MAVLINK_BUFFER_SIZE);
    }
}
void send_heartbeat_msg(void) {
    mavlink_message_t msg;
    uint8_t buf[MAVLINK_MAX_PACKET_LEN];

    mavlink_msg_heartbeat_pack(
    	mavlink.SystemID,
		mavlink.ComponentID,
        &msg,
        MAV_TYPE_QUADROTOR,
        MAV_AUTOPILOT_GENERIC,
        MAV_MODE_GUIDED_ARMED,
        0,
        MAV_STATE_ACTIVE
    );

    uint16_t len = mavlink_msg_to_send_buffer(buf, &msg);
    HAL_UART_Transmit_DMA(mavlink.huart, buf, len);
}
void Send_status_msg(uint8_t SYSID, uint8_t COMPID, const char* text, uint8_t severity)
{
    mavlink_message_t msg;
    uint8_t id = 0;
    uint8_t chunk_seq = 0;
    uint16_t text_len = strlen(text);
    for (uint16_t i = 0; i < text_len; i += 50)
    {
    	char chunk[50] = {0};
    	strncpy(chunk, &text[i], 50);
    	if (strlen(chunk) == 0)
    		break;
        mavlink_msg_statustext_pack(SYSID, COMPID, &msg, severity, chunk, id, chunk_seq);
        uint8_t buffer[MAVLINK_MAX_PACKET_LEN];
        uint16_t bytes_to_send = mavlink_msg_to_send_buffer(buffer, &msg);
        if (!uart_tx_busy)
        {
        	uart_tx_busy = true;
        	HAL_UART_Transmit_DMA(mavlink.huart, buffer, bytes_to_send);
            HAL_GPIO_TogglePin(GPIOA, GPIO_PIN_5);

        }
        if (strlen(chunk) < 50)
        	break;
        chunk_seq++;
    }
}
void UpdateAttitude(float roll, float pitch, float yaw)
{
    mavlink_attitude_t attitude = {
        .time_boot_ms = HAL_GetTick(),
        .roll = roll,
        .pitch = pitch,
        .yaw = yaw,
        .rollspeed = 0,
        .pitchspeed = 0,
        .yawspeed = 0

    };
    mavlink_msg_attitude_encode(1, MAV_COMP_ID_AUTOPILOT1, &latest_attitude_msg, &attitude);
    attitude_msg_ready = true;
}
void UpdateVfrHud(float speed, float heading, float depth)
{
	mavlink_msg_vfr_hud_pack(1, 1, &latest_vfrhud_msg, 0, speed, heading, 0, depth, 0);
	vfrhud_msg_ready = true;
}
void MavlinkTx_Process(void) {
    if (!uart_tx_busy) {
        if (attitude_msg_ready)
        {
            uint16_t len = mavlink_msg_to_send_buffer(tx_buffer, &latest_attitude_msg);
            uart_tx_busy = true;
            HAL_UART_Transmit_DMA(mavlink.huart, tx_buffer, len);
            attitude_msg_ready = false;
        }
        if (vfrhud_msg_ready)
        {
            uint16_t len = mavlink_msg_to_send_buffer(tx_buffer, &latest_vfrhud_msg);
            uart_tx_busy = true;
            HAL_UART_Transmit_DMA(mavlink.huart, tx_buffer, len);
            vfrhud_msg_ready = false;
        }
    }
}
/*
void Send_vfrhud_MSH(float speed, float heading, float depth)
{
    if (!uart_tx_ready) return;

    mavlink_message_t msg;
    uint8_t buffer[MAVLINK_MAX_PACKET_LEN];

    mavlink_msg_vfr_hud_pack(1, 1, &msg, 0, speed, heading, 0, depth, 0);
    uint16_t len = mavlink_msg_to_send_buffer(buffer, &msg);

    uart_tx_ready = false;
    HAL_UART_Transmit_DMA(mavlink.huart, buffer, len);
}
void Send_attitude_msg(float roll, float pitch, float yaw) {
    if (!uart_tx_ready)
    	return;

    mavlink_message_t msg;
    uint8_t buffer[MAVLINK_MAX_PACKET_LEN];

    mavlink_attitude_t attitude = {
        .time_boot_ms = HAL_GetTick(),
        .roll = roll,
        .pitch = pitch,
        .yaw = yaw,
        .rollspeed = 0,
        .pitchspeed = 0,
        .yawspeed = 0
    };

    mavlink_msg_attitude_encode(1, MAV_COMP_ID_AUTOPILOT1, &msg, &attitude);
    uint16_t len = mavlink_msg_to_send_buffer(buffer, &msg);

    uart_tx_ready = false;
    HAL_UART_Transmit_DMA(mavlink.huart, buffer, len);
}*/
void Send_Water_msg(float pressure_hpa, float temp_celsius) {
	mavlink_message_t msg;
	uint8_t buffer[MAVLINK_MAX_PACKET_LEN];
    mavlink_scaled_pressure_t msg_data;
    msg_data.time_boot_ms = HAL_GetTick();
    msg_data.press_abs = pressure_hpa;
    //msg_data.press_diff = diff_pressure_hpa;
    msg_data.temperature = (int16_t)(temp_celsius);
    mavlink_msg_scaled_pressure_encode(1, MAV_COMP_ID_AUTOPILOT1, &msg, &msg_data);
    uint16_t len = mavlink_msg_to_send_buffer(buffer, &msg);
    HAL_UART_Transmit_DMA(mavlink.huart, buffer, len);
}
void Send_tube_msg(float pressure_hpa, float temp_celsius)
{
	mavlink_message_t msg;
	uint8_t buffer[MAVLINK_MAX_PACKET_LEN];
    mavlink_scaled_pressure2_t msg_data;
    msg_data.time_boot_ms = HAL_GetTick();
    msg_data.press_abs = pressure_hpa;
    //msg_data.press_diff = diff_pressure_hpa;
    msg_data.temperature = (int16_t)(temp_celsius);
    mavlink_msg_scaled_pressure2_encode(1, MAV_COMP_ID_AUTOPILOT1, &msg, &msg_data);
    uint16_t len = mavlink_msg_to_send_buffer(buffer, &msg);
    HAL_UART_Transmit_DMA(mavlink.huart, buffer, len);
}
void Msg_processing(mavlink_message_t* msg)
{
	switch (msg->msgid)
    {
	case MAVLINK_MSG_ID_MANUAL_CONTROL:{
    	mavlink_manual_control_t cmd;
        mavlink_msg_manual_control_decode(msg, &cmd);
        Move_Update(cmd.x ,cmd.y, cmd.z, cmd.r);
        break;
    }
    case MAVLINK_MSG_ID_COMMAND_LONG:
    {
    	mavlink_command_long_t cmd;
        mavlink_msg_command_long_decode(msg, &cmd);
        if (cmd.command == MAV_CMD_DO_FLIGHTTERMINATION) //power
        {

        }
        else if (cmd.command == MAV_CMD_COMPONENT_ARM_DISARM)
        {
        	if(cmd.param1 == 1)
        		Motor_Enable(0xFF);
        	else
        		Motor_Disable(0xFF);
        }
        else if(cmd.command == MAV_CMD_DO_SET_SERVO)//light
        {

        }
        else if(cmd.command == MAV_CMD_DO_SET_PARAMETER)//PID
		{

		}
        break;
    }
    default:
    	break;
    }
}
/**/
