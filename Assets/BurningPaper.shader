Shader "Custom/BurningPaper"
{
    // Stylized / cartoon version.
    // Flat colour bands, an ink outline, flame tongues that lick out past the paper's edge,
    // and stepped "hand-drawn" animation. Works with the same BurningPaper.cs script.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Paper Edge)]
        _OutlineColor ("Ink Outline Color", Color) = (0.12, 0.05, 0.05, 1)
        _OutlineWidth ("Ink Outline Width", Range(0, 0.05)) = 0.008
        _CharColor ("Scorch Color", Color) = (0.55, 0.33, 0.18, 1)

        [Header(Flames)]
        [HDR] _FlameOuter ("Flame Outer", Color) = (1.6, 0.25, 0.1, 1)
        [HDR] _FlameMid ("Flame Middle", Color) = (2.0, 0.8, 0.1, 1)
        [HDR] _FlameCore ("Flame Core", Color) = (2.2, 2.0, 0.7, 1)
        _FlameHeight ("Flame Height", Range(0, 0.2)) = 0.06
        _FlameScale ("Flame Tongue Size (higher = more, thinner tongues)", Float) = 14
        _FlameSpeed ("Flame Speed", Float) = 1.5
        _FlameOutline ("Flame Tip Outline (0 = off)", Range(0, 0.4)) = 0.12
        _Wobble ("Edge Wobble", Range(0, 0.05)) = 0.012

        [Header(Style)]
        _AnimFPS ("Animation FPS (0 = smooth)", Float) = 8
        _PixelSize ("Pixelate (pixels across, 0 = off)", Float) = 0
        _NoiseAspect ("Noise Aspect (paper width / height)", Float) = 1

        // Driven by BurningPaper.cs
        [HideInInspector] _BurnMask ("Burn Mask", 2D) = "white" {}
        [HideInInspector] _Progress ("Progress", Float) = 0
        [HideInInspector] _CharWidth ("Char Width", Float) = 0.06
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        ZWrite Off
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
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _BurnMask;
            fixed4 _Color;

            fixed4 _OutlineColor, _CharColor;
            half4 _FlameOuter, _FlameMid, _FlameCore;
            float _OutlineWidth, _CharWidth;
            float _FlameHeight, _FlameScale, _FlameSpeed, _FlameOutline, _Wobble;
            float _AnimFPS, _PixelSize, _NoiseAspect;
            float _Progress;

            float hash (float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise (float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Optional chunky pixel look.
                if (_PixelSize > 0)
                    uv = (floor(uv * _PixelSize) + 0.5) / _PixelSize;

                // Stepped time gives a hand-drawn, "on twos" animation feel.
                float t = _AnimFPS > 0 ? floor(_Time.y * _AnimFPS) / _AnimFPS : _Time.y;
                t *= _FlameSpeed;

                // Two layers of moving noise shape the flame tongues.
                float2 nuv = float2(uv.x * _NoiseAspect, uv.y) * _FlameScale;
                float n = valueNoise(nuv + float2(t * 1.3, t * 2.1)) * 0.65
                        + valueNoise(nuv * 2.3 - float2(t * 1.7, t * 0.9)) * 0.35;

                float burn = tex2D(_BurnMask, uv).r;
                float d = burn - _Progress;              // < 0 means the paper here is gone

                // Flames shrink away as the last bit of paper burns, so nothing lingers after game over.
                float H = _FlameHeight * saturate((1.001 - _Progress) * 15.0);

                float edge  = d + (n - 0.5) * _Wobble;   // wobbly paper edge
                float flame = d + H * n;                 // > 0 inside a flame tongue

                // ---- Paper side ----
                if (edge >= 0)
                {
                    half4 col = tex2D(_MainTex, uv) * i.color;
                    if (edge < _OutlineWidth)
                        col.rgb = _OutlineColor.rgb;                  // ink line
                    else if (edge < _OutlineWidth + _CharWidth)
                        col.rgb = col.rgb * _CharColor.rgb;           // flat scorched band
                    return col;
                }

                // ---- Flame side (beyond the paper edge) ----
                clip(flame);

                float h = max(H, 1e-4);
                half3 c;
                if (flame < h * _FlameOutline * 0.5) c = _OutlineColor.rgb;  // cartoon tip outline
                else if (flame < h * 0.25)           c = _FlameOuter.rgb;
                else if (flame < h * 0.5)            c = _FlameMid.rgb;
                else                                 c = _FlameCore.rgb;

                return half4(c, 1);
            }
            ENDCG
        }
    }
}
