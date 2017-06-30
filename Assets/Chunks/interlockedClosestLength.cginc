
void interlockedClosestLength(float2 value , int threadid ){

  accumVal[threadid].xy = value;

  // accumulate a bit in parralel
  GroupMemoryBarrierWithGroupSync();
  
  if((threadid&0x3)==0){

    float2 fVal = float2(10000,1000000);

    for( int i = 0; i < 4; i++){
      float2 v = accumVal[threadid + i ].xy;
      if( v.x < fVal.x ){
        fVal = v;
      }
    }

    accumVal[threadid+0].xy = fVal;

  }
  GroupMemoryBarrierWithGroupSync();
  if(threadid==0){
    float2 result = accumVal[0].xy;
    for(int i=4; i<NR_THREADS; i+=0x4){ 
      if( accumVal[i].x < result.x ){
        result = accumVal[i].xy;
      } 

    }
    interlockedFullValue.xy = result;
  } 
  GroupMemoryBarrierWithGroupSync();
  
}

