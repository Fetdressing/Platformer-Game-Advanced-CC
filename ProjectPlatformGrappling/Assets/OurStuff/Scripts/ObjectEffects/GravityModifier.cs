using UnityEngine;
using System.Collections;

public class GravityModifier : MonoBehaviour {
    public float newGravityValue = 1f;

    void OnTriggerStay(Collider col)
    {
        if(col.tag == "Player")
        {
            col.GetComponent<StagMovement>().currGravityModifier = newGravityValue;
        }
    }
}
