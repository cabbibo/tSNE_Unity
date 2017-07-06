
Shader "Custom/wireframe" {
	
	Properties{


	}
	
	SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

		//Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
		LOD 200

		Pass{
			Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

											CULL OFF
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0








		struct VertexIn
		{
			float4 position  : POSITION;
			float3 normal    : NORMAL;
			float4 texcoord  : TEXCOORD0;
			float4 tangent   : TANGENT;
		};

		struct VertexOut {
			float4 pos    : POSITION;
			float3 normal : NORMAL;
			float4 uv     : TEXCOORD0;

		};




		VertexOut vert(VertexIn v) {

			VertexOut o;

			o.normal = v.normal;

			o.uv = v.texcoord;

			// Getting the position for actual position
			o.pos = UnityObjectToClipPos(v.position);



			return o;

		}


		// Fragment Shader
		fixed4 frag(VertexOut i) : COLOR{

			float3 col = float3(1,1,1);
			float s = .05;
			if (i.uv.x > s && i.uv.x < 1-s &&i.uv.y > s && i.uv.y < 1-s) { discard; }

			fixed4 color;
			color = fixed4(col , 1);
			return color;


		}


		ENDCG


}
	}
	FallBack "Diffuse"
}
