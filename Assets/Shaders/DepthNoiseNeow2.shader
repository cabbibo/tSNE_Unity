// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DepthNoiseNeow2" {

  Properties {
  
 		// This is how many steps the trace will take.
 		// Keep in mind that increasing this will increase
 		// Cost
    _NumberSteps( "Number Steps", Int ) = 3

    // Total Depth of the trace. Deeper means more parallax
    // but also less precision per step
    _TotalDepth( "Total Depth", Float ) = 0.16


    _NoiseSize( "Noise Size", Float ) = 10
    _NoiseSpeed( "Noise Speed", Float ) = .3
    _HueSize( "Hue Size", Float ) = .3
    _BaseHue( "Base Hue", Float ) = .3




  }

  SubShader {


    Pass {

      CGPROGRAM

      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"


      uniform int _NumberSteps;
      uniform float _TotalDepth;
      uniform float _NoiseSize;
      uniform float _NoiseSpeed;
      uniform float _HueSize;
      uniform float _BaseHue;


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

      float3 hsv(float h, float s, float v){
        return lerp( float3( 1.0,1,1 ), clamp(( abs( frac(h + float3( 3.0, 2.0, 1.0 ) / 3.0 )
        					 * 6.0 - 3.0 ) - 1.0 ), 0.0, 1.0 ), s ) * v;
      }


				// Taken from https://www.shadertoy.com/view/4ts3z2
			// By NIMITZ  (twitter: @stormoid)
			// good god that dudes a genius...

			float tri( float x ){ 
			  return abs( frac(x) - .5 );
			}

			float3 tri3( float3 p ){
			 
			  return float3( 
			      tri( p.z + tri( p.y * 1. ) ), 
			      tri( p.z + tri( p.x * 1. ) ), 
			      tri( p.y + tri( p.x * 1. ) )
			  );

			}
			                                 
			float triNoise3D( float3 p, float spd , float time){
			  
			  float z  = 1.4;
				float rz =  0.;
			  float3  bp =   p;

				for( float i = 0.; i <= 3.; i++ ){
			   
			    float3 dg = tri3( bp * 2. );
			    p += ( dg + time * .1 * spd );

			    bp *= 1.8;
					z  *= 1.5;
					p  *= 1.2; 
			      
			    float t = tri( p.z + tri( p.x + tri( p.y )));
			    rz += t / z;
			    bp += 0.14;

				}

				return rz;

			}

    


      float getFogVal( float3 pos ){


      	//float s1 = 3 * _NoiseSize 

        float v =  triNoise3D( pos , _NoiseSpeed , _Time.y ) * 2;

        return v;
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



        float3 p;

        for( int i = 0; i < _NumberSteps; i++ ){


					float stepVal = float(i)/_NumberSteps;
        	// We get out position by adding the ray direction to the ray origin
        	// Keep in mind thtat because the ray direction is normalized, the depth
        	// into the step will be defined by our number of steps and total depth
          p = ro + rd * stepVal * _TotalDepth ;
  	
		

					// We get our value of how much of the volumetric material we have gone through
					// using the position
					float val = getFogVal( p * _NoiseSize );	

					if( val > .55 && val < .65 ){ 

						val = 1 - (abs( val - .6 ) * 10);
					}else{
						val = 0;
					}


          col += 3 * hsv( stepVal * _HueSize + _BaseHue, 1 , 1) * val ;


        }

        col /= _NumberSteps;

    


		    fixed4 color;
        color = fixed4( col , 1. );
        return color;
      }

      ENDCG
    }
  }
  FallBack "Diffuse"
}
