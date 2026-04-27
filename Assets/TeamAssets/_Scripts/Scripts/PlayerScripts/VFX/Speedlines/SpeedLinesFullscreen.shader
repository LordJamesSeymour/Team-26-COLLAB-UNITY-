Shader "UI/SpeedLinesFullscreen"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("UI Tint", Color) = (1,1,1,1)

        _SpeedAmount("Speed Amount", Range(0,1)) = 0
        _LineDensity("Line Density", Float) = 24
        _Thickness("Thickness", Range(0.0001, 0.02)) = 0.0045
        _Brightness("Brightness", Range(0,6)) = 3.0
        _CenterFade("Center Fade", Range(0,1)) = 0.48
        _EdgeBoost("Edge Boost", Range(0,8)) = 1.2
        _Jitter("Jitter", Range(0,5)) = 0.06
        _Tint("Tint", Color) = (1,1,1,1)

        _FlowPhase("Flow Phase", Float) = 0

        _MinTrailLength("Min Trail Length", Range(0.02,1.0)) = 0.18
        _MaxTrailLength("Max Trail Length", Range(0.02,1.0)) = 0.58
        _TrailFeather("Trail Feather", Range(0.001,0.2)) = 0.02
        _LineSoftness("Line Softness", Range(0.0001,0.05)) = 0.0015

        _AngularJitter("Angular Jitter", Range(0,2)) = 1.4
        _SpawnJitter("Spawn Jitter", Range(0,0.4)) = 0.10
        _RadiusCompensation("Radius Compensation", Range(0,2)) = 1.0
        _EdgeThicknessBoost("Edge Thickness Boost", Range(0,2)) = 0.18
        _EdgeBrightnessBias("Edge Brightness Bias", Range(0.1,4)) = 1.8

        _SheetColumns("Sheet Columns", Float) = 6
        _SheetRows("Sheet Rows", Float) = 1
        _FrameCount("Frame Count", Float) = 6
        _TextureAnimSpeed("Texture Anim Speed", Float) = 5
        _FlipStreakU("Flip Streak U", Range(0,1)) = 0
        _FlipStreakV("Flip Streak V", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            float _SpeedAmount;
            float _LineDensity;
            float _Thickness;
            float _Brightness;
            float _CenterFade;
            float _EdgeBoost;
            float _Jitter;
            float4 _Tint;

            float _FlowPhase;

            float _MinTrailLength;
            float _MaxTrailLength;
            float _TrailFeather;
            float _LineSoftness;

            float _AngularJitter;
            float _SpawnJitter;
            float _RadiusCompensation;
            float _EdgeThicknessBoost;
            float _EdgeBrightnessBias;

            float _SheetColumns;
            float _SheetRows;
            float _FrameCount;
            float _TextureAnimSpeed;
            float _FlipStreakU;
            float _FlipStreakV;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float circularDistance(float a, float b)
            {
                return abs(frac(a - b + 0.5) - 0.5);
            }

            float signedCircularDelta(float a, float b)
            {
                return frac(a - b + 0.5) - 0.5;
            }

            float2 GetSheetUV(float2 streakUV, float frameIndex)
            {
                float cols = max(1.0, _SheetColumns);
                float rows = max(1.0, _SheetRows);
                float frameCount = max(1.0, _FrameCount);

                frameIndex = fmod(frameIndex, frameCount);
                float col = fmod(frameIndex, cols);
                float row = floor(frameIndex / cols);

                streakUV = saturate(streakUV);

                if (_FlipStreakU > 0.5)
                    streakUV.x = 1.0 - streakUV.x;

                if (_FlipStreakV > 0.5)
                    streakUV.y = 1.0 - streakUV.y;

                float2 cellSize = 1.0 / float2(cols, rows);
                return float2((col + streakUV.x) * cellSize.x, (row + streakUV.y) * cellSize.y);
            }

            float4 EvaluateStreak(float angle01, float distNorm, float slotIndex, float slotCount)
            {
                float densityFactor = lerp(0.2, 1.0, _SpeedAmount);
                float activeThreshold = lerp(0.997, 0.22, densityFactor);

                float slotSeed = slotIndex + 17.0;
                float localRate = 0.8 + hash11(slotSeed + 5.3) * 1.9;

                float cycleValue = _FlowPhase * localRate + hash11(slotSeed) * 9.37;
                float cycleIndex = floor(cycleValue);
                float cyclePos = frac(cycleValue);

                float rndA = hash21(float2(slotIndex, cycleIndex + 1.3));
                float rndB = hash21(float2(slotIndex, cycleIndex + 7.1));
                float rndC = hash21(float2(slotIndex, cycleIndex + 15.9));
                float rndD = hash21(float2(slotIndex, cycleIndex + 28.4));
                float rndE = hash21(float2(slotIndex, cycleIndex + 61.7));

                float activeMask = step(activeThreshold, rndA);
                if (activeMask < 0.5)
                    return float4(0,0,0,0);

                float slotCenter01 = (slotIndex + 0.5) / slotCount;
                float slotWidth = 1.0 / slotCount;

                float angleCenter01 = frac(slotCenter01 + (rndB - 0.5) * slotWidth * _AngularJitter);
                float signedLane = signedCircularDelta(angle01, angleCenter01);
                float laneDist = abs(signedLane);

                float spawnRadius = saturate(_CenterFade + rndC * _SpawnJitter);

                float head = lerp(spawnRadius, 1.08, cyclePos);
                float trailLength = lerp(_MinTrailLength, _MaxTrailLength, rndD);
                trailLength *= lerp(0.9, 1.25, _SpeedAmount);

                float tail = max(spawnRadius, head - trailLength);
                float feather = _TrailFeather * (0.85 + rndE * 0.35);

                float trailMask =
                    smoothstep(tail, tail + feather, distNorm) *
                    (1.0 - smoothstep(head - feather, head, distNorm));

                if (trailMask <= 0.0001)
                    return float4(0,0,0,0);

                float trailPos = saturate((distNorm - tail) / max(head - tail, 0.0001));

                float baseWidth = _Thickness * lerp(0.75, 1.35, rndE);
                float radiusComp = pow(max(distNorm, 0.18), _RadiusCompensation);
                float width = baseWidth / radiusComp;
                width *= lerp(1.0, 1.0 + _EdgeThicknessBoost, trailPos);

                float softness = _LineSoftness + (_Jitter * 0.0015);
                float lineMask = 1.0 - smoothstep(width, width + softness, laneDist);

                float brightnessBias = pow(trailPos, _EdgeBrightnessBias);
                float brightnessVar = lerp(0.55, 1.65, rndB);
                float edgeMask = lerp(0.72, 1.0, pow(distNorm, _EdgeBoost));

                float analyticIntensity = lineMask * trailMask;
                analyticIntensity *= lerp(0.18, 1.0, brightnessBias);
                analyticIntensity *= brightnessVar;
                analyticIntensity *= edgeMask;
                analyticIntensity *= activeMask;
                analyticIntensity *= _Brightness * _SpeedAmount;

                float localU = trailPos;
                float localV = saturate(0.5 + (signedLane / max(width + softness, 0.00001)) * 0.5);

                float frameFloat = frac(_FlowPhase * _TextureAnimSpeed * (0.7 + rndA * 0.6) + rndD * 11.13) * max(1.0, _FrameCount);
                float frameIndex = floor(frameFloat);

                float2 streakUV = float2(localU, localV);
                float4 streakTex = tex2D(_MainTex, GetSheetUV(streakUV, frameIndex));

                float texMask = max(streakTex.a, dot(streakTex.rgb, float3(0.3333, 0.3333, 0.3333)));

                float alpha = saturate(analyticIntensity * texMask);
                float3 color = streakTex.rgb * alpha;

                return float4(color, alpha);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = uv - 0.5;
                p.x *= aspect;

                float maxDist = length(float2(0.5 * aspect, 0.5));
                float distNorm = saturate(length(p) / maxDist);

                float angle = atan2(p.y, p.x);
                float angle01 = frac((angle / (2.0 * UNITY_PI)) + 0.5);

                float slotCount = max(10.0, _LineDensity);
                float baseSlot = floor(angle01 * slotCount);

                float4 accum = float4(0,0,0,0);

                [unroll]
                for (int n = -5; n <= 5; n++)
                {
                    float slot = baseSlot + n;
                    float wrappedSlot = slot - floor(slot / slotCount) * slotCount;
                    accum += EvaluateStreak(angle01, distNorm, wrappedSlot, slotCount);
                }

                accum = saturate(accum);
                accum.rgb *= _Tint.rgb * i.color.rgb;
                accum.a *= _Tint.a * i.color.a;

                return accum;
            }
            ENDCG
        }
    }
}