using UnityEngine;
using System.Collections;

public class PhaseOutObject : BaseClass {
    private Transform thisTransform;
    private Renderer thisRenderer;
    private Collider[] thisColliders;


    private Material startMaterial;
    public Material phaseOutMaterial;

    //public float phaseTime = 1.0f;
    public float phaseCooldown = 1.5f;
    public float startTime = 1.0f; //när hela börjar köras, kan behövas offset för att få dem ur fas

    public AudioSource audioSource;

    float currAlpha = 1;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        thisTransform = this.transform;
        thisRenderer = thisTransform.GetComponent<Renderer>();
        startMaterial = thisRenderer.material;

        thisColliders = thisTransform.GetComponentsInChildren<Collider>();
        Reset();

        InvokeRepeating("TogglePhase", startTime, phaseCooldown);
    }

    public override void Reset()
    {
        base.Reset();
        StopAllCoroutines();
        //StartCoroutine(PhaseLifetime());
    }

    void Update()
    {
        bool b = thisColliders[0].enabled;
        Color c = thisRenderer.material.color;
        Color wC;

        if (b)
        {
            //wC = new Color(c.r, c.g, c.b, 0.0f); //fadea ut

            currAlpha -= 1 / ((1 / Time.deltaTime) * phaseCooldown);

            thisRenderer.material.color = new Color(c.r, c.g, c.b, currAlpha);
        }
        else
        {
            //wC = new Color(c.r, c.g, c.b, 1.0f); //fadea in
        }

        
    }

    //IEnumerator PhaseLifetime()
    //{
    //    yield return new WaitForSeconds(startTime);
    //    while(this != null)
    //    {
    //        yield return new WaitForSeconds(phaseCooldown);
    //        TogglePhase();
    //        yield return new WaitForSeconds(phaseTime);
    //        TogglePhase();
    //    }
    //}

    public void TogglePhase()
    {
        bool b = thisColliders[0].enabled;
        Color c;

        if (b)
        {
            currAlpha = 1;
            thisRenderer.material = phaseOutMaterial;

            c = thisRenderer.material.color;
            thisRenderer.material.color = new Color(c.r, c.g, c.b, 0.0f);
        }
        else
        {
            currAlpha = 1;
            thisRenderer.material = startMaterial;

            c = thisRenderer.material.color;
            thisRenderer.material.color = new Color(c.r, c.g, c.b, 1.0f);

            if(audioSource != null)
            {
                audioSource.Play();
            }
        }

        for(int i = 0; i < thisColliders.Length; i++)
        {
            thisColliders[i].enabled = !b;
        }
    }
}
