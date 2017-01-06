using UnityEngine;
using System.Collections;

public class SceneHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void UnloadCurrentScene()
    {
        BaseClass[] baseClasses = FindObjectsOfType<BaseClass>();
        for(int i = 0; i < baseClasses.Length; i++)
        {
            baseClasses[i].Dealloc();
        }
    }
}
