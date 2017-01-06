using UnityEngine;
using System.Collections;

public class AIBase : BaseRagdoll {
    [HideInInspector]
    public Transform thisTransform;
    [HideInInspector]
    public Health thisHealth;

    public Animation animationH;

    public Transform agentTransform;
    [HideInInspector]
    public NavMeshAgent agent;

    [HideInInspector]
    public Transform target;

    [HideInInspector]
    public float currMoveForce = 0;
    //look angle threshhold, hur mycket den måste titta på fienden för att kunna attackera
    [HideInInspector]
    public float lookAngleThreshhold = 15;
    public float turnRatio = 2;

    public float[] agentTransformDistanceThreshhold = { 12, 20 }; //threshhold på hur nära och hur långt ifrån agenten ska befinna sig, min värdet används mest för att kolla ifall transformen behöver röra på sig
    public float agentAllowedTimeFromTransform = 5; //hur länge agenten får vara ifrån transformen, så att den inte ska fastna
    [HideInInspector]
    public float timePointAgentToFar = 0.0f; //när agenten kom för långt ifrån, tidpunkten då det hände, används för o kolla ifall agenten behöver åka tillbaks till transformen

    [HideInInspector]
    public int currSetDestinationID = 0;

    public override void Init()
    {
        base.Init();
        thisTransform = this.transform;
        thisHealth = thisTransform.GetComponent<Health>();
        agent = agentTransform.GetComponent<NavMeshAgent>();
        //animationH = thisTransform.GetComponent<Animation>();
        initTimes++;

    }

    public override void Reset()
    {
        base.Reset();
        ToggleRagdoll(false);
        isGrounded = false;
        ReturnAgent();
    }

    //***agent förflyttning***
    public virtual void SetDestination(Vector3 pos)
    {
        if (!IsReadyToMove()) return;
        agent.SetDestination(pos);
    }

    public virtual void UpdateAgentStatus()
    {
        if (!IsReadyToMove()) return;

        if (agentTransformDistanceThreshhold[1] < Vector3.Distance(thisTransform.position, agentTransform.position))
        {
            if ((Time.time - timePointAgentToFar) > agentAllowedTimeFromTransform) //ifall den stått still förlänge, returnera den då
            {
                timePointAgentToFar = Time.time;
                ReturnAgent();
            }
            agent.Stop();
        }
        else
        {
            timePointAgentToFar = Time.time;
            agent.Resume();
        }
    }

    public void ResetPath() //så att den inte fuckar när den är av, mer safe
    {
        if (IsReadyToMove())
        {
            agent.ResetPath();
            agent.Resume();
            ReturnAgent();
        }
    }

    public void ReturnAgent() //förflyttar den till sin startposition
    {
        if (!IsReadyToMove()) return;
        
        NavMeshHit nhit;
        if (NavMesh.SamplePosition(thisTransform.position, out nhit, 5.0f, NavMesh.AllAreas))
        {
            //Debug.Log(nhit.position.ToString() + " " + thisTransform.position.ToString());
            agent.Warp(thisTransform.position);
        }
    }

    public bool GetRandomNavmeshDestination(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
    //***agent förflyttning***

    //***thistransform förflyttning***
    public virtual void MoveTowardsDestination(Vector3 pos, float moveForce)
    {
        UpdateAgentStatus();
        if (!IsReadyToMove()) return;
        RotateTowards(pos);
        //if (IsTransformCloseEnoughToAgent())
        //{
        //    thisRigidbody.velocity = thisRigidbody.velocity * 0.9f; //break
        //    return;
        //}
        //thisRigidbody.velocity = thisRigidbody.velocity + thisTransform.forward * moveForce * Time.deltaTime;
        //thisRigidbody.AddForce(thisTransform.forward * moveForce * Time.deltaTime);
        AddForceSlowDrag(thisTransform.forward * moveForce * Time.deltaTime, ForceMode.Force, thisRigidbody);
    }

    public virtual void RotateTowards(Vector3 t) //får overridas om för flygande units
    {
        Vector3 tPosWithoutY = new Vector3(t.x, thisTransform.position.y, t.z); //så den bara kollar på x o z leden
        Vector3 direction = (tPosWithoutY - thisTransform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, lookRotation, Time.deltaTime * turnRatio);
    }
    //***thistransform förflyttning***

    public virtual bool IsTransformCloseEnoughToAgent()
    {
        if(Vector3.Distance(thisTransform.position, agentTransform.position) < agentTransformDistanceThreshhold[0])
        {
            return true;
        }
        return false;
    }

    public virtual bool IsIdle()
    {
        if(agent.hasPath == false || agent.isPathStale == true || Vector3.Distance(agent.pathEndPosition, thisTransform.position) < 1.5f || agent.remainingDistance < 2.0f)
        {
            return true;
        }
        return false;
    }
   
    public virtual bool IsReadyToMove()
    {
        if (agent == null) return false;
        if (thisTransform.gameObject.activeSelf == false) return false;
        if (agent.isOnNavMesh == false) return false;
        if (agent.enabled == false) return false;
        if (isGrounded == false) return false;

        return true;
    }

    public bool IsFacingTransform(Transform t)
    {
        Vector3 tPosWithoutY = new Vector3(t.position.x, thisTransform.position.y, t.position.z); //så den bara kollar på x o z leden
        Vector3 vecToTransform = tPosWithoutY - thisTransform.position;
        float angle = Vector3.Angle(thisTransform.forward, vecToTransform);

        if (angle > lookAngleThreshhold)
        {
            //Debug.Log(angle.ToString());
            return false;
        }
        return true;
    }

    public override bool ToggleRagdoll(bool b)
    {
        if (base.ToggleRagdoll(b)) //om den är på cd så vill man inte göra nått med animations mojset
        {
            if (animationH != null)
            {
                animationH.enabled = !b;
            }
            return true;
        }
        return false;
    }

    //***Alerts***
    public virtual void ReportAttacked(Transform t)
    {

    }
    //***Alerts***

    public virtual void PlayStateAnimations()
    {

    }

    //public bool HasReachedPosition(Vector3 pos) //krävs att man har en path
    //{
    //    agent.SetDestination(pos);
    //    if (GetDistanceToPosition(pos) < 1.5f)
    //    {
    //        return true;
    //    }
    //    if (agent.pathPending) //så agent.remainingDistance funkar (den är inte klar med o beräkna så vänta lite)
    //    {
    //        return false;
    //    }
    //    if (agent.remainingDistance < 1.5f)
    //    {
    //        return true;
    //    }
    //    return false;
    //}
}
