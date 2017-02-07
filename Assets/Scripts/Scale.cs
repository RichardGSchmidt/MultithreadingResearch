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

    [Range(0,4)]
    public float strengthAdjustment;
    public MeshRenderer meshRenderer;
    AutoResetEvent resetEvent;
    Transform t;
    Thread thread;
    bool goingDown;
    Vector3 latestScale;
    bool stop;
    Noise2D noiseMap;
    Noise2D noiseMapTransfer;
    ModuleBase modBaseTransfer;
    ModuleBase baseModule;
    Texture2D texture;
    bool updatedNoiseMapAvailable;
    bool threadRunning;
    Texture2D[] textureCyles;
    MeshFilter meshFilter;
    FractalTorus ftorus;
    int depth;
    public int fractalItterations;

    private void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        ftorus = gameObject.GetComponent<FractalTorus>();
    }

    void Start ()
    {
        
        t = transform;
        latestScale = t.localScale;
        texture = new Texture2D(200, 100);
        thread = new Thread(Run);
        thread.Start();
        resetEvent = new AutoResetEvent(false);
        StartCoroutine(SpawnFractals());

    }
    
    private IEnumerator SpawnFractals()
    {
        if (depth < fractalItterations)
        {
            new GameObject("Fractal Torus").AddComponent<Scale>();
        }
        yield return null;
    }

    void Run()
    {
        while (!stop)
        {
            while (threadRunning)
            {
                DateTime time = DateTime.Now;
                System.Random random = new System.Random(time.GetHashCode());
                baseModule = new Perlin(2 + random.Next(250,1000)/1000.0f, .5*strengthAdjustment, .5, 5, 3, QualityMode.Low);
                RidgedMultifractal ridged = new RidgedMultifractal(3 * random.Next(30, 50), .5, random.Next(1, 3), 3, QualityMode.Low);
                baseModule = new Add(baseModule, ridged);

                noiseMap = new Noise2D(200, 100, baseModule);
                noiseMap.GenerateSpherical(-90, 90, -180, 180);
                noiseMapTransfer = noiseMap;
                modBaseTransfer = baseModule;
                updatedNoiseMapAvailable = true;
                threadRunning = false;
            }
        }
    }
	
	void Update ()
    {
        transform.Rotate(Vector3.down * Time.deltaTime);
        transform.Rotate(Vector3.left * Time.deltaTime * 1.8f);
        transform.Rotate(Vector3.back * Time.deltaTime * strengthAdjustment*100);
        if (updatedNoiseMapAvailable)
        {
            meshRenderer.sharedMaterial.mainTexture = noiseMapTransfer.GetTexture();
            ftorus.GenerateFractalTorus(modBaseTransfer, strengthAdjustment);
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

