using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePowerGlobeHandler : BaseClass
{

    public float m_powerValue = 0.1f;
    public int m_globeValue = 1;

    public Transform m_playerTransform;
    private Collider m_playerCollider;
    private StagMovement m_stagMovement;
    private ScoreManager m_scoreManager;
    private PowerManager m_powerManager;
    private SpawnManager m_spawnManager;

    private Vector3 m_startPosition;
    private Rigidbody m_rigidBody;
    private List<ParticleSystem> m_particleSystems;

    private bool hasBeenPicked = false;
    private bool isAlive = true;
    
    // Use this for initialization
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        m_spawnManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>();
        m_playerTransform = m_spawnManager.player;
        m_powerManager = m_playerTransform.GetComponent<PowerManager>();
        m_stagMovement = m_playerTransform.GetComponent<StagMovement>();
        try
        {
            m_scoreManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ScoreManager>();
        }
        catch
        {
            print("Scoremanager not found");
        }

        m_startPosition = transform.position;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
    }

    private void SpawnGlobe()
    {
        transform.position = m_startPosition;
        gameObject.SetActive(true); // Vet inte om man måste reaktivera partikelsystemen också?
        isAlive = true;
    }

    private void CollectGlobe()
    {
        if(m_powerManager != null)
        {
            m_powerManager.AddPower(m_powerValue);
            if (!hasBeenPicked) m_scoreManager.PowerGlobeCollected(m_globeValue);
            hasBeenPicked = true;

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
