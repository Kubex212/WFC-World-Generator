Shader "Unlit/MenuShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _Size ("Size", Float) = 0.1
        _Border ("Border", Float) = 0.1
        _DistortRadius ("Distortion Radius", Float) = 100
        _CursorPosition ("Cursor Position", Vector) = (0.5, 0.5, 0, 0)
        _Offset ("Texture offset", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
             
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 screen : TEXCOORD1;
            };

            float _Size;
            float _Border;
            float _DistortRadius;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _CursorPosition;
            float4 _Color;

            uint Hash(uint s)
            {
                s ^= 2747636419u;
                s *= 2654435769u;
                s ^= s >> 16;
                s *= 2654435769u;
                s ^= s >> 16;
                s *= 2654435769u;
                return s;
            }

            float Random(uint seed)
            {
                return float(Hash(seed)) / 4294967295.0; // 2^32-1
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screen = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 r = fixed2(_ScreenParams.x / _ScreenParams.y,1);

                fixed2 coord = (i.screen.xy - 0.5)*r;
                fixed2 gridCoord = fmod(fmod(coord, _Size)+_Size, _Size)/_Size;
                int2 gridXY = round((coord - gridCoord*_Size)/_Size);
                float2 dif = abs(i.screen.xy - _CursorPosition);
                float border = _Border - length(dif)/_DistortRadius;
                
                float speed = Random(gridXY.x * 7 + gridXY.y * 458697);
                float noiseVal1 = Random(gridXY.x*7 + gridXY.y * 458697 + (int)round(_Time.y*speed));
                float noiseVal2 = Random(gridXY.x*7 + gridXY.y * 458697 + (int)round(_Time.y*speed) + 1);
                float smoothRand = lerp(noiseVal1, noiseVal2, frac(_Time.y*speed+0.5));
                float colordif = 0;// (Random(gridXY.x * 432 + gridXY.y * 45897) - 0.5) / 20;
                fixed4 col = _Color * fixed4(smoothRand, smoothRand -colordif, smoothRand + colordif,1);// fixed4(0.6, 0.6, 0.6, 1);// fixed4(fixed3(1, 1, 1)* sqrt(1 - length(gridCoord - 0.5)), 1);

                float minimum = min(
                    min(
                        1-gridCoord.x,
                        gridCoord.x),
                    min(
                        1-gridCoord.y,
                        gridCoord.y)
                );
                float x = clamp(minimum / border, 0, 1);
                col = col* fixed4(x * x * x, x*x, x, 1);
                return col;
            }
            ENDCG
        }
    }
}
