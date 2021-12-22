#ifndef __SMOOTHING_KERNELS_INC__
#define __SMOOTHING_KERNELS_INC__

//smoothing "kernels"
float W_gaussian(float3 r_vec, float h)
{
	float r_squared = dot(r_vec, r_vec);
	return 1 / (pow(h , 2) * PI) * exp(- r_squared / pow(h , 2));
}

float3 W_gaussian_gradient(float3 r_vec, float h)
{
	return -2.0f * W_gaussian(r_vec, h) * (r_vec) / pow(h , 2);
}

float W_viscosity_laplacian(float3 r_vec,float h)
{
	float r = length(r_vec);
	float result = 0.0f;
	if(r > h)
	{
		result = 0.0f;
	}
	else
		result = 45/(PI * pow(h, 6)) * (h-r);
	return result;
}

float W_poly(float3 r_vec, float h)
{
	float r_squared = dot(r_vec,r_vec);
	float result;
	if(r_squared > pow(h, 2))
	{
		result = 0.0f;
	}
	else
	{
		result = 315/(64*PI *pow(h,9))*pow(pow(h,2)-r_squared, 3);
	}
	return result;

}

float3 W_poly_gradient(float3 r_vec, float h)
{
	float r_squared = dot(r_vec, r_vec);
	float3 result;
	if(r_squared > pow(h, 2) )
	{
		result = zero_vec;
	}
	else
	{
		result = (315*3)/(64*PI*pow(h,9))*pow(pow(h,2)-r_squared,2)*(-2)*r_vec;
	}
	return result;
}


float W_poly_laplacian(float3 r_vec, float h)
{
	float r_squared = dot(r_vec, r_vec);

	float result;
	if(r_squared > pow(h, 2) )
	{
		result = 0.0f;
	}
	else
	{
		result = (-6*315*3)/(64*PI*pow(h,9))*pow(pow(h,2)-r_squared,2) + r_squared*(4*6*315)/(64*PI*pow(h,9))*(pow(h,2)-r_squared);
	}
	return result;
}

float3 W_spiky_gradient(float3 r_vec, float h)
{
	float r = length(r_vec);
	float3 result;
	if( r < 0.01f )
	{
		result = zero_vec;
	}

	else if(r > h)
	{
		result = zero_vec;
	}
	else
	{
		result = -15/(PI*pow(h,6))*3*pow((h-r),2)*normalize(r_vec);
	}
	return result;
}


#endif // __SMOOTHING_KERNELS_INC__