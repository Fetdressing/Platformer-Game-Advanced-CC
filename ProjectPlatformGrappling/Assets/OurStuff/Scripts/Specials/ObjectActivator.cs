using UnityEngine;
using System.Collections;

public class ObjectActivator : BaseClass {
    private Transform player;
    private bool hasEntered = false;

    public bool activateCursor = false;
    public GameObject[] activate_GameObjects;
    public AudioClip sound;
    private AudioSource audioS;
    public float range = 70;

    public int repeatTimes = 0; //om den är mindre än 0 så händer det bara en gång
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        audioS = GetComponent<AudioSource>();
        isLocked = false;

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        hasEntered = false;
    }

    void Update()
    {
        if(Vector3.Distance(player.position, transform.position) < range)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    void Enable()
    {
        if (isLocked) return;
        if (hasEntered == true) return;

        hasEntered = true;

        if (activateCursor)
        {
            Cursor.visible = true;
        }

        for(int i = 0; i < activate_GameObjects.Length; i++)
        {
            activate_GameObjects[i].SetActive(true);
        }

        if (audioS != null && sound != null)
        {
            if (audioS.isPlaying == false)
            {
                audioS.PlayOneShot(sound);
            }
        }
    }

    void Disable()
    {
        if (hasEntered)
        {
            if (activateCursor)
            {
                Cursor.visible = false;
            }

            for (int i = 0; i < activate_GameObjects.Length; i++)
            {
                activate_GameObjects[i].SetActive(false);
            }

            if (repeatTimes < 0)
            {
                isLocked = true;
            }
            else
            {
                repeatTimes++;
            }

            hasEntered = false;
        }
    }

}
