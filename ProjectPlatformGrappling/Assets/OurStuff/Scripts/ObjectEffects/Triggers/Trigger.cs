using UnityEngine;
using System.Collections;

public class Trigger : BaseClass {
    [HideInInspector]
    public bool isTriggered;
    public bool continous = false; //om true så kallar den kommandot hela tiden
    public float collisionExtent = 5;
    public LayerMask collisionMask;

    public ParticleSystem psActivated;

    public FunctionEvent fEventStart;
    public FunctionEvent fEventExit;

    private float startVolume;
    private AudioSource audioSource;
    public AudioClip audioActive;
    public AudioClip audioDeactive;
    // Use this for initialization
    void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        isTriggered = true; //är viktig så att ToggleTrigger inte fuckar med sina if-satser
        ToggleTrigger(false);
        initTimes++;
        audioSource = transform.GetComponent<AudioSource>();
        if(audioSource != null)
            startVolume = audioSource.volume;

        Reset();
        //psActivated = this.transform.GetComponent<ParticleSystem>();
    }

    public override void Reset()
    {
        base.Reset();
        ExitTrigger();
    }

    //void OnTriggerEnter(Collider col)
    //{
    //    ToggleTrigger(true);
    //}

    //void OnTriggerExit(Collider col)
    //{
    //    //kolla ifall det står någon kvar
    //    ToggleTrigger(false);
    //}

    //void OnCollisionEnter(Collision col)
    //{
    //    ToggleTrigger(true);
    //}

    //void OnCollisionExit(Collision col)
    //{
    //    ToggleTrigger(false);
    //}

    void FixedUpdate()
    {
        if (initTimes == 0) return;

        ToggleTrigger(GetTriggered());
    }

    public bool GetTriggered()
    {
        Collider[] col = Physics.OverlapBox(transform.position, new Vector3(collisionExtent, collisionExtent, collisionExtent), Quaternion.identity, collisionMask);
        if(col.Length > 0)
        {
            return true;
        }
        return false;
    }

    public void ToggleTrigger(bool b)
    {
        if (b)
        {
            if (isTriggered != b)
            {
                if (psActivated != null)
                {
                    psActivated.Simulate(0.0f, true, true);
                    ParticleSystem.EmissionModule psemit = psActivated.emission;
                    psemit.enabled = true;
                    psActivated.Play();
                }
                StartTrigger();
                
            }
            else if(continous)
            {
                StartTrigger();
            }
            isTriggered = true;
        }
        else
        {
            if (isTriggered != b)
            {
                ExitTrigger();
            }
            if (psActivated != null)
            {
                psActivated.Stop();
            }                
            isTriggered = false;
            
        }
    }

    public virtual void StartTrigger()
    {
        if(audioSource != null)
        {
            if(audioActive != null)
            {
                //audioSource.clip = audioActive;
                //audioSource.Play();
                StopAllCoroutines();
                StartCoroutine(FadeInClip(audioActive));
            }
        }
        fEventStart.Invoke();
    }

    public virtual void ExitTrigger()
    {
        if (audioSource != null)
        {
            if (audioDeactive != null)
            {
                //audioSource.clip = audioDeactive;
                //audioSource.Play();
                StopAllCoroutines();
                StartCoroutine(FadeInClip(audioDeactive));
            }
        }
            fEventExit.Invoke();
    }

    IEnumerator FadeInClip(AudioClip ac)
    {
        while (!AudioFadeOut())
            yield return new WaitForSeconds(0.01f);
        audioSource.clip = ac;
        audioSource.Play();
        while (!AudioFadeIn())
            yield return new WaitForSeconds(0.01f);
    }

    bool AudioFadeIn()
    {
        if (audioSource.volume < startVolume)
        {
            audioSource.volume += 0.4f * Time.deltaTime;
            return false;
        }
        else
        {
            return true;
        }
    }

    bool AudioFadeOut()
    {
        if (audioSource.volume > 0.002)
        {
            audioSource.volume -= 0.4f * Time.deltaTime;
            return false;
        }
        else
        {
            return true;
        }
    }
}
