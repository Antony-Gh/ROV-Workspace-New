/*
 * PID.h
 *
 *  Created on: Jul 19, 2025
 *      Author: TC
 */
/**/
#ifndef SRC_PID_H_
#define SRC_PID_H_

#include "stm32f4xx.h"
#include "cmsis_os.h"

typedef struct {
  float Kp, Ki, Kd;
  float integrator, prev_error, D_lpf;
  float D_alpha;             // 0..1
  float out_min, out_max;
  float anti_windup_beta;    // 0..1
} PID;
extern float SKP;
extern float SKI;
extern float SKD;
extern float SMAX;
extern float SMIN;
extern float SANTI;
extern float YKP;
extern float YKI;
extern float YKD;
extern float YMAX;
extern float YMIN;
extern float YANTI;
extern float PKP;
extern float PKI;
extern float PKD;
extern float PMAX;
extern float PMIN;
extern float PANTI;
extern float RKP;
extern float RKI;
extern float RKD;
extern float RMAX;
extern float RMIN;
extern float RANTI;



float clampf(float, float, float);
float pid_update(PID*, float, float);
void SetPID(PID*, float, float, float, float, float, float, float);

#endif /* SRC_PID_H_ */
