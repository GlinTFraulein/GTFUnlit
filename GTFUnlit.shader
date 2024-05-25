Shader "GlinTFraulein/GTFUnlit"
{
    Properties
    {
        _BlendMode("Blend Mode", int) = 0
        _MainTex("Texture", 2D) = "white" {}
        [HDR] _Color("Color", Color) = (1, 1, 1, 1)
        _Cutoff ("Alpha Cutout", Range(0.0, 1.0)) = 0.5

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Culling Mode", int) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", int) = 4
        [Enum(Off, 0, On, 1)] _ZWrite("ZWrite", int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcFactor("SrcFactor", int) = 3
        [Enum(UnityEngine.Rendering.BlendMode)] _DstFactor("DstFactor", int) = 10
        //https://qiita.com/gam0022/items/c26a73e244dbbde9b034 そのうちやる(BlendMadeのProperty化)
    }

    SubShader
    {
        Cull [_CullMode]
        ZTest [_ZTest]
        ZWrite [_ZWrite]
        Blend [_SrcFactor] [_DstFactor]

        Tags
        {
            "Queue"      = "Opaque"
            "RenderType" = "Opaque"
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _ALPHATEST_ON
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float     _Cutoff;
            fixed4    _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv) * _Color;
                
                #ifdef _ALPHATEST_ON
                clip(color.a - _Cutoff);
                #endif

                return color;
            }
            
            ENDCG
        }
    }

    FallBack "Unlit/Texture"
    CustomEditor "GTF_Shaders.GTFUnlitGUI"
}
