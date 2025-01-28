Shader  "Custom/InvisibleLighting"
{
    Properties
    {
        _EmissionColor ("Emission Color", Color) = (0.1, 0.8, 0.2, 1.0)  // Emission color for lighting (greenish)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline" }
LOD200

        Pass
        {
Name"ForwardLit"
            Tags
{"LightMode" = "UniversalForward"
}
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // Include URP lighting library
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
    float2 uv : TEXCOORD0;
};

Varyings vert(Attributes IN)
{
    Varyings OUT;
    OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
    OUT.uv = IN.uv;
    return OUT;
}

fixed4 _EmissionColor;

half4 frag(Varyings IN) : SV_Target
{
                // Return fully transparent color
    half4 col = half4(0, 0, 0, 0);

                // Apply emission color to contribute to lighting
    col.rgb = _EmissionColor.rgb;

                // Return transparent with emission
    return col;
}

            ENDHLSL
        }
    }
Fallback"Hidden/Universal Render Pipeline/FallbackError"
}
