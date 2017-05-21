using UnityEngine;
using System.Collections;

public class Wisp : BaseClass {
    private Transform player;
    private Rigidbody o_rigidbody;
    private Vector3 startPosition;
    private Vector3 currMovePos;
    private Vector3 currRotation;
    public float movePosIntervalTime = 8;
    private float movePosIntervalTimer = 0.0f;

    public float speed = 4;
    public float checkDistanceThreshhold = 400;
    
    public float fleeDistance = 25; //hur långt den ska fly
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        o_rigidbody = GetComponent<Rigidbody>();
        startPosition = transform.position;
        currMovePos = transform.position;
        player = GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>().player;

        speed *= 0.001f;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        movePosIntervalTimer = 0.0f;
    }
    // Update is called once per frame
    void Update () {

        bool movePosSet = false; //kolla ifall man redan fått en position
        if (Vector3.Distance(player.position, transform.position) < fleeDistance)
        {
            currMovePos = transform.position + (transform.position - player.position).normalized * fleeDistance * 1.3f;
            movePosSet = true;
            //transform.position = Vector3.Lerp(transform.position, fleePos, Time.time * speed);

        }
        else if(movePosIntervalTimer < Time.time)
        {
            currMovePos = GetNewMovePos();
            currRotation = GetRandomRotation();
            movePosSet = true;
        }

        if( Vector3.Distance(transform.position, startPosition) > checkDistanceThreshhold*3 && movePosIntervalTimer < Time.time && movePosSet == false)
        {
            currMovePos = GetPosHome();
            currRotation = GetRandomRotation();
        }

        o_rigidbody.MovePosition(Vector3.Lerp(transform.position, currMovePos, Time.time * speed));
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, currRotation, Time.time * speed);
	}

    //IEnumerator Flee(Transform t)
    //{
    //    fleeing = true;
    //    while(Vector3.Distance(t.position, transform.position) < 50)
    //    {
    //        Vector3 dir = (transform.position - t.position).normalized;

    //        transform.position = Vector3.Slerp(transform.position, (transform.position + dir * 20), Time.deltaTime * speed);
    //        yield return new WaitForSeconds(0.01f);
    //    }
    //    fleeing = false;
    //}

    //IEnumerator Return()
    //{
    //    returning = true;
    //    while (Vector3.Distance(startPosition, transform.position) < 5)
    //    {
    //        transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * speed);
    //        yield return new WaitForSeconds(0.01f);
    //    }
    //    returning = false;
    //}

    public Vector3 GetNewMovePos()
    {
        movePosIntervalTimer = movePosIntervalTime + Random.Range(-movePosIntervalTime * 0.1f, movePosIntervalTime * 0.1f) + Time.time;
        Vector3 movePos = GetRandomVector() + transform.position;

        int tries = 0; int maxTries = 5;
        float newPosDistanceToPlayer = Vector3.Distance(movePos, player.position);
        while (newPosDistanceToPlayer < fleeDistance * 1.3f && tries < maxTries) //åk inte för nära spelaren
        {
            movePos = GetRandomVector() + transform.position;
            newPosDistanceToPlayer = Vector3.Distance(movePos, player.position);
            tries++;
        }

        return movePos;
    }

    public Vector3 GetPosHome()
    {
        movePosIntervalTimer = movePosIntervalTime + Random.Range(-movePosIntervalTime * 0.1f, movePosIntervalTime * 0.1f) + Time.time;
        int tries = 0; int maxTries = 5;

        Vector3 homePos = GetRandomVector() + transform.position;
        float newPosDistanceToHome = Vector3.Distance(homePos, startPosition);
        float currdistanceHome = Vector3.Distance(transform.position, startPosition);
        while (newPosDistanceToHome > currdistanceHome && tries < maxTries) //åk inte för nära spelaren
        {
            homePos = GetRandomVector() + transform.position;
            newPosDistanceToHome = Vector3.Distance(homePos, startPosition);
            tries++;
        }

        if(newPosDistanceToHome > currdistanceHome)
        {
            return transform.position;
        }

        return homePos;
    }

    public Vector3 GetRandomVector()
    {
        Vector3 p = new Vector3(Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f), Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f), Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f));
        return p;
    }

    public Vector3 GetRandomRotation()
    {
        Vector3 q = new Vector3(0, Random.Range(0, 360), 0);
        return q;
    }
}
