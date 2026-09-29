Shader "Varginha/WorldDarkness"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float2 world : TEXCOORD0; };
            float _Darkness, _BeamOn, _HalfAngle, _HouseOnly;
            float4 _Origin, _Direction;
            float _Distances[81];
            v2f vert(appdata v)
            {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xy; return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float2 delta = i.world - _Origin.xy;
                float distance = length(delta);
                float angle = atan2(_Direction.x * delta.y - _Direction.y * delta.x, dot(_Direction.xy, delta));
                float sampleIndex = saturate((angle / max(.01, _HalfAngle) + 1) * .5) * 80;
                int index = min(79, (int)sampleIndex);
                float reach = lerp(_Distances[index], _Distances[index + 1], frac(sampleIndex));
                float edge = 1 - smoothstep(_HalfAngle * .78, _HalfAngle, abs(angle));
                float radial = 1 - smoothstep(max(0, reach - .32), reach, distance);
                float revealed = edge * radial * _BeamOn;
                float alpha = lerp(_Darkness, .035, revealed);
                // The house's power circuit does not black out the yard or the street.
                if (_HouseOnly > .5)
                    alpha *= (1 - smoothstep(8.5, 9, abs(i.world.x))) * (1 - smoothstep(6.4, 6.8, abs(i.world.y)));
                return fixed4(0, 0, .004, alpha);
            }
            ENDCG
        }
    }
}
