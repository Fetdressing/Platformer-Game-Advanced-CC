using UnityEngine;
using System.Collections;

public class AnimationSpeedModifier : MonoBehaviour {
    public float speedIncrease = 1.5f;
    // Use this for initialization
    void Start()
    {
        foreach (AnimationState state in GetComponent<Animation>())
        {
            state.speed = speedIncrease;
        }

    }

    void Awake()
    {
        foreach (AnimationState state in GetComponent<Animation>())
        {
            state.speed = speedIncrease;
        }

    }

}
