using UnityEngine;
using System.Collections;

public class NAIInvisibleNavmesh : NAINavmesh {
    public Anim revealAnim;
    protected bool isInvisible = false;

    public override void Init()
    {
        base.Init();
    }

    public override void Move(Vector3 pos)
    {
        agent.Resume();
        base.Move(pos);
    }

    public override IEnumerator Attack() //kolla på fienden oxå!
    {
        agent.Stop();

        yield return StartCoroutine(LookAt(player.position));

        if (isInvisible)
        {
            StartCoroutine(ToggleInvisible(false));
            StartCoroutine(PlayAnimation(revealAnim, true));
            while (runningAnim) //vänta på att animation är klar
            {
                yield return new WaitForEndOfFrame();
            }
        }

        StartCoroutine(PlayAnimation(attackAnim[Random.Range(0, attackAnim.Length)], true));
        while (runningAnim) //vänta på att animation är klar
        {
            yield return new WaitForEndOfFrame();
        }
        
        //börja jaga spelaren
        StartCoroutine(ChasePlayer());
    }

    public override IEnumerator ChasePlayer()
    {
        if (Vector3.Distance(player.position, transform.position) > attackDistance)
        {
            StartCoroutine(ToggleInvisible(true));
        }

        return base.ChasePlayer();
    }

    public IEnumerator ToggleInvisible(bool b)
    {
        isInvisible = b;
        if(isInvisible)
        {
            while(healthSpirit.GetTransparency() > 0.0f)
            {
                float t = healthSpirit.GetTransparency() - Time.deltaTime * 10;
                healthSpirit.SetTransparency(t, false);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (healthSpirit.GetTransparency() < 1.0f)
            {
                float t = healthSpirit.GetTransparency() + Time.deltaTime * 10;
                healthSpirit.SetTransparency(t, false);
                yield return new WaitForEndOfFrame();
            }
        }
        
    }
}
