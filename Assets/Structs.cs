using UnityEngine;
using System.Collections;

public class Structs{

    public struct VertBasic{
        public Vector3 pos;
        public Vector3 vel;
        public Vector3 nor;
        public Vector2 uv;
        public float life;
        public Vector3 debug;        
    };

    struct VertHairBasic{

		public Vector3 pos;
		public Vector3 oPos;
		public Vector3 ogPos;
		public Vector3 vel;
		public Vector3 nor;
		public Vector2 uv;
		public Vector3 debug;

	};

	struct VertHairSkinned{

		public Vector3 pos;
		public Vector3 oPos;
		public Vector3 vel;
		public Vector3 nor;
		public Vector2 uv;
		public Vector3 debug;

		public Vector3 triIDs;
		public Vector3 triWeights;

	};

	struct VertSkinned{

		public float used;
	    public Vector3 pos;
	    public Vector3 vel;
	    public Vector3 nor;
	    public Vector2 uv;

	    public Vector3 targetPos;

	    public Vector3 bindPos;
	    public Vector3 bindNor;
	    public Vector4 boneWeights;
	    public Vector4 boneIDs;
	    public Vector3 debug;
	};

    public struct Hand{

        public Matrix4x4 localToWorld;
        public Matrix4x4 worldToLocal;
        public Vector3 pos;
        public Vector3 vel;
        public float trigger;
        public Vector3 debug;

    };

    public struct Head{

        public Matrix4x4 localToWorld;
        public Matrix4x4 worldToLocal;
        public Vector3 pos;
        public Vector3 debug;

    };

    public struct Human{

        public Head head;
        public Hand hand1;
        public Hand hand2;

    };


 
    // keeping each number seperate, to help make ordering easier

    public static int HandStructSize = 16 + 16 + 3 + 3 + 1 + 3;
    public static int HeadStructSize = 16 + 16 + 3 + 3;
    public static int HumanStructSize = HeadStructSize + HandStructSize + HandStructSize;

    public static int VertBasicStructSize = 3 + 3 + 3 + 2 + 1 + 3;
    public static int VertHairBasicStructSize = 3+3+ 3+ 3 + 3 + 2 + 3;
    public static int VertHairSkinnedStructSize = 3+ 3+ 3 + 3 + 2 + 3 + 3 + 3;
    public static int VertSkinnedStructSize = 1 + 3 + 3 + 3+2 + 3 +3 +3+4 +4 + 3;


    public static void AssignVertBasicStruct(float[] inValues, int id, out int index, VertBasic v)
    {

        index = id;

        inValues[index++] = v.pos.x;
        inValues[index++] = v.pos.y;
        inValues[index++] = v.pos.z;

        //vel
        inValues[index++] = v.vel.x;
        inValues[index++] = v.vel.y;
        inValues[index++] = v.vel.z;

        //nor
        inValues[index++] = v.nor.x;
        inValues[index++] = v.nor.y;
        inValues[index++] = v.nor.z;

        //uv
        inValues[index++] = v.uv.x;
        inValues[index++] = v.uv.y;

        //life
        inValues[index++] = v.life;

        //debug
        inValues[index++] = v.debug.x;
        inValues[index++] = v.debug.y;
        inValues[index++] = v.debug.z;

    }


    public static void AssignHandStruct(float[] inValues, int id, out int index, Hand h)
    {

        index = id;

        for (int i = 0; i < 16; i++)
        {
            int x = i % 4;
            int y = (int)Mathf.Floor(i / 4);
            inValues[index++] = h.localToWorld[x, y];
        }

        for (int i = 0; i < 16; i++)
        {
            int x = i % 4;
            int y = (int)Mathf.Floor(i / 4);
            inValues[index++] = h.worldToLocal[x, y];
        }

        //pos
        inValues[index++] = h.pos.x;
        inValues[index++] = h.pos.y;
        inValues[index++] = h.pos.z;

        //vel
        inValues[index++] = h.vel.x;
        inValues[index++] = h.vel.y;
        inValues[index++] = h.vel.z;

        //trigger
        inValues[index++] = h.trigger;


        //debug
        inValues[index++] = h.debug.x;
        inValues[index++] = h.debug.y;
        inValues[index++] = h.debug.z;

    }


    public static void AssignHeadStruct(float[] inValues, int id, out int index, Head h)
    {

        index = id;

        for (int i = 0; i < 16; i++)
        {
            int x = i % 4;
            int y = (int)Mathf.Floor(i / 4);
            inValues[index++] = h.localToWorld[x, y];
        }

        for (int i = 0; i < 16; i++)
        {
            int x = i % 4;
            int y = (int)Mathf.Floor(i / 4);
            inValues[index++] = h.worldToLocal[x, y];
        }

        //pos
        inValues[index++] = h.pos.x;
        inValues[index++] = h.pos.y;
        inValues[index++] = h.pos.z;

        //debug
        inValues[index++] = h.debug.x;
        inValues[index++] = h.debug.y;
        inValues[index++] = h.debug.z;

    }

    public static void AssignNullStruct(float[] inValues, int id, out int index, int size)
    {

        index = id;

        for (int i = 0; i < size; i++)
        {
            inValues[index++] = 0;
        }

    }

    public static void AssignHumanStruct(float[] inValues, int id, out int index, Human h)
    {

        index = id;
        AssignHeadStruct(inValues, index, out index, h.head);
        AssignHandStruct(inValues, index, out index, h.hand1);
        AssignHandStruct(inValues, index, out index, h.hand2);

    }

    public static void AssignNullHumanStruct(float[] inValues, int id, out int index)
    {
        index = id;
        AssignNullStruct(inValues, index, out index, HeadStructSize);
        AssignNullStruct(inValues, index, out index, HandStructSize);
        AssignNullStruct(inValues, index, out index, HandStructSize);
    }
}