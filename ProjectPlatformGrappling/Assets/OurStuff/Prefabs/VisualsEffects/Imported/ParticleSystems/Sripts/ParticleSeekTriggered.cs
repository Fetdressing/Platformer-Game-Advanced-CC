using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSeekTriggered : MonoBehaviour
{
    // Public variables
    public Transform m_target;
    public float m_force = 10.0f;
    public bool m_stopOnTrigger = true;
    public float m_stopDelay = 0.0f;
    public bool m_startOnExit = true;

    // System
    private ParticleSystem m_particleSystem;
    private ParticleSystem.Particle[] m_particleArray;
    private ParticleSystem.MainModule m_psMainModule;

    private bool m_isAttracting = false;
    private int m_maxParticles = 50;
    // Use this for initialization
    void Start()
    {
        m_particleSystem = GetComponent<ParticleSystem>();
        m_psMainModule = m_particleSystem.main;
    }

    void OnTriggerEnter()
    {
        if (m_stopOnTrigger)
        {
            m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        if (m_particleSystem.isPlaying)
        {
            m_isAttracting = true;
        }
    }

    void OnTriggerExit()
    {
        if(!m_stopOnTrigger)
        {
            m_isAttracting = false;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (m_isAttracting)
        {
            if(m_particleSystem.particleCount > 0)
            {
                AttractParticles();
            }
            else if(m_startOnExit)
            {
                m_particleSystem.Play();
                m_isAttracting = false;
            }
            else
            {
                m_isAttracting = false;
            }
        }
    }

    void GetParticleArray(bool p_onTrigger)
    {
        m_maxParticles = m_particleSystem.particleCount;
        if (p_onTrigger || m_particleArray == null || m_particleArray.Length < m_maxParticles)
        {
            m_particleArray = new ParticleSystem.Particle[m_maxParticles];
        }
        m_particleSystem.GetParticles(m_particleArray);
    }

    void AttractParticles()
    {
        GetParticleArray(false);


        float forceDeltaTime = Time.deltaTime * m_force;

        Vector3 targetTransformPosition;
        switch (m_psMainModule.simulationSpace)
        {
            case ParticleSystemSimulationSpace.Local:
                {
                    targetTransformPosition = transform.InverseTransformPoint(m_target.position);
                    break;
                }
            case ParticleSystemSimulationSpace.Custom:
                {
                    targetTransformPosition = m_psMainModule.customSimulationSpace.InverseTransformPoint(m_target.position);
                    break;
                }
            case ParticleSystemSimulationSpace.World:
                {
                    targetTransformPosition = m_target.position;
                    break;
                }
            default:
                {
                    throw new System.NotSupportedException(
                        string.Format("Unsupported simulation space: '{0}'.",
                        System.Enum.GetName(typeof(ParticleSystemSimulationSpace), m_psMainModule.simulationSpace)));
                }


        }
        for (int i = 0; i < m_particleArray.Length; i++)
        {
            Vector3 directionTarget = Vector3.Normalize(targetTransformPosition - m_particleArray[i].position);
            Vector3 seekForce = directionTarget * forceDeltaTime;
            m_particleArray[i].velocity += seekForce;
        }
        m_particleSystem.SetParticles(m_particleArray, m_particleArray.Length);
        m_particleArray = new ParticleSystem.Particle[m_maxParticles];
    }
}
