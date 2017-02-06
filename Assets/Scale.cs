using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class Scale : MonoBehaviour {
    AutoResetEvent resetEvent;
    Transform t;
    Thread thread;
    bool goingDown;
    Vector3 latestScale;
    bool stop;

	// Use this for initialization
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
	
	// Update is called once per frame
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
