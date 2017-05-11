using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseClass : MonoBehaviour
{
    protected int initTimes = 0;
    [System.NonSerialized]
    public bool isLocked;
    protected bool bActivated; //för att kolla med deactivate funktionen
    public virtual void Init()
    {
        //initTimes++; //jag vill kanske använda den som jag själv vill i varje class
    }
    public virtual void Dealloc() { }
    public virtual void Reset() { }
    public virtual void UpdateLoop() { }
    public virtual void Deactivate() { }
    [System.NonSerialized]
    public static float maxDeltaTime = 0.09f; //annars kan man få skumma värden om en frame varar för länge
    [System.NonSerialized]
    public static float deltaTime = 0.01f; //räknas ut sen i GameManager, den får ett startvärde så den inte ska vara null just
    [System.NonSerialized]
    public static float deltaTime_Unscaled = 0.01f;
    public static float lastFixedUpdate_Timepoint = 0.0f;

    public static float ingame_Realtime = 0.0f; //håller tiden, utan Time.timescale (unless spelet är pausat, moddas i GameManager)

    public class Pointer<T>
    {
        public T value;
    }

    public virtual void NewLevel() { }
}