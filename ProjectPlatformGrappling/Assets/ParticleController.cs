using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ParticleSettings
{
    public bool enabled = false;
    public float startMagnitude;
    public float stopMagnitude;
    public ParticleSystem pSystem;
}

public class ParticleController : BaseClass
{

    private StagMovement playerMovementObject;
    private List<ParticleSettings> velSystems = new List<ParticleSettings>();
    private List<ParticleSettings> stackSystems = new List<ParticleSettings>();

    [Header("Velocity Based")]
    public ParticleSettings[] velSystemsHolder = new ParticleSettings[3];

    public ParticleSettings[] stackSystemsHolder = new ParticleSettings[3];

    [Space(10)]
    public Text velocityText;

    private int velocitySystemCount = 0;

    void Start()
    {
        playerMovementObject = GameObject.FindGameObjectWithTag("Player").GetComponent<StagMovement>();
        Debug.Log(velocityText);
        Init();

    }

    public override void Init()
    {
        base.Init();
        for (int i = 0; i < velSystemsHolder.Length; i++)
        {
            if (velSystemsHolder[i].pSystem != null && velSystemsHolder[i].enabled)
            {
                velSystemsHolder[i].pSystem.Stop();
                velSystems.Add(velSystemsHolder[i]);
                velocitySystemCount++;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        float magnitude = playerMovementObject.currMomentum.magnitude;
        float dashMagnitude = playerMovementObject.dashVel.magnitude;
        if (dashMagnitude > magnitude) magnitude = dashMagnitude;
        UpdateGUI(magnitude);

        if(magnitude < velSystems[0].startMagnitude)
        {
            velSystems[0].pSystem.Stop();
        }
        else
        {
            velSystems[0].pSystem.Simulate(0.0f);
            velSystems[0].pSystem.Play();
        }
    }

    void StartParticleSystem(ParticleSystem p_system)
    {

    }

    void UpdateGUI(float magnitude)
    {
        if(velocityText != null) velocityText.text = magnitude.ToString("F1");
    }
}
