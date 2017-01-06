using UnityEngine;
using System.Collections;

public class MagneticBall : MagneticForce
{
    [HideInInspector]
    public Transform homeTransform;
    [HideInInspector]
    public Vector3 homePos;
    [HideInInspector]
    public Vector3 startScale;

    [HideInInspector]
    public MagneticBallState magneticBallState = MagneticBallState.HeadingHome;

    private Transform player;

    public float playerForce = 24000;

    private float cooldownTime = 2.0f;
    private float cooldownTimer = 0.0f;
    public Material cooldownMat;

    private float cooldownTimeCallback = 0.2f;
    private float cooldownTimerCallback = 0.0f;


    //cosmetic
    [HideInInspector]
    public static Color pushColorS = Color.red;
    [HideInInspector]
    public static Color pullColorS = Color.blue;
    [HideInInspector]
    public Color normalColor;

    public Material pushHoloMat;
    public Material pullHoloMat;
    [HideInInspector]
    public static Material pushHoloFMat;
    [HideInInspector]
    public static Material pullHoloFMat;
    [HideInInspector]
    public Material normalMat; //det materialet som används

    [HideInInspector]
    public Transform holoRangeTransform;
    [HideInInspector]
    public ParticleSystem ps;
    [HideInInspector]
    public Light pLight;
    [HideInInspector]
    public TrailRenderer tRenderer;
    [HideInInspector]
    public Renderer thisRenderer;

    [HideInInspector]
    public LineRenderer lineRendererBindPlayer;
	// Use this for initialization
	void Start () {
        Init();
    }

    public override void Init()
    {
        base.Init();
        holoRangeTransform = transform.GetComponentsInChildren<Transform>()[1];
        holoRangeTransform.localScale = new Vector3(range * (1 / transform.localScale.x), range * (1 / transform.localScale.x), range * (1 / transform.localScale.x));

        pushHoloFMat = pushHoloMat;
        pullHoloFMat = pullHoloMat;

        ps = transform.GetComponent<ParticleSystem>();
        pLight = transform.GetComponent<Light>();
        tRenderer = transform.GetComponent<TrailRenderer>();
        thisRenderer = transform.GetComponent<MeshRenderer>();

        switch (forceType)
        {
            case ForceType.Push:
                SetCurrColor(pushColorS);
                SetCurrMaterial(pushHoloFMat);
                break;
            case ForceType.Pull:
                SetCurrColor(pullColorS);
                SetCurrMaterial(pullHoloFMat);
                break;
        }
        normalMat = tRenderer.material;
        normalColor = pLight.color;


        thisRigidbody = transform.GetComponent<Rigidbody>();
        thisRigidbody.isKinematic = false;
        lineRendererBindPlayer = transform.GetComponent<LineRenderer>();
        SetState(MagneticBallState.HeadingHome);
        cooldownTimer = 0.0f;
        startScale = transform.lossyScale;
    }

    public override void Reset()
    {
        base.Reset();
        transform.SetParent(null);
        transform.localScale = startScale;

        StopAllCoroutines();
        cooldownTimer = cooldownTime + Time.time;
        SetState(MagneticBallState.HeadingHome);
    }

    public void SetStartTransform(Transform t) //absolete
    {
        homeTransform = t;
        initTimes++;
    }

    public void SetStartPosition(Vector3 pos)
    {
        homePos = pos;
        initTimes++;
    }

    public void SetPlayer(Transform p)
    {
        player = p;
    }

    // Update is called once per frame
    void Update () {
        UpdateLoop();
    }

    public override void UpdateLoop()
    {
        base.UpdateLoop();
        if (initTimes == 0) return;
        switch (magneticBallState)
        {
            case MagneticBallState.HeadingHome:
                HeadHome();
                break;
            case MagneticBallState.HeadingToTarget:
                //ApplyForce();
                break;
            case MagneticBallState.ApplyingGravity:
                //ApplyForce();
                break;
        }

        if (cooldownTimer > Time.time && magneticBallState == MagneticBallState.HeadingHome) //cooldown, ska bara visas när bollen är hemma o idlar
        {
            SetCurrColor(Color.yellow);
            SetCurrMaterial(cooldownMat);
        }
        else
        {
            SetCurrColor(normalColor);
            SetCurrMaterial(normalMat);
        }

        Vector3[] positionArray = new[] { transform.position, player.position };
        lineRendererBindPlayer.SetPositions(positionArray);
    }

