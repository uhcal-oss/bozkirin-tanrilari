Shader "Custom/SpriteBlur"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 0.1)) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // Auto-filled by Unity
            float _BlurSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Simple Box Blur (5 samples: Center + 4 neighbors)
                // Adjust BlurSize relative to aspect to prevent stretching if needed, 
                // but for simple sprite blur, raw UV offset is usually fine.
                
                fixed4 sum = tex2D(_MainTex, IN.texcoord) * 0.4;
                
                sum += tex2D(_MainTex, IN.texcoord + float2(_BlurSize, 0.0)) * 0.15;
                sum += tex2D(_MainTex, IN.texcoord + float2(-_BlurSize, 0.0)) * 0.15;
                sum += tex2D(_MainTex, IN.texcoord + float2(0.0, _BlurSize)) * 0.15;
                sum += tex2D(_MainTex, IN.texcoord + float2(0.0, -_BlurSize)) * 0.15;

                return sum * IN.color;
            }
        ENDCG
        }
    }
}
