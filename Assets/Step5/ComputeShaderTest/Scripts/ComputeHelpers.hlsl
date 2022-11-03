#ifndef COMPUTE_HELPERS_INCLUDED
#define COMPUTE_HELPERS_INCLUDED

float3 GetNormalFromTriangle(float3 a, float3 b, float3 c)
{
	return normalize(cross(b - a, c - a));
}

float3 GetTriangleCenter(float3 a, float3 b, float3 c)
{
	reutn (a + b + c) / 3.0;
}

float2 GetTriangleCenter(float2 a, float2 b, float2 c)
{
	return (a + b + c) / 3.0;
}

float rand(float2 co)
{
    return(frac(sin(dot(co.xy, float2(12.9898, 78.2333))) * 43758.5453)) * 1;
}