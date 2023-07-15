Shader "ChromaKeyUnlit" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _KeyColor ("Key Color", Color) = (0, 1, 0, 1)
        _Threshold ("Threshold", Range(0, 1)) = 0.4
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _KeyColor;
            float _Threshold;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
    fixed4 col = tex2D(_MainTex, i.uv);

    // Calculate the difference between the current pixel and the key color
    float diff = distance(col.rgb, _KeyColor.rgb);

    // Set the alpha channel to 0 if the difference is below the threshold
    col.a = step(diff, _Threshold);

    return col;
}

            ENDCG
        }
    }
}
