sampler2D inputSampler : register(S0);
sampler2D lutSampler   : register(S1);
float luminanceWeight : register(C0);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 src = tex2D(inputSampler, uv);

    // YCbCr (BT.709)
    float y = dot(src.rgb, float3(0.2126, 0.7152, 0.0722));

    float2 lutUV = float2(y, 0.5);

    float4 target = tex2D(lutSampler, lutUV);
    float3 color = target.rgb;
    float alpha = src.a * target.a;

    float cb = -0.1146 * color.r - 0.3854 * color.g + 0.5000 * color.b;
    float cr =  0.5000 * color.r - 0.4545 * color.g - 0.0455 * color.b;

    float3 preserved;
    preserved.r = y + 1.5748 * cr;
    preserved.g = y - 0.1873 * cb - 0.4681 * cr;
    preserved.b = y + 1.8556 * cb;
    preserved = saturate(preserved);

    float3 result = lerp(color, preserved, luminanceWeight);

    float3 premultiplied = result * alpha;
    return float4(premultiplied, alpha);
}
