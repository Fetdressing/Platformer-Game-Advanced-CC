using UnityEngine;
using System.Collections;

public class Teleport : MonoBehaviour {
    public Transform teleportPosTransform;
    public GameObject effectObject;

    void OnTriggerEnter(Collider col)
    {
        if (effectObject != null)
        {
            GameObject parTemp = GameObject.Instantiate(effectObject.gameObject);
            parTemp.transform.position = col.transform.position;
            Destroy(parTemp, 3);
        }

        col.transform.position = teleportPosTransform.position;

        if (effectObject != null)
        {
            GameObject parTemp2 = GameObject.Instantiate(effectObject.gameObject);
            parTemp2.transform.position = col.transform.position;
            Destroy(parTemp2, 3);
        }
    }
}
