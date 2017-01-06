using UnityEngine;
using System.Collections;

public class HoomingProjectile : ProjectileBase
{
    Transform seekTarget;
    public float checkRadius = 20;
    public float hoomingForce = 6000;
    public float maxVelocity = 120;

    public override void Init()
    {
        base.Init();
        if (initTimes > 0) return;
    }

    public override void Fire(float lifeTime, Vector3 forceDir)
    {
        if (initTimes == 0) return;
        base.Fire(lifeTime, forceDir);
        seekTarget = null;

        StartCoroutine(LifeTime(lifeTime));
        o_Rigidbody.AddForce(forceDir, ForceMode.Impulse);
    }

    public override void Reset()
    {
        base.Reset();
        seekTarget = null;
        StopAllCoroutines();
    }

    void Update()
    {
        if (gameObject.activeSelf == false) return;

        if(o_Rigidbody.velocity.magnitude > maxVelocity)
        {
            o_Rigidbody.velocity = o_Rigidbody.velocity.normalized* maxVelocity;
        }

        if (seekTarget == null)
        {
            Collider[] potTargets = Physics.OverlapSphere(transform.position, checkRadius, targetsLM);

            if (potTargets.Length > 0)
            {
                
                for(int i = 0; i < 10; i++) //gör 10 försök
                {
                    int rP = Random.Range(0, potTargets.Length);
                    HealthSpirit hS = potTargets[rP].GetComponent<HealthSpirit>();
                    if(potTargets[rP].gameObject.activeSelf == true)
                    {
                        if (hS != null && hS.GetCurrHealth() > 0)
                        {
                            seekTarget = potTargets[rP].transform;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            transform.LookAt(seekTarget.position);
            o_Rigidbody.AddForce(transform.forward * hoomingForce, ForceMode.Force);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        for (int i = 0; i < noGoTags.Length; i++)
        {
            if (col.tag == noGoTags[i])
                return;
        }

        if (shooter != null)
        {
            if (col.transform == shooter) return; //inte träffa sig själv
        }
        for (int i = 0; i < damageTags.Length; i++)
        {
            if (col.tag == damageTags[i])
            {
                if (col.tag == "Player") //spelaren dör på ett skott
                {
                    col.GetComponent<PowerManager>().Die();
                }
                else
                {
                    HealthSpirit hSp = col.GetComponent<HealthSpirit>();
                    if (hSp != null)
                    {
                        hSp.AddHealth(-1);
                    }
                }
                PlayParticleSystem();
                break;
            }
        }


        if (!isGhost)
        {
            for (int i = 0; i < healTags.Length; i++)
            {
                if (col.tag == healTags[i])
                {
                    return; //ta inte sönder den om det är en egen
                }
            }
            Die();
        }
    }
}
