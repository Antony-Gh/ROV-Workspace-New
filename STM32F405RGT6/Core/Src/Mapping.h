/**/
#ifndef SRC_MAPING_H_
#define SRC_MAPING_H_

#include <math.h>
#include <string.h>
#include <stdint.h>

#define N_U 6
#define N_T 7

typedef struct
{
	float T_min, T_max;
	float PWM_min, PWM_max;
	float k, b, kr, br;
}ThrusterMap;

extern float r[N_T][3];
extern float d[N_T][3];
extern float dT4[N_T][3];
extern float dT5[N_T][3];
extern ThrusterMap maps[N_T];
extern float Fx_cmd;
extern float Fy_cmd;
extern float Fz_cmd;
extern float A[N_U][N_T], AT4[N_U][N_T], AT5[N_U][N_T], u[N_U], T[N_T], lambda;
extern int16_t PWM[N_T], PWM4[N_T], PWM5[N_T];



void Build_Allocation_Matrix(float A[N_U][N_T], const float r[N_T][3], const float d[N_T][3]);
int Cholesky_Solve(int n, const float *M_in, const float *b_in, float *x_out);
void Mat_ATA_ATu(const float A[N_U][N_T], const float u[N_U], float ATA[N_T * N_T], float ATu[N_T]);
int Allocate_Thrusters(const float A[N_U][N_T], const float u_cmd[N_U], float lambda, float T_out[N_T]);
int Allocate_Thrusters_Bounded(const float A_in[N_U][N_T], const float u_cmd[N_U], float lambda,
                                      const float T_min[N_T], const float T_max[N_T],
                                      int max_iter, float T_out[N_T]);
void Thruster_Map_Init(ThrusterMap *m);
float Thrust_To_PWM(const ThrusterMap *m, float T);
void Map_All_To_PWM(const float T[N_T], const ThrusterMap maps[N_T], int16_t pwm_out[N_T]);
int Allocate_And_Map(const float A[N_U][N_T], const float u_cmd[N_U], float lambda,
                                   const ThrusterMap maps[N_T], int max_iter,
								   float T_out[N_T], int16_t pwm_out[N_T]);
float Allocation_Residual_Norm(const float A[N_U][N_T], const float T[N_T], const float u_cmd[N_U]);
void u_zero(float* , int);

#endif /* SRC_MAPING_H_ */
