using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayInfo : MonoBehaviour {

	public TextAsset asset;

    public TextMesh text;
    public TextMesh bigTitle;

    private float oldID=100000000;

    public pointBufferUpdater updater;

    public Info[] FullInfo;


[System.Serializable]
public class Info
{
    public string username;
    public int bitdepth;
    public string description;
    public string created;
    public int num_comments;
    public float avg_rating;
    public string[] tags;
    public string geotag;
    public int num_ratings;
    public int filesize;
    public string type;
    public float duration;
    public float samplerate;
    public int num_downloads;
    public int bitrate;
    public int id;
    public string name;
}
/*
{
"username": "batchku", 
"bitdepth": 16, 
"description": "vocal syllables, created by recording somewhat rhythmic nonesense talk with an SM-58, followed by automated segmentation of the audio.  Slices are numbered from 001-0xx (with slices that were too short taken out), L means long and S means short.", 
"created": "2005-11-23T16:33:09", "
num_comments": 0, 
"avg_rating": 4.0, 
"tags": ["breath", "human", "phoneme", "syllables", "vocal"], 
"geotag": null, 
"num_ratings": 1, 
"filesize": 37794, 
"type": "aif", 
"duration": 0.427573696145, 
"samplerate": 44100.0, 
"num_downloads": 218, 
"bitrate": 705, 
"id": 10000, 
"name": "S_mb-breath-1 014.aif"
},*/

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

string fixJson(string value)
{
    value = "{\"Items\":" + value + "}";
    return value;
}

	// Use this for initialization
	void Start () {

        string j = fixJson( asset.text);
		FullInfo = JsonHelper.FromJson<Info>(j);

//		print( FullInfo[0].username );

        EventManager.OnTriggerDown += OnTriggerDown;
        EventManager.OnTriggerUp += OnTriggerUp;
		
	}

    void OnTriggerDown( GameObject g ){
        text.gameObject.GetComponent<MeshRenderer>().enabled = true;
    }

    void OnTriggerUp( GameObject g ){
        text.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
	
	// Update is called once per frame
	void Update () {

        float n = updater.values[1];

        if( n != oldID ){ switchText( (int)n ); }

        oldID = n;
		
	}

    void switchText( int id ){
        text.text = FullInfo[id].name;
        bigTitle.text = FullInfo[id].name;
    }
}
