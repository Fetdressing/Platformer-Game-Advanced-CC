using UnityEngine;
using System.Collections;

public class PowerCharger : BaseClass { //DENNA BÖR HA PLAYERONLY LAYER
    private Transform thisTransform;
    private Collider[] thisColliders;
    private PowerManager pM; //powermanager på playern, hämtas i ontrigger och manipuleras via update
    private CameraShaker cameraShaker;

    public float phaseTime = 0.0f; //används vid automatisk toggle, set till 0 ifall man inte vill ha
    public float phaseCooldown = 1.5f;
    public float startTime = 1.0f; //när hela börjar köras, kan behövas offset för att få dem ur fas

    public float decayPowerMultiplayer = 1.0f; //hur mycket power den ska ta/ge förhållande till decayraten på karaktären
    public float maxPowerPercentage = 60;

    private ParticleSystem ps;
    private Light lightActive;
    private AudioSource audioSource;
    public AudioClip activeAudio;

    //ett particlesystem på den som de påverkar oxå, eller linerenderer
    private bool isEffectActive = false;
    private LineRenderer playerBeam;
    private Transform player;

    private float effectDurTime = 1.0f;
    private float effectTimer = 0.0f;

    [Header("Animation")]
    public AnimStandardPlayer animationPlayer;
    public AnimationClip activeAnim;
    public float animationSpeed = 1.0f;
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        thisTransform = this.transform;
        thisTransform.gameObject.layer = LayerMask.NameToLayer("PlayerOnly");

        thisColliders = thisTransform.GetComponentsInChildren<Collider>();
        ps = thisTransform.GetComponent<ParticleSystem>();
        lightActive = thisTransform.GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();

        cameraShaker = GameObject.FindGameObjectWithTag("Manager").GetComponent<CameraManager>().cameraPlayerFollow.GetComponent<CameraShaker>();

        try
        {
            player = GameObject.FindGameObjectWithTag("Player").gameObject.transform;
        }
        catch
        {
            Debug.Log("No player-Object");
        }
        playerBeam = thisTransform.GetComponent<LineRenderer>();

        Reset();
        initTimes++;
    }

    public override void Reset()
    {
        base.Reset();
        StopAllCoroutines();
        effectTimer = 0.0f;
        if (phaseTime != 0)
        {
            StartCoroutine(KillZoneLifetime());
        }
        else
        {
            ToggleKillZone(true);
        }
    }

    void LateUpdate()
    {
        isEffectActive = IsEffectValid();

        if (playerBeam != null)
        {
            if (isEffectActive)
            {
                playerBeam.enabled = true;
                Vector3[] positions = { transform.position, player.position };
                playerBeam.SetPositions(positions);
            }
            else
            {
                playerBeam.enabled = false;
            }
        }

        if (isEffectActive)
        {
            if (animationPlayer != null)
            {
                animationPlayer.PlayAnimation(activeAnim, 1.0f, animationSpeed);
            }

            if(activeAudio != null && audioSource.isPlaying == false)
            {
                audioSource.PlayOneShot(activeAudio);
            }

            float powerDecay = -(pM.powerDecay * decayPowerMultiplayer * Time.deltaTime);
            if(powerDecay < 0)
            {
                cameraShaker.ShakeCamera(0.2f, 1, true);
            }
            pM.AddPower(powerDecay, maxPowerPercentage);
        }
        else
        {
            if (activeAudio != null && audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }

            pM = null;
            effectTimer = 0.0f;
        }

    }

    IEnumerator KillZoneLifetime()
    {
        yield return new WaitForSeconds(startTime);
        while (this != null)
        {
            yield return new WaitForSeconds(phaseCooldown);
            ToggleKillZone();
            yield return new WaitForSeconds(phaseTime);
            ToggleKillZone();
        }
    }

    public void ToggleKillZone()
    {
        if (initTimes == 0) return;
        bool b = thisColliders[0].enabled;

        if (b == false)
        {
            ps.Simulate(0.0f, true, true);
            ParticleSystem.EmissionModule psemit = ps.emission;
            psemit.enabled = true;
            ps.Play();

            if(lightActive != null)
            {
                lightActive.enabled = true;
            }
        }
        else
        {
            ps.Stop();
            if (lightActive != null)
            {
                lightActive.enabled = false;
            }
        }

        for (int i = 0; i < thisColliders.Length; i++)
        {
            thisColliders[i].enabled = !b;
        }
    }

    public void ToggleKillZone(bool b)
    {
        if (initTimes == 0) return;
        if (b == false)
        {
            ps.Simulate(0.0f, true, true);
            ParticleSystem.EmissionModule psemit = ps.emission;
            psemit.enabled = true;
            ps.Play();
            if (lightActive != null)
            {
                lightActive.enabled = true;
            }
        }
        else
        {
            ps.Stop();
            if (lightActive != null)
            {
                lightActive.enabled = false;
            }
        }

        for (int i = 0; i < thisColliders.Length; i++)
        {
            thisColliders[i].enabled = !b;
        }
    }

    void OnTriggerStay(Collider col)
    {
        pM = col.GetComponent<PowerManager>();
        if(pM == null)
        {
            Debug.Log("Ett objekt har player-layer som ej ska ha?");
        }
        //if (col.tag != "Player") return;

        effectTimer = effectDurTime + Time.time;
    }

    public bool IsEffectValid()
    {
        if (pM == null) return false;

        if (pM.gameObject.activeSelf == false || pM.isAlive == false)
        {
            return false;
        }

        if (effectTimer > Time.time) return true;

        return false;
    }

}
