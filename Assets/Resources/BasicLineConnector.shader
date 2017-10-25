Shader "Custom/BasicLineConnector" {

	Properties {

    }
  SubShader{

  	


    Cull off
    Pass{


      CGPROGRAM
      #pragma target 5.0

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"





struct Vert{
	 float id;
   float3 pos;
   float3 vel;
   float3 targetPos;
   float3 debug;
};

uniform float _ClosestID;
uniform float3 _SelectorPosition;

      StructuredBuffer<Vert> _vertBuffer;

      //A simple input struct for our pixel shader step containing a position.
      struct varyings {
          float4 pos 			: SV_POSITION;
          float2 uv  			: TEXCOORD1;
          float3 eye      : TEXCOORD5;
          float3 worldPos : TEXCOORD6;
          float3 debug    : TEXCOORD7;

      };


      varyings vert (uint id : SV_VertexID){

        varyings o;

       	Vert v = _vertBuffer[_ClosestID];

       	float3 sPos = v.pos;
       	float3 ePos = _SelectorPosition;

       	float3 fPos = float3(0,0,0);//mul(float4(0,0,0,1),b.bindPose ).xyz;

       	fPos = v.pos;//mul( b.transform, float4(fPos,1) ).xyz;
       	if( id == 1){
       		fPos = _SelectorPosition;
       	}

				o.pos = mul (UNITY_MATRIX_VP, float4(fPos,1.0f));
				o.worldPos = fPos;
				o.eye = _WorldSpaceCameraPos - o.worldPos;
	
				o.uv = float2(float(id),0);
        



        return o;


      }
      //Pixel function returns a solid color for each point.
      float4 frag (varyings v) : COLOR {
      	float3 col = float3(1,1,1);

        return float4( col , 1. );


      }

      ENDCG

    }
  }

  Fallback Off
  
}