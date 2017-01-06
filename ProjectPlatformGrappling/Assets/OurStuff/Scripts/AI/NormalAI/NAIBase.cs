using UnityEngine;
using System.Collections;

public class NAIBase : BaseClass {
    [System.Serializable]
    public class Anim
    {
        Anim(AnimationClip aC, float aSpeed)
        {
            aClip = aC;
            speed = aSpeed = 1;
        }

        public AnimationClip aClip;
        public float speed;

        public void Play(Animation a, bool crossFade)
        {
            if(crossFade)
            {
                a[aClip.name].speed = speed;
                a.CrossFade(aClip.name, 0.1f);
            }
            else
            {
                a[aClip.name].speed = speed;
                a.Play(aClip.name);
            }
        }
    }

    public Renderer thisRenderer;
    protected HealthSpirit healthSpirit; //alla kan ju ha en, men markera dem som odödliga annars? :>

    // Use this for initialization
    public Animation animHandler;
    public Anim detectedAnim;
    public Anim idleAnim;
    public Anim runAnim;
    public Anim[] attackAnim;

    protected bool runningAnim = false; //spelas en viktig animation?

    protected Transform player;
    public float maxChaseRange = 150;
    public float detectRange = 130;
    public float attackDistance = 10;

    protected Vector3 lastFramePos = Vector3.zero;
    protected float movingSpeed = 0.0f; //hur snabbt man rör sig

    void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        healthSpirit = GetComponent<HealthSpirit>();

        if (thisRenderer == null)
        {
            thisRenderer = GetComponent<Renderer>();
        }

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        StopAllCoroutines();
        StartCoroutine(Idle());
    }

    void Update()
    {
        movingSpeed = Vector3.Distance(transform.position, lastFramePos) * Time.deltaTime;
        lastFramePos = transform.position;
    }

    public virtual void Move(Vector3 pos)
    {
        
    }

    public virtual IEnumerator ChasePlayer()
    {
        while (Vector3.Distance(player.position, transform.position) < maxChaseRange)
        {
            if (Vector3.Distance(player.position, transform.position) < attackDistance)
            {
                StartCoroutine(Attack());
                break;
            }

            if (movingSpeed > 0.001f)
            {
                StartCoroutine(PlayAnimation(runAnim, false));
            }
            else
            {
                StartCoroutine(PlayAnimation(idleAnim, false));
            }
            Move(player.position);

            yield return new WaitForEndOfFrame();
        }
    }

    public virtual IEnumerator Attack()
    {
        StartCoroutine(PlayAnimation(attackAnim[Random.Range(0, attackAnim.Length)], true));
        while (runningAnim) //vänta på att animation är klar
        {
            yield return new WaitForEndOfFrame();
        }

        //börja jaga spelaren
        StartCoroutine(ChasePlayer());
    }

    public virtual IEnumerator Idle()
    {
        while(Vector3.Distance(player.position, transform.position) > detectRange)
        {
            StartCoroutine(PlayAnimation(idleAnim, false));
            yield return new WaitForSeconds(0.2f);
        }

        StartCoroutine(Detected());
    }

    public virtual IEnumerator Detected()
    {
        StartCoroutine(PlayAnimation(detectedAnim, true));
        while(runningAnim) //vänta på att animation är klar
        {
            yield return new WaitForEndOfFrame();
        }

        //börja jaga spelaren
        StartCoroutine(ChasePlayer());
    }


    public IEnumerator PlayAnimation(Anim aClip, bool important) //wait for return
    {
        if (important)
        {
            runningAnim = true;
            aClip.Play(animHandler, true);

            while (animHandler.isPlaying)
            {
                //Debug.Log(animHandler[aClip.aClip.name].time.ToString() + " / " + animHandler[aClip.aClip.name].length.ToString());
                yield return null;
            }
            runningAnim = false;
        }
        else
        {
            aClip.Play(animHandler, true);
        }
    }

    public virtual IEnumerator LookAt(Vector3 tPos)
    {
        Vector3 modPos = new Vector3(tPos.x, 0, tPos.z);
        Vector3 modTPos = new Vector3(transform.position.x, 0, transform.position.z);

        Vector3 dir = (modPos - modTPos); //vill inte den ska röra sig upp o ned genom dessa vektorer
        dir = dir.normalized;

        Quaternion lookRotation = Quaternion.LookRotation(dir);

        float turnRatio = 5;

        while (!IsLookingAlong(dir, 10)) //magnitude så att den inte ska försöka titta på en när denne är ovanför
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnRatio);
            yield return new WaitForEndOfFrame();
        }
    }

    public bool IsFacing(Vector3 pos, float angle)
    {
        Vector3 modPos = pos - transform.position;

        if (Vector3.Angle(modPos, transform.forward) < angle)
        {            
            return true;
        }
        return false;
    }

    public bool IsLookingAlong(Vector3 dir, float angle)
    {
        if (Vector3.Angle(dir, transform.forward) < angle)
        {
            return true;
        }
        return false;
    }
}
