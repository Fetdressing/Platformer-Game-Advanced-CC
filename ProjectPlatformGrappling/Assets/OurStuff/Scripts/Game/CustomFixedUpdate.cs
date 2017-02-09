using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFixedUpdate {
    private System.Action updateFunction;
    private static System.Diagnostics.Stopwatch m_Timeout = new System.Diagnostics.Stopwatch();

    [HideInInspector]
    public float ufDeltaTime = 0.02f; //hur ofta Fixedupdate den ska köras

    float reachingTime = 0.0f;
    float reacherTime = 0.0f;
    float m_MaxAllowedTimestep = 0.05f; //ifall beräkningarna av ufUpdates tar för långt tid så kan värden bli fel typ
                                        // Use this for initialization
    void Start () {
		
	}

    public CustomFixedUpdate(float aFixedDeltaTime, System.Action aFixecUpdateCallback)
    {
        ufDeltaTime = aFixedDeltaTime;
        updateFunction = aFixecUpdateCallback;
    }

    public bool UpdateF(float freq) //viktigt att denna endast körs från ett ställe
    {
        m_Timeout.Reset();
        m_Timeout.Start();

        reachingTime += freq;
        while (reacherTime < reachingTime)
        {
            reacherTime += ufDeltaTime;

            updateFunction();//kör alla funktioner

            if ((m_Timeout.ElapsedMilliseconds / 1000.0f) > m_MaxAllowedTimestep)
                return false;
        }

        return true;
    }
}
