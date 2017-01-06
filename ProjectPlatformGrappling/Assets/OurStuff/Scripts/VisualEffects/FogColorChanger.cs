using UnityEngine;
using System.Collections;

public class FogColorChanger : BaseClass {
    EnvironmentChanger environmentChanger;
    public Color[] colors;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        environmentChanger = GameObject.FindGameObjectWithTag("Manager").GetComponent<EnvironmentChanger>();
    }

    void OnTriggerStay(Collider col)
    {
        if(col.transform.tag == "Player")
        {
            if(colors.Length > 0)
            {
                environmentChanger.StartFogColorChange(colors);
            }
        }
    }
}
