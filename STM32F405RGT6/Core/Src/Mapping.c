/**/
#include "Maping.h"

float r[N_T][3] = {
		{ 0.30f,  0.20f,  0.00f},
        { 0.30f, -0.20f,  0.00f},
        {-0.30f,  -0.20f,  0.00f},
        {-0.30f, 0.20f,  0.00f},
        { 0.30f,  0.00f,  0.00f},
        {-0.30f,  0.00f,  0.00f},
        { 0.00f, -0.20f,  0.00f}
};
float d[N_T][3] = {
        {-0.707, -0.707, 0},
        {-0.707,0.707, 0},
        { 0.707,0.707, 0},
        { 0.707, -0.707, 0},
        { 0.0, 0.0, 1},
        { 0.0, 0.0, 1},
        { 0.0, 0.0,-1}
};
float dT4[N_T][3] = {
        {0, 0, 0},
        {0, 0, 0},
        {0, 0, 0},
        {0, 0, 0},
        {0, 0, 1},
        {0, 0, 0},
        {0, 0, 0}
};
float dT5[N_T][3] = {
        {0, 0, 0},
        {0, 0, 0},
        {0, 0, 0},
        {0, 0, 0},
        {0, 0, 0},
        {0, 0, 1},
        {0, 0, 0}
};
ThrusterMap maps[N_T] = {
        {-20.30f, 25.5, 1188, 1812, 0, 0 },
        {-20.30f, 25.5, 1188, 1812, 0, 0 },
        {-20.30f, 25.5, 1188, 1812, 0, 0 },
        {-20.30f, 25.5, 1188, 1812, 0, 0 },
        {-20.30f, 25.5, 1188, 1812, 0, 0 },
        {-20.30f, 25.5, 1188, 1812, 0, 0 },
        {-20.30f, 25.5, 1188, 1812, 0, 0 }
};
float Fx_cmd = 0.0f;
float Fy_cmd = 0.0f;
float Fz_cmd = 0.0f;
float A[N_U][N_T] = {0};
float AT4[N_U][N_T] = {0};
float AT5[N_U][N_T] = {0};
float u[N_U] = {0};
float T[N_T] = {0};
int16_t PWM[N_T]= {1500};
int16_t PWM4[N_T] = {1500};
int16_t PWM5[N_T] = {1500};
float lambda = 0.1f;

