using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleVelocityTrails : BaseClass {

    // Components
    private StagMovement playerMovementObject;
    private ParticleSystem particleSystem;
    private ParticleSystem.TrailModule trailModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;

    public int ShowParticlesAtStep = -1; // Show the particles at this step. Disabled at -1

    [Header("Trail Variables")]
    public Vector2 baseCounts;  
    public Vector2 baseValues; // Trail base lifetimes (2 values randomized)
    public bool step1 = true;
    public int step1count = 5;
    public float step1StartMag = -1.0f;
    public Vector2 step1Values; // Trail step1 lifetimes (2 values randomized)
    public bool step2 = true;
    public int step2Count = 10;
    public float step2StartMag = -1.0f;
    public Vector2 step2Values;
    public bool step3 = false;
    public int step3Count = 15;
    public float step3StartMag = -1.0f;
    public Vector2 step3Values;
    public bool step4 = false;
    public int step4Count = 20;
    public float step4StartMag = -1.0f;
    public Vector2 step4Values;

    // Use this for initialization
    void Start () {
		playerMovementObject = GameObject.FindGameObjectWithTag("Player").GetComponent<StagMovement>();
        particleSystem = GetComponent<ParticleSystem>();
        trailModule = particleSystem.trails;
        emissionModule = particleSystem.emission;
        emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(1.0f, 10.0f);
        mainModule = particleSystem.main;
        mainModule.maxParticles = 10;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 velocity = playerMovementObject.currMomentum;
        float magnitude = velocity.magnitude;
        
        if(magnitude < step1StartMag)
        {

        }

	}
}
