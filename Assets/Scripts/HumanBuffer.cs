using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HumanBuffer : MonoBehaviour {
    public GameObject Human;
    public int numberHumans =1;
    public ComputeBuffer _buffer;
    private float[] inValues;

    // Use this for initialization
    void Start() {
        RebuildHumans();
    }
    void RebuildHumans() {

       
        inValues = new float[1 * Structs.HumanStructSize];

        // Rebuild buffers
        createBuffers();
    }

    private void createBuffers() {
        _buffer = new ComputeBuffer(1, Structs.HumanStructSize * sizeof(float));
    }


    void FixedUpdate()
    {
        int index = 0;

        Structs.AssignHumanStruct(inValues, index, out index, Human.GetComponent<HumanInfo>().human);
     
        _buffer.SetData(inValues);
    }


    // Update is called once per frame
    void Update()
    {

    }
}