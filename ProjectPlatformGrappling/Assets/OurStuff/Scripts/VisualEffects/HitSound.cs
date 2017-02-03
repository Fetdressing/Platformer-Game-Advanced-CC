using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class HitSound : BaseClass {
    public RandomSound rSound;
    private AudioSource audioSource;
    public AudioClip audioClip;
    public float volume = 0.4f;

    public string[] dontRepeatTags = { "MovingPlatform"}; //grejer som bara ska kunna träffas en gång i streck
    private Transform lastHit;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        audioSource = GetComponent<AudioSource>();

        if(rSound == null)
        {
            rSound = GetComponent<RandomSound>();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        bool isRepeatTag = false;
        for(int i = 0; i < dontRepeatTags.Length; i++)
        {
            if(col.tag == dontRepeatTags[i])
            {
                isRepeatTag = true;
                break;
            }
        }

        if (isRepeatTag)
        {
            if (lastHit != col.transform)
            {
                if (rSound != null)
                {
                    rSound.Play();
                }
                else
                {
                    audioSource.PlayOneShot(audioClip, volume);
                }
            }
            lastHit = col.transform;
        }
        else
        {
            if (rSound != null)
            {
                rSound.Play();
            }
            else
            {
                audioSource.PlayOneShot(audioClip, volume);
            }
        }
    }
}
