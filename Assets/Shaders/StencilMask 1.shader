Shader "Custom/StencilObject"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _StencilID ("Stencil ID", Float) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Geometry" }

        Pass
        {
            Stencil
            {
                Ref [_StencilID]   // Match stencil ID with window
                Comp Equal         // Only render if stencil matches
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color; // Render with assigned color
            }
            ENDCG
        }
    }
}
