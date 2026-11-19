
Shader "Geidai/Game3/WizardHintEdge"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (0.65,0.9,1,1)

        _Playing ("Playing", Range(0,1)) = 0
        _LineWidth ("Solid Line Width", Range(1,40)) = 12
        _GlowRadius ("Idle Glow Radius", Range(2,160)) = 65
        _GlowOpacity ("Idle Glow Opacity", Range(0,1)) = 0.8
        _GlowSpeed ("Glow Speed", Range(0,10)) = 2

        [HideInInspector] _SourceZoom ("Source Zoom", Vector) = (1,1,0,0)
        [HideInInspector] _SourceUVRect ("Sprite UV Rect", Vector) = (0,0,1,1)
        [HideInInspector] _SourceRectSize ("Source Rect Size", Vector) = (500,700,0,0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _EdgeColor;

            float _Playing;
            float _LineWidth;
            float _GlowRadius;
            float _GlowOpacity;
            float _GlowSpeed;

            float4 _SourceZoom;
            float4 _SourceUVRect;
            float4 _SourceRectSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            // Sprite の範囲外は透明として扱う
            float SampleAlpha(float2 uv)
            {
                float inside =
                    step(_SourceUVRect.x, uv.x) *
                    step(_SourceUVRect.y, uv.y) *
                    step(uv.x, _SourceUVRect.z) *
                    step(uv.y, _SourceUVRect.w);

                float2 safeUV = clamp(
                    uv,
                    _SourceUVRect.xy,
                    _SourceUVRect.zw
                );

                return tex2D(_MainTex, safeUV).a * inside;
            }

            // 指定距離の8方向にある不透明領域を探す
            float SurroundingAlpha(float2 uv, float2 radius)
            {
                float2 diagonal = radius * 0.70710678;
                float m = 0;

                m = max(m, SampleAlpha(uv + float2(radius.x, 0)));
                m = max(m, SampleAlpha(uv - float2(radius.x, 0)));
                m = max(m, SampleAlpha(uv + float2(0, radius.y)));
                m = max(m, SampleAlpha(uv - float2(0, radius.y)));

                m = max(m, SampleAlpha(uv + diagonal));
                m = max(m, SampleAlpha(uv - diagonal));
                m = max(m, SampleAlpha(uv + float2(diagonal.x, -diagonal.y)));
                m = max(m, SampleAlpha(uv + float2(-diagonal.x, diagonal.y)));

                return m;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 拡大した描画領域内でも
                // 元の師匠画像の大きさを維持する
                float2 centerUV =
                    (_SourceUVRect.xy + _SourceUVRect.zw) * 0.5;

                float2 uv =
                    centerUV +
                    (i.uv - centerUV) * _SourceZoom.xy;

                float a = SampleAlpha(uv);

                // UI座標からSpriteのUV距離に変換
                float2 uvPerUnit =
                    (_SourceUVRect.zw - _SourceUVRect.xy) /
                    max(_SourceRectSize.xy, float2(1,1));

                // 内側には描画しない
                float outside = 1.0 - smoothstep(0.04, 0.45, a);

                // 再生中：太く明瞭な外側の実線
                float2 lineStep = uvPerUnit * _LineWidth;

                float lineAlpha = max(
                    SurroundingAlpha(uv, lineStep * 0.5),
                    SurroundingAlpha(uv, lineStep)
                );

                float solidEdge =
                    saturate(lineAlpha * 1.8) * outside;

                // 待機中：広い範囲の柔らかい光
                float2 glowStep = uvPerUnit * _GlowRadius;

                float nearGlow =
                    SurroundingAlpha(uv, glowStep * 0.35);

                float midGlow =
                    SurroundingAlpha(uv, glowStep * 0.7);

                float farGlow =
                    SurroundingAlpha(uv, glowStep);

                float glow = saturate(
                    nearGlow * 0.7 +
                    midGlow * 0.35 +
                    farGlow * 0.15
                ) * outside;

                float wave =
                    0.82 +
                    0.18 * sin(
                        _Time.y * _GlowSpeed +
                        i.uv.x * 12.0 +
                        i.uv.y * 8.0
                    );

                float idleEdge =
                    glow * _GlowOpacity * wave;

                // 再生状態で切り替え
                float finalAlpha = lerp(
                    idleEdge,
                    solidEdge,
                    saturate(_Playing)
                );

                return fixed4(
                    _EdgeColor.rgb * i.color.rgb,
                    saturate(
                        finalAlpha *
                        _EdgeColor.a *
                        i.color.a
                    )
                );
            }
            ENDCG
        }
    }
}
