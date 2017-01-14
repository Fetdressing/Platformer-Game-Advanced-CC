using UnityEngine;
using System.Collections;

public class WallJumpObj : BaseClass {
    StagMovement stagMovement;

    public GameObject particleHit;
	// Use this for initialization
	void Start () {
	
	}

    public override void Init()
    {
        base.Init();
        stagMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<StagMovement>();
    }


    void OnCollisionStay(Collision col)
    {
        if(col.gameObject.tag == "Player")
        {
            stagMovement.AddJumpsAvaible(stagMovement.jumpAmount, stagMovement.jumpAmount);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {

        }
    }
}
