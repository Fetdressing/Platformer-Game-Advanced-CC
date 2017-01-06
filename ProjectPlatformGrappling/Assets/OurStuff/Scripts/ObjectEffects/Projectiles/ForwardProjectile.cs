using UnityEngine;
using System.Collections;

public class ForwardProjectile : ProjectileBase {


    public override void Init()
    {
        base.Init();
        if (initTimes > 0) return;
    }

    public override void Fire(float lifeTime, Vector3 forceDir)
    {
        if (initTimes == 0) return;
        base.Fire(lifeTime, forceDir);

        StartCoroutine(LifeTime(lifeTime));
        o_Rigidbody.AddForce(forceDir, ForceMode.Impulse);
    }

    public override void Reset()
    {
        base.Reset();
        StopAllCoroutines();
    }

    void OnTriggerEnter(Collider col)
    {
        for(int i = 0; i < noGoTags.Length; i++)
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
            Die();
        }
    }
}
