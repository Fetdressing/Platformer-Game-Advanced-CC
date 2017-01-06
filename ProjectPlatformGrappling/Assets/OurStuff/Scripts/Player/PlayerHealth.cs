using UnityEngine;
using System.Collections;

public class PlayerHealth : Health {
    private Camera activeCamera;
	// Use this for initialization

    public override void Init()
    {
        base.Init();
        activeCamera = GameObject.FindGameObjectWithTag("Manager").GetComponent<CameraManager>().cameraPlayerFollow;
    }


    public override bool AddHealth(int h)
    {
        bool b = base.AddHealth(h);
        float damagedFactor = 1 - GetCurrHealth() / maxHealth;

        if(damagedFactor < 0.3f) //bloodscreen eller nått annat?
        {
            
        }

        return b;
    }
}
