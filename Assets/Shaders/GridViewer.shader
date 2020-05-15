Shader "Unlit/GridViewer"
{
    Properties
    {
        _ScaleShift ("Lines per Unit & Shift", Vector) = (10, 10, 0, 0)
        _Color ("Colour", Color) = (1, 0, 0, 1)
        _Thickness ("Thickness", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float4 _ScaleShift;
            fixed4 _Color;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = 2.0f * (v.uv - 0.5f);
                o.worldPos = worldPos.xz * _ScaleShift.xy + _ScaleShift.zw;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 distanceFromLine = abs(frac(i.worldPos + 0.5f) - 0.5f);
                float2 speedPerPixel = fwidth(i.worldPos);

                float2 pixelsFromLine = abs(distanceFromLine / speedPerPixel);

                float opacity = 1.0f - saturate(min(pixelsFromLine.x, pixelsFromLine.y) - 0.5f * (_Thickness - 0.5f));

                float radiusSquared = dot(i.uv, i.uv);
                float falloff = max(1.0f - radiusSquared, 0.0f);


                fixed4 col = _Color;
                col.a *= opacity * falloff * falloff;

                return col;
            }
            ENDCG
        }
    }
}