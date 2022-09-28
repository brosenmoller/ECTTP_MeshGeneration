//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void ChooseCorrectColor_float(float height, float heightCutoff1, float heightCutoff2, 
                              float heightBlendCutoff1, float heightBlendCutoff2,
                              float4 color0, float4 color1, float4 color2,  
                              out float4 finalColor)
{
    if (height < heightCutoff1 - heightBlendCutoff1)
    {
        finalColor = color0;
        return;
    }
    else if (height < heightCutoff1 + heightBlendCutoff1)
    {
        float diff = abs(height - heightCutoff1 + heightBlendCutoff1);
        float mapped = clamp((1 / heightBlendCutoff1) * diff, 0, 1); // Map value to between 0 and 1 depending on blendCutoff
        finalColor = lerp(color0, color1, mapped);
        return;
    }
    else if (height < heightCutoff2 - heightBlendCutoff2)
    {
        finalColor = color1;
        return;
    }
    else if (height < heightCutoff2 + heightBlendCutoff2)
    {
        float diff = abs(height - heightCutoff2 + heightBlendCutoff2);
        float mapped = clamp((1 / heightBlendCutoff2) * diff, 0, 1); // Map value to between 0 and 1 depending on blendCutoff
        finalColor = lerp(color1, color2, mapped);
        return;
    }
    else 
    {
        finalColor = color2;
        return;
    }
}


void RoundedCornerLerp_float(float input, float blendFactor, float blendPower, out float lerp)
{
    lerp = 1 - pow(1 - pow(input, blendFactor), blendPower);
}

#endif //MYHLSLINCLUDE_INCLUDED