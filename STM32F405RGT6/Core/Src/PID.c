/*
 * PID.c
 *
 *  Created on: Jul 19, 2025
 *      Author: TC
 */
#include "PID.h"

float clampf(float v, float lo, float hi){
	if(v<lo)
		return lo;
	if(v>hi)
		return hi;
	return v;
}
float pid_update(PID* p, float error, float dt){
	float P = p->Kp * error;
	p->integrator += p->Ki * error * dt;
	float deriv = (error - p->prev_error) / dt;
	p->D_lpf += p->D_alpha * (deriv - p->D_lpf);
	float D = p->Kd * p->D_lpf;
	float u = P + p->integrator + D;
	float u_sat = clampf(u, p->out_min, p->out_max);
	p->integrator += p->anti_windup_beta * (u_sat - u);
	p->prev_error = error;
	return u_sat;
}
void SetPID(PID* p, float kp, float ki, float kd, float d_alpha, float max, float min, float anti){
	p->Kp = kp;
	p->Ki = ki;
	p->Kd = kd;
	p->D_alpha = d_alpha;
	p->out_max = max;
	p->out_min = min;
	p->anti_windup_beta = anti;
}
/*
PID yawrate_pd = { .Kp=1.0f, .Ki=0.0f, .Kd=0.08f, .D_alpha=0.1f,
                   .out_min=-20.0f, .out_max=20.0f, .anti_windup_beta=0.4f }; // outputs ~Nm
PID heading_pi = { .Kp=1.0f, .Ki=0.2f, .Kd=0.0f,
                   .out_min=-0.6f, .out_max=0.6f, .anti_windup_beta=0.4f };   // outputs yaw_rate rad/s
PID depth_pi   = { .Kp=2.0f, .Ki=0.3f, .Kd=0.0f,
                   .out_min=-60.0f, .out_max=60.0f, .anti_windup_beta=0.5f }; // // outputs Fz N
*/
/*
Inner yaw-rate PD @ 200 Hz

Outer heading PI @ 50 Hz → commands yaw-rate

Depth PI @ 50 Hz → vertical force
*//**/
