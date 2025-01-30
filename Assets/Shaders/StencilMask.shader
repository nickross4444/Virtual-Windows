Shader "Custom/StencilMask"
{  Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _StencilID ("Stencil ID", Float) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }

        Pass
        {
            Stencil
            {
                Ref [_StencilID]   // Unique stencil value per layer
                Comp Always
                Pass Replace
            }
            
            ColorMask 0 // Don't render any color, just set stencil
        }
    }
}
