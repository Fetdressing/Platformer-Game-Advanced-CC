using UnityEngine;
using System.Collections;

public class AnimStandardPlayer : BaseClass { //hämtar speeden på denna transformen
    public Animation animationH;

    public AnimationClip walk;
    public AnimationClip run;
    private AnimationClip currMoveClip;

    public AnimationClip idle;

    public float walkASpeed = 1.0f;
    public float runASpeed = 1.5f;
    public float idleASpeed = 1.0f;

    private Vector3 lastFramePos = Vector3.zero;
    private Vector3 currFramePos = Vector3.zero;

    public float runSpeedThreshhold = 50;
    // Use this for initialization
    void Start () {
        Init();
    }

    public override void Init()
    {
        base.Init();

        if (animationH == null)
            animationH = transform.GetComponent<Animation>();

        animationH[walk.name].speed = walkASpeed;
        animationH[run.name].speed = runASpeed;
        animationH[idle.name].speed = idleASpeed;

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        currMoveClip = walk;
    }

    // Update is called once per frame
    void Update () {
        currFramePos = transform.position;
        float speed = (currFramePos - lastFramePos).magnitude / Time.deltaTime;
        
        lastFramePos = transform.position;

        if(speed > runSpeedThreshhold)
        {
            currMoveClip = run;
        }
        else
        {
            currMoveClip = walk;
        }

        if(speed < 0.1f)
        {
            animationH.CrossFade(idle.name);
        }
        else
        {
            animationH.CrossFade(currMoveClip.name);
        }
	}

    public void PlayAnimation(AnimationClip aClip, float weight, float animSpeed)
    {
        animationH[aClip.name].speed = animSpeed;
        animationH[aClip.name].layer = 1;
        animationH[aClip.name].weight = weight;
        animationH.CrossFade(aClip.name, 0.1f);
    }
}
