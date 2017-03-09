using UnityEngine;
using System.Collections;

public class StagSpeedBreaker : BaseClass {
    [System.NonSerialized] public bool active = false;
    StagMovement stagMovement; //kunna skicka att man gjort hit osv
    PowerManager pm;

    Renderer[] renderers;
    Collider[] colliders;
    IEnumerator fadeOut;
    float startAlpha = 0.6f;

    bool lastUnitHit_Viable = true; //ifall jag kan träffa internalLastUnitHit igen, för att hantera så man inte knockar i samma unit om den har dubbla colliders tex
    Transform internalLastUnitHit; //används för o mecka collision

    private int powerGained = 10;
    bool ready = true; //så man bara träffar ett target/dash
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

    public override void Reset()
    {
        base.Reset();
        ready = true;
    }

    void Update()
    {
        if (initTimes == 0) return;
        if(internalLastUnitHit != null && lastUnitHit_Viable == false)
        {
            float reqDistance = 55; //avståndet som spelaren måste komma ifrån senaste targetet för att man ska kunna dasha på den igen
            if(Vector3.Distance(stagMovement.transform.position, internalLastUnitHit.position) > reqDistance) //ett värde där jag tror spelaren är utanför collidern
            {
                lastUnitHit_Viable = true;
            }
        }
    }

    public void UnIgnoreLastUnitHit() //unignorerar collidern
    {
        if (initTimes == 0) return;
        stagMovement.IgnoreCollider(false, internalLastUnitHit);
    }

    void OnTriggerEnter(Collider col)
    {
        DashHit(col.transform);
    }

    public void DashHit(Transform col)
    {
        if (initTimes == 0) return;
        if (lastUnitHit_Viable == false && col.transform == internalLastUnitHit) return;
        if (!ready) return;
        HealthSpirit h = col.GetComponent<HealthSpirit>();
        if (h != null && h.IsAlive())
        {
            if (h.isDashTarget)
            {
                ready = false;
                stagMovement.BreakDash(false);
                stagMovement.IgnoreCollider(false, internalLastUnitHit);

                internalLastUnitHit = col.transform;
                lastUnitHit_Viable = false;
                stagMovement.lastUnitHit = col.transform;
                stagMovement.IgnoreCollider(2f, col.transform); //så man inte collidar med den när man åker igenom, sätter högre tid på den nu, för den SKA ta bort ignoren när dashen slutar
                                                                //stagMovement.IgnoreCollider(true, col.transform);
                if (stagMovement.staggDashIE != null)
                {
                    stagMovement.StopCoroutine(stagMovement.staggDashIE);
                }
                stagMovement.staggDashIE = stagMovement.StaggDash(true, 0.024f, 0.3f);
                stagMovement.StartCoroutine(stagMovement.staggDashIE);
                //stagMovement.Dash(true, true); //använd kamera riktningen
                //Debug.Log("Felet med riktningen är att man kallar dash före stagger, gör så att de körs i rad");
                //stagMovement.Stagger(0.25f);
                stagMovement.AddJumpsAvaible(stagMovement.jumpAmount, stagMovement.jumpAmount);
                pm.AddPower(1, 80);
            }
            h.AddHealth(-2);
        }
    }

    //IEnumerator StagDash()
    //{

    //}

    public void Activate(bool force = false) //force gör så att man slipper checken
    {
        ready = true;
        active = true;
        ToggleColliders(true);
        ToggleRenderers(true);

        stagMovement.IgnoreLayer(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Unit"), true); //ignorera all kollision mellan spelarobjektet och alla fiender, så man inte studsar mot dem under dash
    }

    public void Disable()
    {
        if (initTimes == 0) return;
        if (fadeOut != null || renderers[0].enabled == false) return;

        fadeOut = FadeOut(0.5f);
        StartCoroutine(fadeOut);

        stagMovement.IgnoreLayer(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Unit"), false);
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

            if (renderers.Length != 0) //kan man inkludera particlesystem oxå om man vill fadea deras transparency!
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Color c = renderers[i].material.color;
                    renderers[i].material.color = new Color(c.r, c.g, c.b, currAlpha);
                }
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
