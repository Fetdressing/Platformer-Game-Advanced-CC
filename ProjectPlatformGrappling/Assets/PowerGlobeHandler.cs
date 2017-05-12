using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGlobeHandler : BaseClass
{

    public Transform m_playerTransform;
    private Collider m_playerCollider;
    private StagMovement m_stagMovement;
    private ScoreManager m_scoreManager;
    private PowerManager m_powerManager;

    public Collider m_colliderSmall;
    public Collider m_colliderLarge;

    [System.Serializable]
    public class PowerGlobeParticleSystems
    {
        public string name = "Power Globe System";
        public ParticleSystem ps;
        public bool isMain = false;
        public bool followAndEmit = false;
        public bool follow = false;
        public bool onTriggerOnly = false;
        [HideInInspector]
        public float timer;
        [HideInInspector]
        public bool isAttracting = false;
        [HideInInspector]
        public bool isACtive = true;
    }
    public List<PowerGlobeParticleSystems> m_particleSystems;

    // TODO : Sound
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        if (m_playerTransform == null)
        {
            m_playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
        m_playerCollider = m_playerTransform.GetComponent<Collider>();
        try
        {
            m_scoreManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ScoreManager>();
        }
        catch
        {
            print("(PowerGlobeHandler) Scoremanager could not be found");
        }
        m_powerManager = m_playerTransform.GetComponent<PowerManager>();

        for (int i = 0; i < m_particleSystems.Count; i++)
        {
            if (m_particleSystems[i].ps == null) print("Particle system at (" + i + ") is unassigned");
            else
            {
                if(m_particleSystems[i].ps.trigger.enabled)
                {
                    ParticleSystem.TriggerModule tmodule = m_particleSystems[i].ps.trigger;
                    print(tmodule.GetCollider(0));
                    
                }
            }
        }

    }
    public override void Reset()
    {
        base.Reset();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < m_particleSystems.Count; i++)
        {
            PowerGlobeParticleSystems ps = m_particleSystems[i];
            //print(ps);
            if (ps.isACtive)
            {
                if (ps.isMain)
                {

                }
                if (ps.followAndEmit)
                {

                }
                if (ps.follow)
                {

                }
                if (ps.onTriggerOnly)
                {

                }
            }
        }
    }
}
