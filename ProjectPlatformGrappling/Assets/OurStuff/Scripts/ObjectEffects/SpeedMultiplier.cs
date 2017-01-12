using UnityEngine;
using System.Collections;

public class SpeedMultiplier : MonoBehaviour {
    public float multiplier = 1.2f;
    public float duration = 0.5f;

    void OnTriggerStay(Collider col)
    {
        if(col.tag == "Player")
        {
            col.GetComponent<StagMovement>().ApplySpeedMultiplier(multiplier, duration);
        }
    }
}
