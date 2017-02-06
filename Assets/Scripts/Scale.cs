using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Operator;
using LibNoise.Generator;

/// <summary>
/// Basic threading research from a tutorial 
/// I'm doing to learn techniques,
/// going to use this as a stepping point to
/// offload texture generation routines to
/// a seperate threads for Ground Growing
/// and Uplifting.
/// </summary>

public class Scale : MonoBehaviour {
    AutoResetEvent resetEvent;
    Transform t;
    Thread thread;
    bool goingDown;
    Vector3 latestScale;
    bool stop;
    Noise2D noiseMap;
    ModuleBase baseModule;
    
	void Start ()
    {
        t = transform;
        latestScale = t.localScale;
        thread = new Thread(Run);
        thread.Start();
        resetEvent = new AutoResetEvent(false);
	}
    

    void Run()
    {
        DateTime time = DateTime.Now;
        while (!stop)
        {


            baseModule = new Perlin(1.2, 2.3, 1, 3, time.GetHashCode(), QualityMode.High);
            noiseMap = new Noise2D(200, 200, baseModule);
            noiseMap.GenerateSpherical(-90, 90, -180, 180);
            var now = DateTime.Now;
            var deltaTime = now - time;
            time = now;
            resetEvent.WaitOne();
            latestScale += latestScale *(float)deltaTime.TotalSeconds* (goingDown ? -1f : 1f);


            if ((goingDown && latestScale.magnitude < 1) || (!goingDown && latestScale.magnitude > 5))
            {
                goingDown = !goingDown;
            }
        }
    }
	
	void Update ()
    {
        resetEvent.Set();
        t.localScale = latestScale;
	}

    public void OnDestroy()
    {
        stop = true;
        thread.Abort();
    }

    public void OnApplicationQuit()
    {
        stop = true;
        thread.Abort();
    }
}
