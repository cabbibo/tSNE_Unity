// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/VolumetricBasic" {
	
  Properties {
  
 		// This is how many steps the trace will take.
 		// Keep in mind that increasing this will increase
 		// Cost
    _NumberSteps( "Number Steps", Int ) = 3

    // Total Depth of the trace. Deeper means more parallax
    // but also less precision per step
    _TotalDepth( "Total Depth", Float ) = 0.16


  }

  SubShader {


    Pass {

      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"


      uniform int _NumberSteps;
      uniform float _TotalDepth;


      struct VertexIn{
         float4 position  : POSITION; 
         float3 normal    : NORMAL; 
         float4 texcoord  : TEXCOORD0; 
         float4 tangent   : TANGENT;
      };


      struct VertexOut {
          float4 pos    	: POSITION; 
          float3 normal 	: NORMAL; 
          float4 uv     	: TEXCOORD0; 
          float3 ro     	: TEXCOORD1;
          float3 rd     	: TEXCOORD2;
      };


      float getFogVal( float3 pos ){

      	return abs( sin( pos.x * 40) + sin(pos.y * 40 ) + sin( pos.z * 40 ));
      }
      
      VertexOut vert(VertexIn v) {
        
        VertexOut o;

        o.normal = v.normal;

        o.uv = v.texcoord;
       
  
        // Getting the position for actual position
        o.pos = UnityObjectToClipPos(  v.position );
     
        float3 mPos = mul( unity_ObjectToWorld , v.position );

        // The ray origin will be right where the position is of the surface
        o.ro = v.position;


        float3 camPos = mul( unity_WorldToObject , float4( _WorldSpaceCameraPos , 1. )).xyz;

        // the ray direction will use the position of the camera in local space, and 
        // draw a ray from the camera to the position shooting a ray through that point
        o.rd = normalize( v.position.xyz - camPos );

        return o;

      }

      // Fragment Shader
      fixed4 frag(VertexOut i) : COLOR {

				// Ray origin 
        float3 ro 			= i.ro;

        // Ray direction
        float3 rd 			= i.rd;       

        // Our color starts off at zero,   
        float3 col = float3( 0.0 , 0.0 , 0.0 );

        // the accumulation of our stepping through the object
        float traceAccumulation = 0;

        float3 p;

        for( int i = 0; i < _NumberSteps; i++ ){

        	// We get out position by adding the ray direction to the ray origin
        	// Keep in mind thtat because the ray direction is normalized, the depth
        	// into the step will be defined by our number of steps and total depth
          p = ro + rd * float( i ) * _TotalDepth / _NumberSteps;
  	
	
					// We get our value of how much of the volumetric material we have gone through
					// using the position
					float val = getFogVal( p );	

          traceAccumulation += val;
          col += float3( val , val , val );


        }

        col /= _NumberSteps;

        // Not using this in the current shader, 
        // but there is a bunch of fun stuff you
        // can use it for!
        traceAccumulation /= _NumberSteps;


		    fixed4 color;
        color = fixed4( col , 1. );
        return color;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}