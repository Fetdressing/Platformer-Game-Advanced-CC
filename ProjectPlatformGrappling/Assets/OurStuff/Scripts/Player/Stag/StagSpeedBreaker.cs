using UnityEngine;
using System.Collections;

public class StagSpeedBreaker : BaseClass {
    [HideInInspector] public bool active = false;
    StagMovement stagMovement; //kunna skicka att man gjort hit osv
    PowerManager pm;

    Renderer[] renderers;
    Collider[] colliders;
    IEnumerator fadeOut;
    float startAlpha = 0.6f;

    int min_DistanceThreshhold = 4; //hur nära man får vara activationPoint för att kollidea
    Vector3 activationPoint = Vector3.zero;

    Transform internalLastUnitHit; //används för o mecka collision

    private int powerGained = 10;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        stagMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<StagMovement>();
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PowerManager>();
        colliders = GetComponentsInChildren<Collider>();
        renderers = GetComponentsInChildren<Renderer>();

        Disable();
        initTimes++;
    }

    void OnTriggerEnter(Collider col)
    {
        HealthSpirit h = col.GetComponent<HealthSpirit>();
        if(h != null && h.IsAlive())
        {
            stagMovement.BreakDash(false);
            stagMovement.IgnoreCollider(false, internalLastUnitHit);

            internalLastUnitHit = col.transform;
            stagMovement.lastUnitHit = col.transform;
            stagMovement.IgnoreCollider(0.6f, col.transform); //så man inte collidar med den när man åker igenom
            //stagMovement.IgnoreCollider(true, col.transform);
            if(stagMovement.staggDashIE != null)
            {
                stagMovement.StopCoroutine(stagMovement.staggDashIE);
            }
            stagMovement.staggDashIE = stagMovement.StaggDash(true, 0.024f, 0.2f);
            stagMovement.StartCoroutine(stagMovement.staggDashIE);
            //stagMovement.Dash(true, true); //använd kamera riktningen
            //Debug.Log("Felet med riktningen är att man kallar dash före stagger, gör så att de körs i rad");
            //stagMovement.Stagger(0.25f);
            stagMovement.AddJumpsAvaible(stagMovement.jumpAmount, stagMovement.jumpAmount);
            h.AddHealth(-2);
            pm.AddPower(1, 80);
        }
    }

    //IEnumerator StagDash()
    //{

    //}

    public void Activate()
    {
        if (active) return;
        activationPoint = transform.position;

        active = true;
        ToggleColliders(true);
        ToggleRenderers(true);

        stagMovement.IgnoreLayer(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Unit"), true); //ignorera all kollision mellan spelarobjektet och alla fiender, så man inte studsar mot dem under dash
    }

    public void Disable()
    {
        if (initTimes == 0) return;
        if (fadeOut != null || renderers[0].enabled == false) return;

        stagMovement.IgnoreLayer(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Unit"), false);

        fadeOut = FadeOut(0.5f);
        StartCoroutine(fadeOut);
    }

    public void InstantDisable()
    {
        if (initTimes == 0) return;
        ToggleColliders(false);
        ToggleRenderers(false);

        stagMovement.IgnoreLayer(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Unit"), false);

        if (fadeOut != null)
        {
            StopCoroutine(fadeOut);
        }
        fadeOut = null;
        active = false;
    }

    void ToggleColliders(bool b)
    {
        for(int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = b;
        }
    }

    IEnumerator FadeOut(float time)
    {
        
        float currAlpha = startAlpha;
        while (currAlpha > 0)
        {
            currAlpha -= startAlpha / ((startAlpha / Time.deltaTime) * time);
            if (currAlpha < 0) currAlpha = 0;

            for (int i = 0; i < renderers.Length; i++)
            {
                Color c = renderers[i].material.color;
                renderers[i].material.color = new Color(c.r, c.g, c.b, currAlpha);
            }
            yield return new WaitForEndOfFrame();
        }
        ToggleColliders(false);
        ToggleRenderers(false);
        fadeOut = null;
        active = false;
    }

    void ToggleRenderers(bool b)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = b;
            Color c = renderers[i].material.color;
            renderers[i].material.color = new Color(c.r, c.g, c.b, startAlpha);
        }
    }
}
