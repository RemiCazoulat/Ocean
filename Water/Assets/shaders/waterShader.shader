Shader "Unlit/waterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members tangent,binormal)
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct wave
            {
                float amplitude;
                float frequency;
                float phase;
                float3 direction;
                int type;
            };
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 normal : NORMAL0;
            };

            StructuredBuffer<wave> waves;
            int wave_number;
            float time;
            int wind_speed;

            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert ( appdata v)
            {
                v2f o;
                float y = v.vertex.y;
                float dx = 0;
                float dz = 0;
                //float slope_x = 0;
                //float slope_z = 0;
                float dxOld = 0;
                float dzOld = 0;
                for (int i = 0; i < wave_number; i ++) {
                    float dir_dot_xz = dot(v.vertex, waves[i].direction);
                    if(i != 0)
                    {
                        dir_dot_xz += dxOld;
                        
                    }
                    const float dir_x = waves[i].direction.x;
                    const float dir_z = waves[i].direction.z;
                    const float amplitude = waves[i].amplitude * (0.5f + (float)wind_speed  / 200);
                    const float frequency =  waves[i].frequency;
                    const float phase = waves[i].phase;
                    y += amplitude * exp(sin(dir_dot_xz * frequency + _Time.y * phase) - 1);
                    dx += frequency * amplitude * dir_x *  exp(sin(dir_dot_xz * frequency + _Time.y * phase) - 1) * cos(dir_dot_xz * frequency + _Time.y * phase);
                    dz += frequency * amplitude * dir_z *  exp(sin(dir_dot_xz * frequency + _Time.y * phase) - 1) * cos(dir_dot_xz * frequency + _Time.y * phase);
                    dxOld = dx;
                    dzOld = dz;
                    /*
                    const float x_plus = amplitude * sin(dir_x * (v.vertex.x + 1) * frequency + _Time.y * phase);
                    const float x_minus = amplitude * sin(dir_x * (v.vertex.x - 1) * frequency + _Time.y * phase);
                    slope_x = (x_plus - x_minus) / ((v.vertex.x + 1) - (v.vertex.x - 1));
                    const float z_plus = amplitude * sin(dir_z * (v.vertex.z + 1) * frequency + _Time.y * phase);
                    const float z_minus = amplitude * sin(dir_z * (v.vertex.z - 1) * frequency + _Time.y * phase);
                    slope_z = (z_plus - z_minus) / ((v.vertex.z + 1) - (v.vertex.z - 1));
                    */
                }
                v.vertex.y = y;
                float3 tangent = float3(1.0, dx, 0.0);
                float3 binormal = float3(0.0, dz, 1.0);
                //tangent = normalize(tangent);
                //binormal = normalize(binormal);
                const float3 cross_product = float3(binormal.y * tangent.z - binormal.z * tangent.y,
                                                    binormal.z * tangent.x - binormal.x * tangent.z,
                                                    binormal.x * tangent.y - binormal.y * tangent.x);                
                o.normal =  float4(cross_product.x, cross_product.y, cross_product.z, 1.0);
                o.normal = normalize(o.normal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float angle = 0;
                float3 lightDir = _WorldSpaceLightPos0;
                //lightDir.x * sin(radians(angle));
                //lightDir.y * cos(radians(angle));
                lightDir.y += angle;
                
                const float cos_angle =  dot(i.normal, normalize(lightDir));
                
                //cos_angle = max(0.0, cos_angle);
                //cos_angle = min(1.0, cos_angle);
                float r = cos_angle;
                float g = cos_angle;
                float b = cos_angle;
                float a = 0.5;
                fixed4 gaussian_diffuse = fixed4(r, g, b, a);
                const fixed4 sea_blue = fixed4(0, 0.1, 0.2, 1.0);
                gaussian_diffuse *= sea_blue;
                fixed4 color = gaussian_diffuse;
                return color;
            }
            ENDCG
        }
    }
}
