Shader "Custom/TMP_OccludedCRT"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        
        [Header(Retro CRT State)]
        [HDR] _RetroColor ("Retro Color", Color) = (0.1, 0.9, 0.2, 1)
        _ScanlineDensity ("Scanline Density", Float) = 100.0
        _ScanlineSpeed ("Scroll Speed", Float) = -5.0
        _ScanlineAngle ("Scanline Angle", Range(-180.0, 180.0)) = 0.0
        _ScanlineThickness ("Scanline Thickness", Range(0.01, 0.99)) = 0.5
        
        [Header(Depth Settings)]
        _DepthBias ("Occlusion Bias", Range(-0.01, 0.01)) = 0.001 
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent-100"
            "RenderPipeline"="UniversalPipeline" 
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; 
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _RetroColor;
                float _ScanlineDensity;
                float _ScanlineSpeed;
                float _ScanlineAngle;
                float _ScanlineThickness;
                float _DepthBias;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float distance = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                float textAlpha = smoothstep(0.45, 0.55, distance) * input.color.a;

                clip(textAlpha - 0.01);

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                screenUV.x *= _ScreenParams.x / _ScreenParams.y; 

                float rad = _ScanlineAngle * (3.14159265359 / 180.0);
                float s = sin(rad);
                float c = cos(rad);
                float rotatedY = screenUV.x * s + screenUV.y * c;

                float scanline = frac((rotatedY * _ScanlineDensity) + (_Time.y * _ScanlineSpeed));
                
                clip(scanline - (1.0 - _ScanlineThickness));

                float2 depthUV = input.screenPos.xy / input.screenPos.w;
                float rawSceneDepth = SampleSceneDepth(depthUV);
                float objectDepth = input.positionCS.z; 

                #if UNITY_REVERSED_Z
                    bool isOccluded = rawSceneDepth > (objectDepth + _DepthBias);
                #else
                    bool isOccluded = rawSceneDepth < (objectDepth - _DepthBias);
                #endif

                if (isOccluded)
                {
                    return half4(_RetroColor.rgb, textAlpha * _RetroColor.a);
                }

                return half4(input.color.rgb, textAlpha);
            }
            ENDHLSL
        }
    }
}