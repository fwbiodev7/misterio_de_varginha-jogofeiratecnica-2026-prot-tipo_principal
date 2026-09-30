Shader "Varginha/OriginalSceneryTone"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _LeafTint ("Leaf shade", Color) = (.68,.76,.55,1)
        _TrunkTint ("Other pixels shade", Color) = (1,1,1,1)
        _FoliageOnly ("Separate foliage", Float) = 1
        _Saturation ("Saturation", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            sampler2D _MainTex;
            fixed4 _Color, _LeafTint, _TrunkTint;
            fixed _FoliageOnly, _Saturation;
            v2f vert(appdata v)
            {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv;
                o.color = v.color * _Color; return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                // Separate foliage from wood; preserve every original pixel and alpha value.
                fixed leaf = step(c.r * .86, c.g) * step(c.b * 1.15, c.g) * step(.1, c.g);
                fixed luminance = dot(c.rgb, fixed3(.3,.59,.11));
                c.rgb = lerp(luminance.xxx, c.rgb, _Saturation);
                c.rgb *= lerp(_TrunkTint.rgb, _LeafTint.rgb, leaf * _FoliageOnly);
                c *= i.color; c.rgb *= c.a; return c;
            }
            ENDCG
        }
    }
}
