Shader "Custom/EdgeBurn2D"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Burn ("Burn Amount", Range(0,1)) = 0
        _Circular ("Shape", Range(0,1)) = 0

        _NoiseScale ("Noise Scale", Float) = 5
        _NoiseStrength ("Noise Strength", Range(0,0.6)) = 0.2

        _FireWidth ("Fire Band Width", Range(0.01,0.3)) = 0.08
        _CharWidth ("Char Band Width", Range(0.01,0.4)) = 0.12
        _Intensity ("Fire Brightness", Range(1,6)) = 2.5
        _Flicker ("Flicker Speed", Range(0,10)) = 4

        // Fire gradient, hottest (at the burning front) to coolest (behind it)
        _Col0 ("Fire 1 - Hottest", Color) = (0.08, 0.04, 0.03, 1)
        _Col1 ("Fire 2", Color) = (0.08, 0.04, 0.03, 1)
        _Col2 ("Fire 3", Color) = (0.7, 0.1, 0.02, 1)
        _Col3 ("Fire 4", Color) = (1, 0.45, 0.05, 1)
        _Col4 ("Fire 5 - Coolest", Color) =  (1, 0.85, 0.2, 1)
        _CharColor ("Char Colour", Color) = (1, 0.97, 0.7, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float _Burn;
            float _Circular;
            float _NoiseScale;
            float _NoiseStrength;
            float _FireWidth;
            float _CharWidth;
            float _Intensity;
            float _Flicker;
            float4 _Col0;
            float4 _Col1;
            float4 _Col2;
            float4 _Col3;
            float4 _Col4;
            float4 _CharColor;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;
            float2 uv         : TEXCOORD0;
            float4 color      : COLOR;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv         : TEXCOORD0;
            float4 color      : COLOR;
        };

        Varyings Vert(Attributes i)
        {
            Varyings o;
            o.positionCS = TransformObjectToHClip(i.positionOS);
            o.uv = TRANSFORM_TEX(i.uv, _MainTex);
            o.color = i.color * _Color;
            return o;
        }

        // ---- simple procedural noise ----
        float Hash(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        float ValueNoise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float a = Hash(i);
            float b = Hash(i + float2(1, 0));
            float c = Hash(i + float2(0, 1));
            float d = Hash(i + float2(1, 1));
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }

        float Fbm(float2 p)
        {
            return 0.65 * ValueNoise(p) + 0.35 * ValueNoise(p * 2.1 + 7.3);
        }

        // ---- fire colour ramp: t = 1 is hottest, t = 0 is coolest ----
        float3 FireRamp(float t)
        {
            float x = saturate(t) * 4.0;
            float3 c = lerp(_Col4.rgb, _Col3.rgb, saturate(x));
            c = lerp(c, _Col2.rgb, saturate(x - 1.0));
            c = lerp(c, _Col1.rgb, saturate(x - 2.0));
            c = lerp(c, _Col0.rgb, saturate(x - 3.0));
            return c;
        }

        half4 Frag(Varyings i) : SV_Target
        {
            half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;

            // 1) Edge picture: 0 in the middle, 1 at the border
            float2 centred = i.uv - 0.5;
            float squareEdge = 2.0 * max(abs(centred.x), abs(centred.y));
            float circleEdge = 2.0 * length(centred);
            float edge = saturate(lerp(squareEdge, circleEdge, _Circular));

            // 2) Add noise so the burn line is ragged
            float noise = Fbm(i.uv * _NoiseScale);
            float v = edge * (1.0 - _NoiseStrength) + noise * _NoiseStrength;

            // 3) Burn front moves from the edges (high v) inward (low v)
            float start = 1.0 + _FireWidth + _CharWidth + 0.02;
            float threshold = lerp(start, -0.01, _Burn);
            float d = threshold - v;      // distance behind the burn front

            clip(d);                      // burnt away -> invisible

            // 4) Char band (dark, fades into the normal sprite)
            float charAmt = 1.0 - saturate((d - _FireWidth) / _CharWidth);
            float3 col = lerp(sprite.rgb, _CharColor.rgb, charAmt);

            // 5) Fire band with colour gradient
            float t = saturate(d / _FireWidth);
            float flick = ValueNoise(i.uv * _NoiseScale * 4.0 + _Time.y * _Flicker);
            t = saturate(t + (flick - 0.5) * 0.35);
            float3 fire = FireRamp(t) * _Intensity;
            float fireAmt = 1.0 - smoothstep(_FireWidth * 0.6, _FireWidth, d);
            col = lerp(col, fire, fireAmt);

            return half4(col, sprite.a);
        }
        ENDHLSL

        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
