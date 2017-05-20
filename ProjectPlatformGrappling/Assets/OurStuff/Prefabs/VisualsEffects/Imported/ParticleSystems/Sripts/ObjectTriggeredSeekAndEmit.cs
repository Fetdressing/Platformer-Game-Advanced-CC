using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTriggeredSeekAndEmit : BaseClass
{

    private ParticleSystem m_ps;
    private List<ParticleSystem.Particle> m_triggerParticles = new List<ParticleSystem.Particle>();
    private SphereCollider m_collider;
    private Transform m_psTransform;

    public bool m_shouldEmit = false;
    public ParticleSystem m_childSystem;

    public Transform m_playerTransform;
    private Collider m_playerCollider;

    public float m_force = 10.0f;
    private bool m_attract = false;
    private bool m_isAlive = true;
    public int m_emitCount = 50;
    // Use this for initialization
    void Start()
    {
        m_collider = GetComponent<SphereCollider>();
        m_collider.isTrigger = true; // must be trigger

        m_psTransform = transform;
        m_ps = GetComponent<ParticleSystem>();

        m_playerCollider = m_playerTransform.GetComponent<Collider>();

    }

    public override void Reset()
    {
        base.Reset();
        gameObject.SetActive(true);
    }

    public override void Deactivate()
    {
        base.Deactivate();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        m_attract = true;
    }

    void OnParticleTrigger()
    {
        if(m_ps != null)
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
    void Update()
    {
        if(m_attract && m_isAlive)
        {
            Attract();
        }
    }

    void Attract()
    {
        Vector3 direction = Vector3.Normalize(m_playerTransform.position - m_psTransform.position);
        float ForceDeltaTime = m_force * Time.deltaTime;
        Vector3 velocity = direction * ForceDeltaTime;
        m_psTransform.position += velocity;
    }
}
