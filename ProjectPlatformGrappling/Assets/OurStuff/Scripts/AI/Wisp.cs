using UnityEngine;
using System.Collections;

public class Wisp : BaseClass {
    private Vector3 startPosition;
    private Vector3 currMovePos;
    public float movePosIntervalTime = 8;
    private float movePosIntervalTimer = 0.0f;

    public float speed = 4;
    public float checkDistanceThreshhold = 400;

    private bool fleeing = false;
    private Transform fleeingUnit;
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
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        fleeing = false;
        returning = false;
        movePosIntervalTimer = 0.0f;
    }
    // Update is called once per frame
    void Update () {
        if (fleeing)
        {
            Vector3 dir = (transform.position - fleeingUnit.position);
            transform.position = Vector3.Slerp(transform.position, dir, Time.deltaTime * speed * 6);
            if (Vector3.Distance(startPosition, transform.position) < 5)
            {
                fleeing = false;
            }
            else
            {
                return;
            }
        }

        if(movePosIntervalTimer < Time.time)
        {
            movePosIntervalTimer = movePosIntervalTime + Time.time;
            currMovePos = new Vector3(Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f), Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f), Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f));
        }

        if(Vector3.Distance(transform.position, startPosition) > checkDistanceThreshhold*3 && returning == false)
        {
            returning = true;
        }

        if(returning == true)
        {
            transform.position = Vector3.Slerp(transform.position, startPosition, Time.deltaTime * speed);
            if(Vector3.Distance(startPosition, transform.position) < 5)
            {
                returning = false;
            }
        }
        else
        {
            Vector3 dir = (transform.position - currMovePos).normalized;
            transform.position = Vector3.Slerp(transform.position, currMovePos, Time.deltaTime * speed);
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

    void OnTriggerEnter(Collider col)
    {
        if(col.tag != "Player") { return; }
        fleeingUnit = col.transform;
        fleeing = true;

    }
}
