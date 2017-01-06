using UnityEngine;
using System.Collections;

public class NAINavmesh : NAIBase {
    protected NavMeshAgent agent;

    public override void Init()
    {
        base.Init();
        agent = GetComponent<NavMeshAgent>();
    }

    public override void Move(Vector3 pos)
    {
        base.Move(pos);
        agent.SetDestination(pos);
    }

    //public override IEnumerator LookAt(Transform tPos)
    //{
    //    Vector3 modPos = new Vector3(tPos.position.x, 0, tPos.position.z);
    //    Vector3 modTPos = new Vector3(transform.position.x, 0, transform.position.z);

    //    Vector3 dir = (modPos - modTPos); //vill inte den ska röra sig upp o ned genom dessa vektorer
    //    dir = dir.normalized;

    //    agent.Resume();
    //    while (!IsFacing(modPos, 25)) //magnitude så att den inte ska försöka titta på en när denne är ovanför
    //    {
    //        agent.SetDestination(tPos.position);
    //        yield return new WaitForEndOfFrame();
    //    }
    //    agent.Stop();
    //}

    //public override IEnumerator LookAt(Vector3 tPos)
    //{
    //    agent.Stop();
    //    return base.LookAt(tPos);
    //}
}
