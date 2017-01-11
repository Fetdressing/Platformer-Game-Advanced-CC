using UnityEngine;
using System.Collections;

public class BaseUnit : BaseClass { //denna borde alla units ärva från
    [HideInInspector]
    public Vector3 startPos;


    public override void Init()
    {
        base.Init();
        startPos = transform.position;
    }

    public override void Reset()
    {
        base.Reset();
        transform.position = startPos;
    }
}
