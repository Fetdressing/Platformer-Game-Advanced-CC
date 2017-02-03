using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimationPlayer : BaseClass {
    public Animation aHandler;
    public AnimationClip[] aClips;
	// Use this for initialization
	void Start () {
        Init();
    }

    public override void Init()
    {
        base.Init();
        if (aHandler == null)
        {
            aHandler = GetComponent<Animation>();
        }
    }

    public void PlayAnimation(int animID)
    {
        aHandler.CrossFade(aClips[animID].name);
    }
}
