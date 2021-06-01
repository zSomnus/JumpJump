Shader "Transparent/RimLighting" {
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _AlphaRange("Alpha Range",Range(0,1)) = 0
        _RimColor("Rim Color",Color) = (1,1,1,1)
    }
        SubShader
    {
        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IngoreProject" = "True"}
            LOD 200
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NormalMap;
            float _AlphaRange;
            float4 _RimColor;
            fixed4 _Color;

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normalDir : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f v) : COLOR
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - v.worldPos);
                float normalDotViewDir = saturate(dot(normalize(v.normalDir),viewDir));
                fixed3 diffuse = normalDotViewDir * _Color;
                return fixed4(diffuse + _RimColor ,(1 - normalDotViewDir) * (1 - _AlphaRange) + _AlphaRange);
            }
            ENDCG
        }
    }
        FallBack "Diffuse"
}