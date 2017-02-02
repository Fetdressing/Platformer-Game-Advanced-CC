using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]
public class ParticlePlexus : MonoBehaviour
{

    public float m_maxDistance = 1.0f;
    public int m_maxConnections = 5;
    public int m_maxLRs = 100;


    new ParticleSystem m_particleSystem;
    ParticleSystem.Particle[] m_particles;


    ParticleSystem.MainModule m_mainModule;

    public LineRenderer m_lineRendererTemplate;
    List<LineRenderer> m_lineRenderers = new List<LineRenderer>();

    Transform _transform;
    // Use this for initialization
    void Start()
    {
        m_particleSystem = GetComponent<ParticleSystem>();
        m_mainModule = m_particleSystem.main;
        _transform = transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        int maxParticles = m_mainModule.maxParticles;

        if(m_particles == null || m_particles.Length < maxParticles)
        {
            m_particles = new ParticleSystem.Particle[maxParticles];
        }
        int lrIndex = 0;
        int lineRendererCount = m_lineRenderers.Count;

        if (lineRendererCount > m_maxLRs)
        {
            for (int i = m_maxLRs; i < lineRendererCount; i++)
            {
                Destroy(m_lineRenderers[i].gameObject);
            }

            int removedCount = lineRendererCount - m_maxLRs;
            m_lineRenderers.RemoveRange(m_maxLRs, removedCount);
            lineRendererCount -= removedCount;
        }

        if (m_maxConnections > 0 && m_maxLRs > 0)
        {
            m_particleSystem.GetParticles(m_particles);
            int particleCount = m_particleSystem.particleCount;

            float maxDistanceSqr = m_maxDistance * m_maxDistance;

            switch (m_mainModule.simulationSpace)
            {
                case ParticleSystemSimulationSpace.Local:
                    {
                        _transform = transform;
                        m_lineRendererTemplate.useWorldSpace = false;
                        break;
                    }
                case ParticleSystemSimulationSpace.Custom:
                    {
                        _transform = m_mainModule.customSimulationSpace;
                        m_lineRendererTemplate.useWorldSpace = false;
                        break;
                    }
                case ParticleSystemSimulationSpace.World:
                    {
                        _transform = transform;
                        m_lineRendererTemplate.useWorldSpace = true;
                        break;
                    }
                default:
                    {
                        throw new System.NotSupportedException(
                            string.Format("Unsupported simulation space '{0}'",
                            System.Enum.GetName(typeof(ParticleSystemSimulationSpace), m_mainModule.simulationSpace)));
                    }
            }

            for (int i = 0; i < particleCount; i++)
            {
                if(lrIndex == m_maxLRs)
                {
                    break;
                }
                Vector3 p1_position = m_particles[i].position;
                int connections = 0;
                for (int j = i + 1; j < particleCount; j++)
                {
                    Vector3 p2_position = m_particles[j].position;
                    float distanceSqr = Vector3.SqrMagnitude(p1_position - p2_position);

                    if (distanceSqr <= maxDistanceSqr)
                    {
                        LineRenderer lr;
                        if (lrIndex == lineRendererCount)
                        {
                            lr = Instantiate(m_lineRendererTemplate, _transform, false);
                            m_lineRenderers.Add(lr);
                            lineRendererCount++;
                        }
                        lr = m_lineRenderers[lrIndex];
                        lr.enabled = true;

                        lr.SetPosition(0, p1_position);
                        lr.SetPosition(1, p2_position);
                        lrIndex++;
                        connections++;
                        if(connections == m_maxConnections || lrIndex == m_maxLRs)
                        {
                            break;
                        }
                    }
                }
            }
            
        }
        for (int i = lrIndex; i < lineRendererCount; i++)
        {
            m_lineRenderers[i].enabled = false;
        }
    }
}
