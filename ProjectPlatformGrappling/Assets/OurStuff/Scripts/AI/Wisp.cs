using UnityEngine;
using System.Collections;

public class Wisp : BaseClass {
    private Transform player;
    private Vector3 startPosition;
    private Vector3 currMovePos;
    public float movePosIntervalTime = 8;
    private float movePosIntervalTimer = 0.0f;

    public float speed = 4;
    public float checkDistanceThreshhold = 400;
    
    public float fleeDistance = 25; //hur långt den ska fly
    private bool returning = false;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        startPosition = transform.position;
        currMovePos = transform.position;
        player = GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>().player;

        speed *= 0.001f;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        returning = false;
        movePosIntervalTimer = 0.0f;
    }
    // Update is called once per frame
    void Update () {
        print("en random rotation varför gång de förflyttar sig kanske?");
        if (Vector3.Distance(player.position, transform.position) < fleeDistance)
        {
            Vector3 fleePos = transform.position + (transform.position - player.position).normalized * fleeDistance * 1.3f;
            transform.position = Vector3.Lerp(transform.position, fleePos, Time.time * speed);

            return;
        }

        if(movePosIntervalTimer < Time.time)
        {
            movePosIntervalTimer = movePosIntervalTime + Time.time;
            currMovePos = GetRandomVector() + transform.position;
        }

        if(returning == false && Vector3.Distance(transform.position, startPosition) > checkDistanceThreshhold*3)
        {
            returning = true;
        }

        if(returning == true)
        {
            transform.position = Vector3.Lerp(transform.position, startPosition, Time.time * speed * 0.01f);
            if(Vector3.Distance(startPosition, transform.position) < 5)
            {
                returning = false;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, currMovePos, Time.time * speed);
        }
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

    public Vector3 GetRandomVector()
    {
        Vector3 p = new Vector3(Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f), Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f), Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f));
        return p;
    }
}
