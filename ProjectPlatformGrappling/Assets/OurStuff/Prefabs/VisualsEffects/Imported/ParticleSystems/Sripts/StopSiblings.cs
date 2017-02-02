using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StopSiblings : MonoBehaviour {

    public ParticleSystem m_siblingSystem1;
	// Use this for initialization
	void Start () {
		
	}
	
    void OnTriggerEnter()
    {
        if(m_siblingSystem1 != null)
        {
            m_siblingSystem1.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
