using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Trigger : BaseClass {
    public bool once = false; //om den är true så körs bara "StartTrigger" en gång
    [HideInInspector]
    public bool isTriggered;
    public bool continous = false; //om true så kallar den kommandot hela tiden
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

    void FixedUpdate()
    {
        if (initTimes == 0) return;

        ToggleTrigger(GetTriggered());
    }

    public bool GetTriggered()
    {
        Vector3 extents = GetComponent<BoxCollider>().size;
        Collider[] col = Physics.OverlapBox(transform.position, extents, transform.rotation, collisionMask);
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
            else if (continous) //maybe?
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
        if(once)
        {
            Destroy(this, 3);
        }
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