void Build_Allocation_Matrix(float A[N_U][N_T], const float r[N_T][3], const float d[N_T][3])
{
    for (int i = 0; i < N_T; ++i)
    {
        A[0][i] = d[i][0];
        A[1][i] = d[i][1];
        A[2][i] = d[i][2];
        float rx = r[i][0], ry = r[i][1], rz = r[i][2];
        float dx = d[i][0], dy = d[i][1], dz = d[i][2];
        A[3][i] = ry * dz - rz * dy;
        A[4][i] = rz * dx - rx * dz;
        A[5][i] = rx * dy - ry * dx;
    }
}
int Cholesky_Solve(int n, const float *M_in, const float *b_in, float *x_out)
{
	float M[N_T * N_T];
	float b[N_T];
    for (int i = 0; i < n*n; ++i)
    	M[i] = M_in[i];
    for (int i = 0; i < n; ++i)
    	b[i] = b_in[i];
    for (int i = 0; i < n; ++i)
    {
        for (int j = 0; j <= i; ++j)
        {
        	float s = M[i*n + j];
            for (int k = 0; k < j; ++k)
            	s -= M[i*n + k] * M[j*n + k];
            if (i == j)
            {
            	if (s <= 0.0f)
            		return -1;
            	M[i*n + i] = sqrtf(s);
            }
            else
            {
            	M[i*n + j] = s / M[j*n + j];
            }
        }
    }
    float y[N_T];
    for (int i = 0; i < n; ++i)
    {
    	float s = b[i];
        for (int k = 0; k < i; ++k)
        	s -= M[i*n + k] * y[k];
        y[i] = s / M[i*n + i];
    }
    for (int i = n - 1; i >= 0; --i)
    {
    	float s = y[i];
        for (int k = i + 1; k < n; ++k)
        	s -= M[k*n + i] * x_out[k];
        x_out[i] = s / M[i*n + i];
    }
    return 0;
}
void Mat_ATA_ATu(const float A[N_U][N_T], const float u[N_U], float ATA[N_T * N_T], float ATu[N_T])
{
    for (int i = 0; i < N_T * N_T; ++i)
    	ATA[i] = 0.0f;
    for (int j = 0; j < N_T; ++j)
    	ATu[j] = 0.0f;
    for (int j = 0; j < N_T; ++j)
    {
        for (int k = 0; k < N_T; ++k)
        {
        	float s = 0.0f;
            for (int i = 0; i < N_U; ++i)
            	s += A[i][j] * A[i][k];
            ATA[j*N_T + k] = s;
        }
        float s2 = 0.0f;
        for (int i = 0; i < N_U; ++i)
        	s2 += A[i][j] * u[i];
        ATu[j] = s2;
    }
}
int Allocate_Thrusters(const float A[N_U][N_T], const float u_cmd[N_U], float lambda, float T_out[N_T])
{
	float ATA[N_T * N_T], ATu[N_T];
    Mat_ATA_ATu(A, u_cmd, ATA, ATu);
    float l2 = lambda * lambda;
    for (int i = 0; i < N_T; ++i)
    	ATA[i*N_T + i] += l2;
    return Cholesky_Solve(N_T, ATA, ATu, T_out);
}
int Allocate_Thrusters_Bounded(const float A_in[N_U][N_T], const float u_cmd[N_U], float lambda,
                                      const float T_min[N_T], const float T_max[N_T],
                                      int max_iter, float T_out[N_T])
{
    if (Allocate_Thrusters(A_in, u_cmd, lambda, T_out) != 0) return -1;
    uint8_t fixed[N_T] = {0};
    for (int iter = 0; iter < max_iter; ++iter)
    {
        int changed = 0;
        int n_fixed = 0;
        for (int j = 0; j < N_T; ++j)
        {
        	float Tj = T_out[j];
        	float low = T_min ? T_min[j] : -INFINITY;
        	float high = T_max ? T_max[j] : INFINITY;
            if (Tj < low)
            {
            	T_out[j] = low;
            	if (!fixed[j])
            	{
            		fixed[j] = 1;
            		changed = 1;
            	}
            }
            else if (Tj > high)
            {
            	T_out[j] = high;
            	if (!fixed[j])
            	{
            		fixed[j] = 1;
            		changed = 1;
            	}
            }
            if (fixed[j])
            	++n_fixed;
        }
        if (!changed) break;
        if (n_fixed >= N_T) break;
        float u_resid[N_U];
        for (int i = 0; i < N_U; ++i)
        {
        	float s = u_cmd[i];
            for (int j = 0; j < N_T; ++j)
            	if (fixed[j])
            		s -= A_in[i][j] * T_out[j];
            u_resid[i] = s;
        }
        int map_j[N_T]; int n_free = 0;
        for (int j = 0; j < N_T; ++j)
        	if (!fixed[j])
        		map_j[n_free++] = j;
        float ATA[N_T*N_T];
        float ATu[N_T];
        for (int a = 0; a < n_free*n_free; ++a)
        	ATA[a] = 0.0f;
        for (int a = 0; a < n_free; ++a)
        	ATu[a] = 0.0f;
        for (int aj = 0; aj < n_free; ++aj)
        {
            int j = map_j[aj];
            for (int ak = 0; ak < n_free; ++ak)
            {
                int k = map_j[ak];
                float s = 0.0f;
                for (int i = 0; i < N_U; ++i)
                	s += A_in[i][j] * A_in[i][k];
                ATA[aj*n_free + ak] = s;
            }
            float s2 = 0.0f;
            for (int i = 0; i < N_U; ++i)
            	s2 += A_in[i][j] * u_resid[i];
            ATu[aj] = s2;
        }
        float l2 = lambda*lambda;
        for (int a = 0; a < n_free; ++a)
        	ATA[a*n_free + a] += l2;
        float T_free[N_T];
        if (n_free > 0)
        {
            if (Cholesky_Solve(n_free, ATA, ATu, T_free) != 0)
            	return -1;
            for (int a = 0; a < n_free; ++a)
            	T_out[ map_j[a] ] = T_free[a];
        }
    }
    return 0;
}
void Thruster_Map_Init(ThrusterMap *m)
{
	m->k = (m->PWM_max - 1500) / (m->T_max - 0);
	m->kr = (m->PWM_min - 1500) / (m->T_min - 0);
	m->b = m->PWM_max - m->k * m->T_max;
	m->br = m->PWM_min - m->kr * m->T_min;
}
float Thrust_To_PWM(const ThrusterMap *m, float T)
{
    if (T < m->T_min)
    	T = m->T_min;
    if (T > m->T_max)
    	T = m->T_max;
    float pwm = m->k * T + m->b;
    if (pwm < m->PWM_min) pwm = m->PWM_min;
    if (pwm > m->PWM_max) pwm = m->PWM_max;
    return pwm;
}
void Map_All_To_PWM(const float T[N_T], const ThrusterMap maps[N_T], int16_t pwm_out[N_T])
{
    for (int i = 0; i < N_T; ++i)
    	pwm_out[i] = (int16_t)Thrust_To_PWM(&maps[i], T[i]);
}
int Allocate_And_Map(const float A[N_U][N_T], const float u_cmd[N_U], float lambda,
                                   const ThrusterMap maps[N_T], int max_iter,
								   float T_out[N_T], int16_t pwm_out[N_T])
{
	float Tmin[N_T], Tmax[N_T];
    for (int i = 0; i < N_T; ++i)
    {
    	Tmin[i] = maps[i].T_min;
    	Tmax[i] = maps[i].T_max;
    }
    int ok = Allocate_Thrusters_Bounded(A,u_cmd, lambda, Tmin, Tmax, max_iter, T_out);
    if (ok != 0)
    	return ok;
    Map_All_To_PWM(T_out, maps, pwm_out);
    return 0;
}
float Allocation_Residual_Norm(const float A[N_U][N_T], const float T[N_T], const float u_cmd[N_U])
{
	float r2 = 0.0f;
    for (int i = 0; i < N_U; ++i)
    {
    	float s = 0.0f;
    	for (int j = 0; j < N_T; ++j)
    		s += A[i][j] * T[j];
        float ri = s - u_cmd[i];
        r2 += ri * ri;
    }
    return sqrtf(r2);
}
void u_zero(float u[], int n )
{
	for(int i=0;i<n;i++) u[i] = 0;
}
