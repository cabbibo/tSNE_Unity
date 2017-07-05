Shader "Custom/LargePointShader" {

		Properties{
			_Texture("Texture" , 2D) = "white" {}
		}




			SubShader{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend SrcAlpha One
			AlphaTest Greater .01
			Cull off
			ColorMask RGB
			Cull Off Lighting Off ZWrite Off Fog{ Color(0,0,0,0) }
			Pass{


			CGPROGRAM
#pragma target 5.0

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"


			uniform sampler2D _Texture;


		struct Vert {
			float id;
			float3 pos;
			float3 vel;
			float3 targetPos;
			float3 debug;
		};

		uniform float4x4 _LargeMatrix;
		uniform float4x4 _SmallMatrix;

		uniform float _ClosestID;

		StructuredBuffer<Vert> _vertBuffer;

		//A simple input struct for our pixel shader step containing a position.
		struct varyings {
			float4 pos 			: SV_POSITION;
			float2 uv  			: TEXCOORD1;
			float3 eye      : TEXCOORD5;
			float3 worldPos : TEXCOORD6;
			float3 debug    : TEXCOORD7;

		};


		varyings vert(uint id : SV_VertexID) {

			varyings o;

			int bID = id / 6;

			int tri = id % 6;

			Vert v = _vertBuffer[bID];

			float3 fPos = float3(0,0,0);//mul(float4(0,0,0,1),b.bindPose ).xyz;

			float3 undone = mul( _SmallMatrix , float4(v.pos, 1)).xyz;
			//undone = v.pos;
			fPos = mul( _LargeMatrix , float4(undone,1) ).xyz;

			float3 up = UNITY_MATRIX_IT_MV[0].xyz;
			float3 ri = UNITY_MATRIX_IT_MV[1].xyz;

			o.debug = float3(.3,0,0);
			float size = .003;
			size = .13 * (max(v.debug.x - .5,0) + .1);
			if (v.id == _ClosestID) {
				o.debug = float3(1,1,0);

			}

			o.debug.z = v.debug.x;
			o.debug.y = v.debug.y;

			float2 fUV;
			if (tri == 0 || tri == 5) { fPos -= ri * size; fUV = float2(0,0); }
			if (tri == 1 || tri == 4) { fPos += ri * size; fUV = float2(1,1); }
			if (tri == 2) { fPos += up * size; fUV = float2(0,1); }
			if (tri == 3) { fPos -= up * size; fUV = float2(1,0); }

			o.pos = mul(UNITY_MATRIX_VP, float4(fPos,1.0f));
			o.worldPos = fPos;
			o.eye = _WorldSpaceCameraPos - o.worldPos;

			o.uv = fUV;




			return o;


		}
		//Pixel function returns a solid color for each point.
		float4 frag(varyings v) : COLOR{

			float3 tCol = tex2D(_Texture, float2(v.uv)).xyz;
			float3 col = tCol * v.debug;//float3(v.uv.x , v.uv.y , 0);//v.debug;

			return float4(col , .5);


		}

			ENDCG

		}
		}

			Fallback Off

	}