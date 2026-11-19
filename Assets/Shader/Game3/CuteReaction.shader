
Shader "Geidai/Game3/CuteReaction"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _TopColor ("Top Color", Color) = (1,0.96,0.65,1)
        _BottomColor ("Bottom Color", Color) = (1,0.7,0.85,1)
        _EdgeColor ("Edge Highlight", Color) = (1,1,1,1)

        _EdgeWidth ("Edge Width", Range(0,8)) = 2
        _EdgeStrength ("Edge Strength", Range(0,1)) = 0.7

        _Shimmer ("Shimmer", Range(0,1)) = 0.2
        _ShimmerSpeed ("Shimmer Speed", Range(0,10)) = 2

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
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _Color;
            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _EdgeColor;

            float _EdgeWidth;
            float _EdgeStrength;
            float _Shimmer;
            float _ShimmerSpeed;
            float4 _ClipRect;

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
                float4 localPos : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex;
                o.uv = v.uv;
                o.color = v.color * _Color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 元画像の透明度を使用
                float alpha = tex2D(_MainTex, i.uv).a;

                // 上下グラデーション
                float t = saturate(i.uv.y);

                float3 color = lerp(
                    _BottomColor.rgb,
                    _TopColor.rgb,
                    t
                );

                // 内側の輪郭を検出
                float2 d =
                    _MainTex_TexelSize.xy * _EdgeWidth;

                float neighbor = 1.0;

                neighbor = min(neighbor,
                    tex2D(_MainTex, i.uv + float2(d.x,0)).a);

                neighbor = min(neighbor,
                    tex2D(_MainTex, i.uv - float2(d.x,0)).a);

                neighbor = min(neighbor,
                    tex2D(_MainTex, i.uv + float2(0,d.y)).a);

                neighbor = min(neighbor,
                    tex2D(_MainTex, i.uv - float2(0,d.y)).a);

                float edge = saturate(alpha - neighbor);

                color = lerp(
                    color,
                    _EdgeColor.rgb,
                    edge * _EdgeStrength
                );

                // ゆっくり流れる光沢
                float wave = sin(
                    i.uv.y * 14.0 +
                    i.uv.x * 8.0 -
                    _Time.y * _ShimmerSpeed
                );

                float shimmer =
                    (wave * 0.5 + 0.5) * _Shimmer;

                color = lerp(
                    color,
                    _EdgeColor.rgb,
                    shimmer * 0.35
                );

                float finalAlpha = alpha *
                    lerp(_BottomColor.a, _TopColor.a, t) *
                    i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                    finalAlpha *= UnityGet2DClipping(
                        i.localPos.xy, _ClipRect
                    );
                #endif

                return fixed4(
                    color * i.color.rgb,
                    finalAlpha
                );
            }

            ENDCG
        }
    }
}
