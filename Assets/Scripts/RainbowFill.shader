Shader "Unlit/RainbowFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RainbowSpeed ("Rainbow Speed", Range(0, 10)) = 2.0 //色が変化するスピード
        _GradDensity ("Gradient Density", Range(0, 10)) = 1.5 //グラデーションの密度（細かさ）
        _Saturation ("Saturation", Range(0, 1)) = 0.7 //彩度
        _Brightness ("Brightness", Range(0, 1)) = 1.0 //明度
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        LOD 100
        Cull Off
        Lighting Off
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
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float _RainbowSpeed;
            float _GradDensity;
            float _Saturation;
            float _Brightness;

            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //画像の色と透明度を取得
                fixed4 texColor = tex2D(_MainTex, i.texcoord) * i.color;

                //UV座標（斜め方向）を計算に入れてグラデーションを作る
                float grad = (i.texcoord.x + i.texcoord.y) * _GradDensity;

                //グラデーションを動かす
                float hue = frac(_Time.y * _RainbowSpeed + grad);

                float3 rainbowColor = hsv2rgb(float3(hue, _Saturation, _Brightness));

                return fixed4(rainbowColor, texColor.a);
            }
            ENDCG
        }
    }
}
