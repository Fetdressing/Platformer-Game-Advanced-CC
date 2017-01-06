using UnityEngine;
using System.Collections;

public class ObjectSpawner : BaseClass {
    public Transform spawnObject;
    public Transform spawnPoint;

    public GameObject particleEffect;

    public float cooldownTime = 3.0f;
    private float cooldownTimer = 0.0f;

    private Quaternion startRotation;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        startRotation = spawnObject.rotation;
        if(spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }

    public override void Reset()
    {
        base.Reset();
        cooldownTimer = 0.0f;
    }


    void OnTriggerEnter(Collider col)
    {
        if (cooldownTimer > Time.time) return;

        cooldownTimer = Time.time + cooldownTime;

        GameObject parTemp = GameObject.Instantiate(particleEffect.gameObject);
        parTemp.transform.position = spawnObject.position;
        Destroy(parTemp, 3);

        Rigidbody tempRig = spawnObject.GetComponent<Rigidbody>();
        spawnObject.rotation = startRotation;
        if (tempRig != null)
        {
            tempRig.velocity = new Vector3(0, 0, 0);
        }
        spawnObject.position = spawnPoint.position;
        spawnObject.gameObject.SetActive(true);

        GameObject parTemp2 = GameObject.Instantiate(particleEffect.gameObject);
        parTemp.transform.position = spawnObject.position;
        Destroy(parTemp, 3);
    }
}
