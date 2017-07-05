/*
using UnityEngine;
using System.Collections.Generic;
using Normal.Realtime;
using System.Linq;

public class HumanBuffer : MonoBehaviour {
    public Realtime realtime;
    public GameObject[] Humans;
    public int numberHumans { get { return realtime.players.Count; } }
    public ComputeBuffer _buffer;
    private float[] inValues;

    // Use this for initialization
    void Start() {
        RebuildHumans();
    }

    void OnEnable() {
        realtime.playerJoinedRoom += PlayerJoinedRoom;
        realtime.playerLeftRoom   += PlayerLeftRoom;
    }

    void OnDisable() {
        realtime.playerJoinedRoom -= PlayerJoinedRoom;
        realtime.playerLeftRoom   -= PlayerLeftRoom;
    }

    void PlayerJoinedRoom(Player player) {
        RebuildHumans();
    }

    void PlayerLeftRoom(Player player) {
        RebuildHumans();
    }

    void RebuildHumans() {
        // Reset size of inValues
        if (realtime.players.Count > 0)
            inValues = new float[numberHumans * StructHelper.HumanStructSize];
        else
            inValues = new float[1 * StructHelper.HumanStructSize];

        // Rebuild buffers
        createBuffers();
    }

    private void createBuffers() {
        if (_buffer != null)
            _buffer.Release();

        if (realtime.players.Count > 0)
            _buffer = new ComputeBuffer(numberHumans, StructHelper.HumanStructSize * sizeof(float));
        else
            _buffer = new ComputeBuffer(1, StructHelper.HumanStructSize * sizeof(float));
    }


    void FixedUpdate()
    {
        int index = 0;
        if (realtime.players.Count > 0)
        {
            foreach (Player player in realtime.players)
            {
                
                StructHelper.AssignHumanStruct(inValues, index, out index, player.GetComponent<HumanInfo>().human);
            }
        } else {
            StructHelper.AssignNullStruct(inValues, index, out index, StructHelper.HumanStructSize);
        }

        _buffer.SetData(inValues);
    }


    // Update is called once per frame
    void Update()
    {

    }
}*/