    public override void FixedUpdate()
    {
        FixedUpdateLoop();
    }

    public override void FixedUpdateLoop()
    {
        //ingenting, för tydligen körs Updates via arv
        lineRendererBindPlayer.enabled = false;
        if(Input.GetKey(KeyCode.F))
        {
            if (magneticBallState != MagneticBallState.HeadingHome)
            {
                lineRendererBindPlayer.enabled = true;
                ApplyForceTarget(player, playerForce);
            }
        }
    }

    public void SetState(MagneticBallState bS)
    {
        magneticBallState = bS;
        switch (magneticBallState)
        {
            case MagneticBallState.HeadingHome:
                holoRangeTransform.gameObject.SetActive(false);
                ps.enableEmission = false;
                pLight.enabled = false;
                thisRigidbody.isKinematic = true;
                break;
            case MagneticBallState.HeadingToTarget:
                holoRangeTransform.gameObject.SetActive(false);
                ps.enableEmission = false;
                pLight.enabled = true;
                thisRigidbody.isKinematic = false; //forces ska applyas
                break;
            case MagneticBallState.ApplyingGravity:
                holoRangeTransform.gameObject.SetActive(true);
                ps.enableEmission = true;
                pLight.enabled = true;
                thisRigidbody.isKinematic = true;
                break;
        }
    }

    void HeadHome()
    {
        transform.position = Vector3.Slerp(transform.position, homePos, Time.deltaTime * 20f);
    }

    void OnTriggerEnter(Collider col)
    {
        if (magneticBallState == MagneticBallState.HeadingToTarget)
        {
            transform.SetParent(col.transform, true);
            SetState(MagneticBallState.ApplyingGravity);
        }
        return;
        if(magneticBallState == MagneticBallState.ApplyingGravity)
        {
            if(col.gameObject.tag == "MagneticBall") //en annan kula
            {
                MagneticBall mForce = col.gameObject.GetComponent<MagneticBall>();
                if(mForce.magneticBallState == MagneticBallState.HeadingToTarget)
                {
                    //slå ihop dem
                }
            }
        }
    }

    public void OrderHeadHome()
    {
        if (cooldownTimerCallback > Time.time) return;
        transform.SetParent(null);
        transform.localScale = startScale;

        StopAllCoroutines();
        cooldownTimer = cooldownTime + Time.time;
        SetState(MagneticBallState.HeadingHome);
    }
    public void OrderFire(float force, float stayTime)
    {
        if (cooldownTimer > Time.time) return;
        transform.SetParent(null);
        transform.localScale = startScale;

        cooldownTimerCallback = cooldownTimeCallback + Time.time;
        StopAllCoroutines();
        SetState(MagneticBallState.HeadingToTarget);
        thisRigidbody.AddForce(thisRigidbody.transform.forward * force, ForceMode.Impulse);
        StartCoroutine(FlyTimeChecker(stayTime));
    }

    IEnumerator FlyTimeChecker(float time)
    {
        yield return new WaitForSeconds(time);
        if(magneticBallState == MagneticBallState.HeadingToTarget)
        {
            OrderHeadHome();
        }
    }



    public void SetCurrMaterial(Material m)
    {
        holoRangeTransform.GetComponent<Renderer>().material = m;
        //thisRenderer.material = m;
        //renderer.material = pushHoloFMat; //vänta lite med dessa
        tRenderer.material = m;
    }

    public void SetCurrColor(Color c)
    {
        ps.startColor = c;
        pLight.color = c;
    }

}
public enum MagneticBallState { HeadingHome, HeadingToTarget, ApplyingGravity };
