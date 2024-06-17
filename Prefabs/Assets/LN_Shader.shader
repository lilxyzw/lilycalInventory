Shader "Hidden/LocalNightmodeShader"
{
    Properties
    {
        _Sonar ("Sonar", Int) = 0
        _ClampValue ("Clamp", Range(0,1)) = 1.0
        _Multiply ("Multiply", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags {"Queue" = "Overlay+1000"}

        Cull Off
        ZWrite Off
        ZTest Always
        HLSLINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        bool _Sonar;
        float _ClampValue;
        float _Multiply;

        struct appdata
        {
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vertBase(appdata i, bool cond)
        {
            v2f o;
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            if(unity_CameraProjection._m20 != 0.0 || unity_CameraProjection._m21 != 0.0 || cond) return o;

            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.positionCS = float4(i.uv*2.0-1.0,0.0,1.0);
            return o;
        }
        ENDHLSL

        Pass
        {
            BlendOp Min
            HLSLPROGRAM
            v2f vert(appdata i){ return vertBase(i, _ClampValue == 1); }
            float4 frag() : SV_Target { return float4(_ClampValue.rrr,1); }
            ENDHLSL
        }

        Pass
        {
            Blend DstColor Zero
            HLSLPROGRAM
            v2f vert(appdata i){ return vertBase(i, _Multiply == 1.0); }
            float4 frag() : SV_Target { return float4(_Multiply.rrr,1); }
            ENDHLSL
        }

        Pass
        {
            Blend One One
            HLSLPROGRAM

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            float4 OptimizedMul(float4x4 mat, float3 vec)
            {
                return mat._m00_m10_m20_m30 * vec.x + (mat._m01_m11_m21_m31 * vec.y + (mat._m02_m12_m22_m32 * vec.z + mat._m03_m13_m23_m33));
            }

            v2f vert(appdata i){ return vertBase(i, !_Sonar); }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 scnUV = i.positionCS.xy / _ScreenParams.xy;
                #if defined(UNITY_SINGLE_PASS_STEREO)
                    scnUV.x *= 0.5;
                #endif
                float depthRaw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, scnUV);
                float depth = LinearEyeDepth(depthRaw);
                if(depthRaw == 0 || !_Sonar) return 0;

                float4 positionCS = float4(
                    (scnUV * 2.0 - 1.0) * depth,
                    1,
                    depth
                );
                #if UNITY_UV_STARTS_AT_TOP
                    positionCS.y = -positionCS.y;
                #endif
                float3 positionVS = positionCS.xyw / UNITY_MATRIX_P._m00_m11_m32;
                float3 positionWS = OptimizedMul(UNITY_MATRIX_I_V, positionVS).xyz;

                float4 col = float4(saturate(1 - abs(frac(positionWS) - 0.5) * 8), 0);
                col.rgb = pow(frac(length(positionWS - mul(unity_ObjectToWorld, float4(0,0,0,1))) * 0.5 - _Time.x * 10), 10) * float3(0.2,0.6,1.0);

                return col;
            }
            ENDHLSL
        }
    }
}