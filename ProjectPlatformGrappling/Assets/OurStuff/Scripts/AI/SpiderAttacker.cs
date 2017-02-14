using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAttacker : BaseClass {
    WallMovement movementH;
    private Vector3 startPosition;
    private Vector3 currMovePos;
    public float movePosIntervalTime = 8;
    private float movePosIntervalTimer = 0.0f;

    public float speed = 150;
    public float chaseSpeed = 200;
    public float checkDistanceThreshhold = 20;

    public Transform[] patrolPoints; //kan lämnas tom för random movement
    protected int currPatrolPointID = 0;

    public float playerChaseDistance = 100;

    private bool chasing = false;
    private Transform player;
    private bool returning = false;

    public Animation animH;
    public AnimationClip idle;
    public AnimationClip run;
    public float idleSpeed = 1.0f;
    public float runSpeed = 1.5f;

    float currSpeed = 0.0f;
    Vector3 lastFramePos;
    // Use this for initialization
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        float randomAddedSpeed = Random.Range(-speed * 0.4f, speed * 0.4f); //för att få lite variation
        speed += randomAddedSpeed;
        chaseSpeed += randomAddedSpeed;

        movementH = GetComponent<WallMovement>();

        startPosition = transform.position;
        currMovePos = transform.position;

        if (animH != null)
        {
            animH[idle.name].speed = idleSpeed + randomAddedSpeed * 0.003f;
            animH[run.name].speed = runSpeed + randomAddedSpeed * 0.003f;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        chasing = false;
        returning = false;
        movePosIntervalTimer = 0.0f;

    }
    // Update is called once per frame
    void Update()
    {
        if (!movementH.jumping)
        {
            float randomr = Random.Range(0, 500);

            if(randomr < 10)
                movementH.Jump(75);
        }

        Vector3 dir = transform.forward;
        float distanceToMovePos = Vector3.Distance(transform.position, currMovePos);

        if (patrolPoints.Length > 0) //man har patrolpoints
        {
            if (distanceToMovePos < 5) //nästa patrolposition
            {
                currPatrolPointID += 1;
                if (currPatrolPointID >= patrolPoints.Length)
                    currPatrolPointID = 0;

                currMovePos = patrolPoints[currPatrolPointID].position;
            }

            dir = (currMovePos - transform.position).normalized;
            movementH.Move(dir, Time.deltaTime * speed);
            
        }
        else
        {
            //utan patrolpoints
            float playerDistance = Vector3.Distance(player.position, transform.position);
            if (playerDistance < playerChaseDistance)
            {
                chasing = true;
            }
            else
            {
                chasing = false;
            }


            if (chasing)
            {

                dir = (player.position - transform.position).normalized;
                movementH.Move(dir, Time.deltaTime * chaseSpeed);
                if (Vector3.Distance(startPosition, transform.position) < 5)
                {
                    chasing = false;
                }

            }
            else
            {
                if (movePosIntervalTimer < Time.time)
                {
                    movePosIntervalTimer = movePosIntervalTime + Time.time;
                    currMovePos = GetRandomVector() + transform.position;
                }

                if (Vector3.Distance(transform.position, startPosition) > checkDistanceThreshhold * 3 && returning == false) //för långt ifrån start positionen, hem igen!
                {
                    returning = true;
                    currMovePos = GetRandomVector() + transform.position;
                }
                else if (Vector3.Distance(transform.position, currMovePos) < 5) //nått målet! nästa patrolposition
                {
                    currMovePos = GetRandomVector() + transform.position;
                }

                if (returning == true)
                {
                    dir = (startPosition - transform.position).normalized;
                    movementH.Move(dir, Time.deltaTime * speed);
                    if (Vector3.Distance(startPosition, transform.position) < 5)
                    {
                        returning = false;
                    }
                }
                else
                {
                    dir = (currMovePos - transform.position).normalized;
                    movementH.Move(dir, Time.deltaTime * speed);
                }

            }
        }

        currSpeed = (lastFramePos - transform.position).magnitude;
        lastFramePos = transform.position;
        if(animH != null)
        {

            if(currSpeed > 0.1f)
            {
                animH.CrossFade(run.name);
            }
            else
            {
                animH.CrossFade(idle.name);
            }
        }
    }

    public Vector3 GetRandomVector()
    {
        Vector3 p = new Vector3(Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f), Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f), Random.Range(-checkDistanceThreshhold * 0.5f, checkDistanceThreshhold * 0.5f));
        return p;
    }
}
