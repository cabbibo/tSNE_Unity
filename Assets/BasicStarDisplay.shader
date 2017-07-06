﻿Shader "Custom/BasicStarDisplay" {

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
		uniform float _Active;
		uniform float3 _SelectorPos;


		struct Vert {
			float id;
			float3 pos;
			float3 vel;
			float3 targetPos;
			float3 debug;
		};

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

			fPos = v.pos;//mul( b.transform, float4(fPos,1) ).xyz;

			float3 up = UNITY_MATRIX_IT_MV[0].xyz;
			float3 ri = UNITY_MATRIX_IT_MV[1].xyz;

			o.debug = float3(.3,0,0);
			float size = .003;
			size = .001;// *(max(v.debug.x - .5, 0) + .1);
			if (v.id == _ClosestID) {
				o.debug = float3(1,1,0);

			}

			o.debug.x = v.debug.x;
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
			float3 col = tCol * ((v.debug * _Active) + (float3(.4, .4, .4) * (1 - _Active)));//float3(v.uv.x , v.uv.y , 0);//v.debug;

			//return float4(col * (1 + _Active), .3 + 2 * _Active);;
			return float4(v.debug.xyz,.1);;


		}

			ENDCG

		}
		}

			Fallback Off

	}