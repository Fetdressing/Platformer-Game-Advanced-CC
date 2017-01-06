using UnityEngine;
using System.Collections;

public class Spawnpoint : BaseClass {
    [HideInInspector]
    public bool isPassed = false;

    public Renderer thisRenderer;
    public Material takenMaterial;

    private AudioSource audioSource;
    public AudioClip takenAudioClip;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        audioSource = GetComponent<AudioSource>();
        isPassed = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (isPassed) return;

        if(col.tag == "Player")
        {
            if(thisRenderer != null && takenMaterial != null)
            {
                thisRenderer.material = takenMaterial;
            }

            if(audioSource != null && takenAudioClip != null)
            {
                audioSource.PlayOneShot(takenAudioClip);
            }

            SpawnManager spawnManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>();
            spawnManager.SetLatestSpawn(transform);

            isPassed = true;
        }
    }
}
