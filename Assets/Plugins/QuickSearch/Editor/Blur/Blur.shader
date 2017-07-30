Shader "Hidden/QuickSearch-Blur" {
  Properties {
    _MainTex ("", 2D) = "white" {}
    _BlurSize ("", Range(0.0, 1.0)) = 1.0
    _Tint ("", Color) = (0.0, 0.0, 0.0, 0.0)
    _Tinting ("", Range(0.0, 1.0)) = 0.64
  }

  SubShader {

    Pass { // Pass 0 - Horizontal
      ZTest Always Cull Off ZWrite Off
      Fog { Mode off }
      Blend Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma fragmentoption ARB_precision_hint_fastest

      #include "UnityCG.cginc"

      struct v2f {
        half4 pos : POSITION;
        half2 uv : TEXCOORD0;
      };

      uniform sampler2D _MainTex;
      uniform half4 _MainTex_TexelSize;
      uniform half _BlurSize;

      v2f vert(appdata_img v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
      }

      half4 frag(v2f i) : COLOR {
        half4 color = 0.16 * tex2D(_MainTex, i.uv);
        color += 0.15 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(1.0 * _BlurSize, 0.0));
        color += 0.15 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(1.0 * _BlurSize, 0.0));
        color += 0.12 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(2.0 * _BlurSize, 0.0));
        color += 0.12 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(2.0 * _BlurSize, 0.0));
        color += 0.09 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(3.0 * _BlurSize, 0.0));
        color += 0.09 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(3.0 * _BlurSize, 0.0));
        color += 0.06 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(4.0 * _BlurSize, 0.0));
        color += 0.06 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(4.0 * _BlurSize, 0.0));
        return color;
      }
      ENDCG
    }

    Pass { // Pass 1 - Vertical
      ZTest Always Cull Off ZWrite Off
      Fog { Mode off }
      Blend Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma fragmentoption ARB_precision_hint_fastest

      #include "UnityCG.cginc"

      struct v2f {
        half4 pos : POSITION;
        half2 uv : TEXCOORD0;
      };

      uniform sampler2D _MainTex;
      uniform half4 _MainTex_TexelSize;
      uniform half _BlurSize;

      v2f vert(appdata_img v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
      }

      half4 frag(v2f i) : COLOR {
        half4 color = 0.16 * tex2D(_MainTex, i.uv);
        color += 0.15 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0.0, 1.0 * _BlurSize));
        color += 0.15 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(0.0, 1.0 * _BlurSize));
        color += 0.12 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0.0, 2.0 * _BlurSize));
        color += 0.12 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(0.0, 2.0 * _BlurSize));
        color += 0.09 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0.0, 3.0 * _BlurSize));
        color += 0.09 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(0.0, 3.0 * _BlurSize));
        color += 0.06 * tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * half2(0.0, 4.0 * _BlurSize));
        color += 0.06 * tex2D(_MainTex, i.uv - _MainTex_TexelSize.xy * half2(0.0, 4.0 * _BlurSize));
        return color;
      }
      ENDCG
    }

    Pass { // Pass 2 - Tint
      ZTest Always Cull Off ZWrite Off
      Fog { Mode off }
      Blend Off

      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      #pragma fragmentoption ARB_precision_hint_fastest

      #include "UnityCG.cginc"

      struct v2f {
        half4 pos : POSITION;
        half2 uv : TEXCOORD0;
      };

      uniform sampler2D _MainTex;
      uniform half4 _Tint;
      uniform half _Tinting;

      v2f vert(appdata_img v) {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
      }

      half4 frag(v2f i) : COLOR {
        return lerp(tex2D(_MainTex, i.uv), _Tint, _Tinting);
      }
      ENDCG
    }
  }

  Fallback off
}