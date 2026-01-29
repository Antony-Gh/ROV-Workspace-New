#include "mpu6050.h"

#define WHO_AM_I_REG     0x75
#define PWR_MGMT_1       0x6B
#define ACCEL_XOUT_H     0x3B

static int16_t AcX, AcY, AcZ, GyX, GyY,GyZ;
static float ax, ay, az, gx, gy, gz;
static float roll_filtered = 0.0f;
static float pitch_filtered = 0.0f;
static float yaw_filtered = 0.0f;
static const float alpha = 0.96f;

bool MPU6050_ReadWhoAmI(uint8_t *whoami)
{
    return HAL_I2C_Mem_Read(&hi2c1, MPU6050_ADDR, WHO_AM_I_REG, I2C_MEMADD_SIZE_8BIT, whoami, 1, HAL_MAX_DELAY) == HAL_OK;
}

bool MPU6050_Init(void)
{
    uint8_t check;
    if (!MPU6050_ReadWhoAmI(&check) || check != 0x68) {
        return false;
    }

    uint8_t data[2] = { PWR_MGMT_1, 0x00 };
    if (HAL_I2C_Master_Transmit(&hi2c1, MPU6050_ADDR, data, 2, HAL_MAX_DELAY) != HAL_OK)
        return false;

    HAL_Delay(100);
    return true;
}

IMUData MPU6050_Update(float dt)
{
	IMUData imudata = {0};
    uint8_t rawData[14];
    if (HAL_I2C_Mem_Read(&hi2c1, MPU6050_ADDR, ACCEL_XOUT_H, I2C_MEMADD_SIZE_8BIT, rawData, 14, HAL_MAX_DELAY) != HAL_OK)
        return imudata;

    AcX = (int16_t)(rawData[0] << 8 | rawData[1]);
    AcY = (int16_t)(rawData[2] << 8 | rawData[3]);
    AcZ = (int16_t)(rawData[4] << 8 | rawData[5]);
    GyX = (int16_t)(rawData[8] << 8 | rawData[9]);
    GyY = (int16_t)(rawData[10] << 8 | rawData[11]);
    GyZ = (int16_t)(rawData[12] << 8) | rawData[13];

    ax = AcX / 16384.0f;
    ay = AcY / 16384.0f;
    az = AcZ / 16384.0f;
    gx = GyX / 131.0f;
    gy = GyY / 131.0f;
    gz = GyZ / 131.0f;

    float roll_accel = atan2f(-ay, sqrtf(ax * ax + az * az)) * 180.0f / M_PI;
    roll_filtered = alpha * (roll_filtered + gx * dt) + (1.0f - alpha) * roll_accel;
    imudata.roll = roundf(roll_filtered*100.0f)/100.0f;

    float pitch_accel = atan2f(-ax, sqrtf(ay * ay + az * az)) * 180.0f / M_PI;
    pitch_filtered = alpha * (pitch_filtered + gy * dt) + (1.0f - alpha) * pitch_accel;
    imudata.pitch = roundf(pitch_filtered*100.0f)/100.0f;

    yaw_filtered += gz * dt;
    imudata.yaw = roundf(yaw_filtered * 100.0f) / 100.0f;

    return imudata;
}
/**/
