using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class AINavmesh : AIMoveable {
    protected NavMeshAgent agent;

    protected NavMeshPath currPath;
    protected float standStillTimePoint = 0.0f; //för att kolla ifall den stått still för länge, dvs den klarar ej pathen
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        patrolState = new PatrolState(); //dessa behöver vara innan base.init, för där kallas reset
        chaseState = new ChaseState();
        guardPState = new GuardPState();

        base.Init();

        agent = GetComponent<NavMeshAgent>();
    }

    public override void Reset()
    {
        base.Reset();
        newPatrolTimer = 0.0f;
        statePattern.ChangeState(guardPState);
    }

    public override void Move(Vector3 pos, float speed)
    {
        agent.Resume();
        agent.speed = speed;
        agent.SetDestination(pos);
    }

    public override void Stop()
    {
        base.Stop();
        agent.Stop();
    }

    public override bool GetPatrolPoint(ref Vector3 pos) //returnerar vector3.zero ifall den ej klarar
    {
        if (agent.pathPending) return false;
        if (newPatrolTimer > Time.time) return false;
        newPatrolTimer = Time.time + newPartrolCD;

        Vector3 tempPatrol = Vector3.zero;
        for (int i = 0; i < 30; i++)
        {

            if (patrolPoints.Length == 0)
            {
                //helt random
                tempPatrol = startPosition + Random.insideUnitSphere * randomPatrol_MaxDistance;
                NavMeshHit navHit;
                if(NavMesh.Raycast(tempPatrol + Vector3.up * 150, tempPatrol + Vector3.down * 200, out navHit, NavMesh.AllAreas))
                {
                    tempPatrol = navHit.position;
                }
                //tempPatrol = new Vector3(startPosition.x + x, startPosition.y, startPosition.z + z);
            }
            else
            {
                currPatrolPointIndex = NextIndex(patrolPoints.Length, currPatrolPointIndex);
                tempPatrol = patrolPoints[currPatrolPointIndex].position;
            }
                        
            NavMeshHit hitH;
            if (NavMesh.SamplePosition(tempPatrol, out hitH, 2.0f, NavMesh.AllAreas)) //rangen är bara runt den positionen man redan valt
            {
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(hitH.position, path); //kolla ifall det är en legit path

                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    pos = hitH.position;
                    return true;
                }
                else
                {
                    //Debug.Log(transform.name + " " + path.status.ToString());
                }
            }
        }

        return false;
    }

    public override bool IsValidMove(Vector3 pos)
    {
        if (agent.pathPending) return true;
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(pos, path);
        if (path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }
        if (agent.isPathStale)
        {
            return false;
        }

        return true;
    }


    public override void RotateTowards(Vector3 pos)
    {
        return;
    }

    public override bool HasReached(Vector3 reacher, Vector3 reachPos, float distanceMargin)
    {
        if(agent.isPathStale || agent.remainingDistance < distanceMargin || Vector3.Distance(new Vector3(reachPos.x, reacher.y, reachPos.z), reacher) < distanceMargin)
        {
            return true;
        }
        return false;
    }

    public override bool HasReached(Vector3 reacher, Vector3 reachPos, float distanceMargin, bool useY)
    {
        if (agent.isPathStale || agent.remainingDistance < distanceMargin || Vector3.Distance(reachPos, reacher) < distanceMargin)
        {
            return true;
        }
        return false;
    }
}
