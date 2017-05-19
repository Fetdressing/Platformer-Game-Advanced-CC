using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEvent : BaseClass {
    public float time = 20;
    public float randomValue = 5;
    public bool repeat = false;
    public FunctionEvent fEventStart;
    
    // Use this for initialization
    void Start()
    {
        Reset();
    }

    public override void Reset()
    {
        base.Reset();

        StopAllCoroutines();
        StartCoroutine(RunEvent());
    }
	
    IEnumerator RunEvent()
    {
        float vTime = time + Random.Range(-randomValue, randomValue);
        if(repeat)
        {
            while(this != null)
            {
                yield return new WaitForSeconds(vTime);
                fEventStart.Invoke();
                vTime = time + Random.Range(-randomValue, randomValue);
            }
        }
        else
        {
            yield return new WaitForSeconds(vTime);
            fEventStart.Invoke();
        }
    }
}
