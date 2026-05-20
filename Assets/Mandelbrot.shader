Shader "Custom/Mandelbrot"
{
    Properties
    {
        _Zoom ("Zoom", Float) = 1
        _Center ("Center", Vector) = (0,0,0,0)
        _Iterations ("Iterations", Int) = 100
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Zoom;
            float4 _Center;
            int _Iterations;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = (i.uv - 0.5) * 4.0 / _Zoom;
                float2 c = uv + _Center.xy;

                float2 z = float2(0, 0);
                int iter = 0;

                for (int j = 0; j < 1000; j++)
                {
                    if (j >= _Iterations) break;
                    if (dot(z, z) > 4.0) break;

                    z = float2(
                        z.x * z.x - z.y * z.y,
                        2.0 * z.x * z.y
                    ) + c;

                    iter++;
                }

                float t = iter / (float)_Iterations;
                return fixed4(t, t * t, t * 0.5, 1);
            }
            ENDCG
        }
    }
}