using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Operator;
using LibNoise.Generator;

/// <summary>
/// Basic threading research 
/// I'm doing to learn techniques,
/// going to use this as a stepping point to
/// offload texture generation routines to
/// a seperate threads for Ground Growing
/// and Uplifting.
/// </summary>

public class Scale : MonoBehaviour {

    public MeshRenderer meshRenderer;
    AutoResetEvent resetEvent;
    Transform t;
    Thread thread;
    bool goingDown;
    Vector3 latestScale;
    bool stop;
    Noise2D noiseMap;
    ModuleBase baseModule;
    Texture2D texture;
    bool updatedNoiseMapAvailable;
    bool threadRunning;
    
	void Start ()
    {
        t = transform;
        latestScale = t.localScale;
        texture = new Texture2D(400, 200);
        thread = new Thread(Run);
        thread.Start();
        resetEvent = new AutoResetEvent(false);   
    }
    

    void Run()
    {
        while (!stop)
        {
            while (threadRunning)
            {
                DateTime time = DateTime.Now;
                baseModule = new Perlin(2, 2.3, .5, 3, time.GetHashCode(), QualityMode.High);
                noiseMap = new Noise2D(4000, 2000, baseModule);
                noiseMap.GenerateSpherical(-90, 90, -180, 180);
                updatedNoiseMapAvailable = true;
                threadRunning = false;
            }
        }
    }
	
	void Update ()
    {
        transform.Rotate(Vector3.down * Time.deltaTime);
        if (updatedNoiseMapAvailable)
        {
            meshRenderer.sharedMaterial.mainTexture = noiseMap.GetTexture();
            updatedNoiseMapAvailable = false;
        }
        else if (!threadRunning)
        {
            threadRunning = true;
        }
	}

    public void OnDestroy()
    {
        stop = true;
        threadRunning = false;
        thread.Abort();
    }

    public void OnApplicationQuit()
    {
        stop = true;
        threadRunning = false;
        thread.Abort();
    }
}
