Shader "Code"
{
    Properties
    {
        _Testparticlealpha("Testparticlealpha", 2D) = "black" {}
        _TintColor("Tint Color", Color) = (0.1860982, 0.8983208, 0.9622642, 1)
        _Brightness("Brightness", Float) = 3.0
        _AlphaBoost("Alpha Boost", Float) = 1.5
        _AlphaCutoffLow("Alpha Cutoff Low", Range(0, 1)) = 0.04
        _AlphaCutoffHigh("Alpha Cutoff High", Range(0, 1)) = 0.20
        _ScrollSpeed("Scroll Speed", Vector) = (0.2, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 100
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "CodeTransparentUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_Testparticlealpha);
            SAMPLER(sampler_Testparticlealpha);

            CBUFFER_START(UnityPerMaterial)
                float4 _Testparticlealpha_ST;
                float4 _TintColor;
                float _Brightness;
                float _AlphaBoost;
                float _AlphaCutoffLow;
                float _AlphaCutoffHigh;
                float4 _ScrollSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _Testparticlealpha);
                output.color = input.color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv + (_Time.y * _ScrollSpeed.xy);
                float4 tex = SAMPLE_TEXTURE2D(_Testparticlealpha, sampler_Testparticlealpha, uv);

                // Use the brightness of the visible code as the mask.
                // This removes black texture backgrounds even when the PNG has no real alpha channel.
                float maskSource = max(max(tex.r, tex.g), tex.b);
                float alpha = smoothstep(_AlphaCutoffLow, _AlphaCutoffHigh, maskSource);
                alpha = saturate(alpha * _AlphaBoost * _TintColor.a * input.color.a);

                float3 rgb = _TintColor.rgb * maskSource * _Brightness;
                rgb *= input.color.rgb;

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
