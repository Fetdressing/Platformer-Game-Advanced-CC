using UnityEngine;
using System.Collections;

public class BreakerObject : BaseClass {
    public GameObject disableRootObj; //ifall man har en root som oxå ska disablas
    private Transform thisTransform;
    public Renderer thisRenderer;
    private Collider[] thisColliders;

    public bool refresh = true; //ifall den ska komma tillbaks av sig själv efter den försvunnit
    public string[] collisionFadeLayers = { };
    public string[] fadeTags;
    public GameObject particleEffect; //when hitting stuff

    private Material startMaterial;
    public Material phaseOutMaterial;

    public float fadeAmount = 10;

    public float fadeTime = 4;
    float currAlpha = 1;

    private bool fading = false;

    public Animation animationH;
    public AnimationClip breakAnim;
    public AnimationClip renewAnim;

    public AnimationClip idleAnim;
    public AnimationClip breakingAnim;
    public float animSpeed = 1.0f;
    public float idleASpeed = 1.0f;

    IEnumerator fadeIn;
    IEnumerator fadeOut;

    public GameObject deathPar;
    // Use this for initialization
    void Start()
    {
        if (initTimes != 0) return;
        Init();
    }

    void Awake()
    {
        if (initTimes != 0) return;
        Init();
    }

    public override void Init()
    {
        base.Init();
        thisTransform = this.transform;

        if (thisRenderer == null)
        {
            thisRenderer = thisTransform.GetComponent<Renderer>();
        }

        startMaterial = thisRenderer.material;

        thisColliders = thisTransform.GetComponentsInChildren<Collider>();

        if (animationH == null && breakingAnim != null)
        {
            animationH = transform.GetComponent<Animation>();
            if (animationH != null)
            {
                if (breakingAnim != null)
                {
                    animationH[breakingAnim.name].speed = idleASpeed;
                }
            }
        }

        transform.tag = "BreakerObject";

        Reset();

        initTimes++;
    }

    public override void Reset()
    {
        base.Reset();
        StopAllCoroutines();
        fading = false;
        currAlpha = 1;

        Color c = thisRenderer.material.color;
        thisRenderer.material.color = new Color(c.r, c.g, c.b, currAlpha);
        gameObject.SetActive(true);
        if(disableRootObj != null)
        {
            disableRootObj.SetActive(true);
        }
        //StartCoroutine(PhaseLifetime());
    }

    //void OnTriggerEnter(Collider col)
    //{
    //    if(col.tag == "Player")
    //    {
    //        if (fading)
    //            return;
    //        StartCoroutine(FadeOut());
    //    }
    //}

    void Update()
    {
        if(!fading)
        {
            if (animationH != null && animationH.isPlaying == false)
            {
                animationH.CrossFade(idleAnim.name);
            }
        }
    }

    public void Break() //låt någon kalla på det
    {
        if (initTimes == 0) return;
        if (fading)
            return;

        if(fadeOut != null)
        {
            StopCoroutine(fadeOut);
        }

        if (fadeIn != null)
        {
            StopCoroutine(fadeIn);
        }

        fadeOut = FadeOut();
        StartCoroutine(fadeOut);
    }

    public void ReturnIn() //existera igen!
    {
        if (initTimes == 0) return;
        if (fadeOut != null)
        {
            StopCoroutine(fadeOut);
        }

        if (fadeIn != null)
        {
            StopCoroutine(fadeIn);
        }

        fadeIn = FadeIn();
        StartCoroutine(fadeIn);
    }

    public void ReturnInInstant()
    {
        if (initTimes == 0) return;

        gameObject.SetActive(true);
        if (disableRootObj != null)
        {
            disableRootObj.SetActive(true);
        }

        if (fadeOut != null)
        {
            StopCoroutine(fadeOut);
        }

        if (fadeIn != null)
        {
            StopCoroutine(fadeIn);
        }

        if (animationH != null)
        {
            animationH.Play(renewAnim.name);
        }

        for (int i = 0; i < thisColliders.Length; i++)
        {
            thisColliders[i].enabled = true;
        }

        Color c = thisRenderer.material.color;
        currAlpha = 1;
        thisRenderer.material.color = new Color(c.r, c.g, c.b, currAlpha);
        fading = false;
    }

    IEnumerator FadeOut()
    {
        fading = true;
        
        while (currAlpha > 0.0f)
        {
            Color c = thisRenderer.material.color;
            
            currAlpha -= 1 / ((1 / Time.deltaTime) * fadeTime);
            thisRenderer.material.color = new Color(c.r, c.g, c.b, currAlpha);

            if(animationH != null)
            {
                animationH[breakingAnim.name].speed = animSpeed * (1 - currAlpha);
                animationH.CrossFade(breakingAnim.name);
            }

            yield return new WaitForEndOfFrame();
        }

        if (animationH != null)
        {
            animationH.Play(breakAnim.name);
        }

        for (int i = 0; i < thisColliders.Length; i++)
        {
            thisColliders[i].enabled = false;
        }

        if (refresh)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            gameObject.SetActive(false);
            if (disableRootObj != null)
            {
                disableRootObj.SetActive(false);
            }
        }

        if(deathPar != null)
        {
            GameObject temp = Instantiate(deathPar.gameObject);
            temp.transform.position = transform.position;
            Destroy(temp.gameObject, 3);
        }
        fadeOut = null;
    }

    IEnumerator FadeIn()
    {
        gameObject.SetActive(true);
        if (disableRootObj != null)
        {
            disableRootObj.SetActive(true);
        }
        
        Color c;

        c = thisRenderer.material.color;

        while (currAlpha < 1.0f)
        {
            c = thisRenderer.material.color;
            
            currAlpha += 1 / ((1 / Time.deltaTime) * (fadeTime * 2));
            thisRenderer.material.color = new Color(c.r, c.g, c.b, currAlpha * 0.2f);

            yield return new WaitForEndOfFrame();
        }

        if (animationH != null)
        {
            animationH.Play(renewAnim.name);
        }

        for (int i = 0; i < thisColliders.Length; i++)
        {
            thisColliders[i].enabled = true;
        }
        thisRenderer.material.color = new Color(c.r, c.g, c.b, currAlpha);
        fading = false;

        fadeIn = null;
    }

    void OnTriggerEnter(Collider col)
    {
        //for(int i = 0; i < collisionFadeLayers.Length; i++)
        //{
        //    if(LayerMask.LayerToName(col.gameObject.layer) == collisionFadeLayers[i])
        //    {
        //        Break();
        //        return;
        //    }
        //}

        if (particleEffect != null)
        {
            GameObject tempPar = Instantiate(particleEffect.gameObject);
            tempPar.transform.position = col.transform.position;
            Destroy(tempPar.gameObject, 3);
        }

        for (int i = 0; i < fadeTags.Length; i++)
        {
            if(col.tag == fadeTags[i])
            {
                Break();
                return;
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (particleEffect != null)
        {
            GameObject tempPar = Instantiate(particleEffect.gameObject);
            tempPar.transform.position = col.transform.position;
            Destroy(tempPar.gameObject, 3);
        }

        for (int i = 0; i < fadeTags.Length; i++)
        {
            if (col.gameObject.tag == fadeTags[i])
            {
                Break();
                return;
            }
        }
    }

    void OnDisable()
    {
        if (!refresh)
        {
            StopAllCoroutines();
        }
    }
}
