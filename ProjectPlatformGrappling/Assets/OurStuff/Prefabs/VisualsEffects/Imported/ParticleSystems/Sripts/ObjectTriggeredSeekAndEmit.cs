using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTriggeredSeekAndEmit : BaseClass
{

    private ParticleSystem m_ps;
    private List<ParticleSystem.Particle> m_triggerParticles = new List<ParticleSystem.Particle>();
    private StagMovement m_stagMovement;
    private SphereCollider m_collider;
    private Transform m_psTransform;

    public bool m_shouldEmit = false;
    public ParticleSystem m_childSystem;

    public Transform m_playerTransform;
    private Collider m_playerCollider;

    public bool m_lerp = true;
    public float m_chaseSpeed = 500.0f;
    private float m_distance = 0.0f;

    public float m_force = 10.0f;
    private bool m_attract = false;
    private bool m_firstAttractFrame = true;
    private float m_attractTime = 0.0f;
    private bool m_isAlive = true;
    public int m_emitCount = 50;
    // Use this for initialization
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        m_collider = GetComponent<SphereCollider>();
        m_collider.isTrigger = true; // must be trigger

        m_psTransform = transform;
        m_ps = GetComponent<ParticleSystem>();

        m_firstAttractFrame = true;
        m_attractTime = 0.0f;
        if (m_playerTransform == null)
        {
            m_playerTransform = GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>().player;
        }
        m_playerCollider = m_playerTransform.GetComponent<Collider>();
    }
    public override void Reset()
    {
        base.Reset();
        gameObject.SetActive(true);
        m_firstAttractFrame = true;
        m_attract = false;
        m_attractTime = 0.0f;
        m_isAlive = true;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        m_attract = true;
        m_firstAttractFrame = true;
    }

    void OnParticleTrigger()
    {
        if (m_ps != null)
        {
            int particles = m_ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, m_triggerParticles);

            if (particles > 0)
            {
                m_ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

                m_childSystem.transform.position = m_psTransform.position;
                m_childSystem.Emit(m_emitCount);
                m_isAlive = false;
                Deactivate();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_attract && m_isAlive)
        {
            Attract();
        }
    }

    void Attract()
    {
        if (m_firstAttractFrame)
        {
            m_distance = Vector3.Distance(m_playerTransform.position, m_psTransform.position);

            m_firstAttractFrame = false;
        }
        if (m_lerp)
        {
            // float distanceToPlayer = Vector3.Distance(m_playerTransform.position, m_psTransform.position);
            // float traveledDistance = m_distance / m_distance - distanceToPlayer ;
            // print (m_distance + "    " + distanceToPlayer);
            // m_psTransform.position = Vector3.Lerp(m_psTransform.position, m_playerTransform.position, traveledDistance);

            float distanceToPlayer = Vector3.Distance(m_playerTransform.position, m_psTransform.position);
            //float traveledDistance = m_distance / m_distance - distanceToPlayer;
            //print(m_distance + "    " + distanceToPlayer);
            
            m_attractTime += Time.fixedDeltaTime;
            m_psTransform.position = Vector3.Lerp(m_psTransform.position, m_playerTransform.position, m_attractTime * 2.0f);
            //typ sinlerp
            //m_psTransform.position = Vector3.Lerp(m_psTransform.position, m_playerTransform.position, Mathf.Sin(Mathf.PI * m_attractTime)/2.0f);
        }
        else
        {
            Vector3 direction = Vector3.Normalize(m_playerTransform.position - m_psTransform.position);
            float ForceDeltaTime = m_force * Time.deltaTime;
            Vector3 velocity = direction * ForceDeltaTime;
            m_psTransform.position += velocity;
        }
    }

}
