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
    private List<ParticleSystem.Particle> m_triggeredParticles = new List<ParticleSystem.Particle>();
    private ParticleSystem m_ps;

    private List<ParticleSystem> m_siblingSystems;

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
        // Setup particle systems
        m_ps = GetComponent<ParticleSystem>();
        m_startPosition = transform.position;
        GetSiblingSystems();

        m_spawnManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>();
        if (m_playerCollider == null)
        {
            m_playerTransform = m_spawnManager.player;
        }
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


        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        SpawnGlobe(m_ps);
        for (int i = 0; i < m_siblingSystems.Count; i++)
        {
            SpawnGlobe(m_siblingSystems[i]);
        }
    }

    private void SpawnGlobe(ParticleSystem ps)
    {
        ps.transform.position = m_startPosition;
        ps.gameObject.SetActive(true); // Vet inte om man måste reaktivera partikelsystemen också?

        if(ps.GetComponent<ObjectTriggeredSeekAndEmit>()) ps.GetComponent<ObjectTriggeredSeekAndEmit>().Reset();
        if(ps.GetComponent<ParticleSeekTriggered>()) ps.GetComponent<ParticleSeekTriggered>().Reset();

        isAlive = true;
    }

    private void CollectGlobe()
    {
        if (m_powerManager != null)
        {
            m_powerManager.AddPower(m_powerValue);
            if (!hasBeenPicked)
            {
                m_scoreManager.PowerGlobeCollected(m_globeValue);
                //print("YAYA");
            }
            hasBeenPicked = true;   // Not used at the moment, but decides whether this has been picked or not. Will not count towards
                                    // number of collected globes if true and might slightly change the visuals
        }
    }

    void OnParticleTrigger()
    {
        // Collect globe, add power.
        if (m_ps != null)
        {
            int triggeredParticleCount = m_ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, m_triggeredParticles);
            if (triggeredParticleCount > 0)
            {
                CollectGlobe();
            }
        }
    }

    void GetSiblingSystems()
    {
        int childSystemCount = m_ps.transform.parent.childCount;
        m_siblingSystems = new List<ParticleSystem>();
        for (int i = 0; i < childSystemCount; i++)
        {
            ParticleSystem ps = m_ps.transform.parent.GetChild(i).GetComponent<ParticleSystem>();
            if (ps != m_ps && ps != null)
            {
                m_siblingSystems.Add(ps);
            }
        }
    }
}
