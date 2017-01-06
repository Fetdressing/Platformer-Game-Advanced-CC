using UnityEngine;
using System.Collections;
using System;

public class BossBase : BaseClass {
    [System.Serializable]
    public class BossEvent
    {
        public float delay; //när man byter event så tar det denna tid
        public string methodName;
    }

    [System.Serializable]
    public class BossState //boss state innehåller events, ett state kan ändras vid en viss HP mängd tex tex
    {
        [HideInInspector]
        public int currEventIndex = 0;
        public BossEvent[] bossEvent;
    }

    protected int currStateIndex = 0;
    public BossState[] bossState;

    // Use this for initialization
    void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        RunNextState();
    }

    protected void RunNextEvent() //kalla denna för att dra igång nytt event på det nuvarande statet
    {
        StartCoroutine(NextEvent(bossState[currStateIndex]));
    }

    public virtual IEnumerator NextEvent(BossState currBossState)
    {
        int currEventI = currBossState.currEventIndex;
        BossEvent currBossEvent = currBossState.bossEvent[currEventI];

        yield return new WaitForSeconds(currBossEvent.delay);

        this.SendMessage(currBossEvent.methodName); //förlitar mig sedan på att denna function/coorutine kallar vidare när den är klar, annars stoppar hela kedjan

        if (currStateIndex+1 < bossState.Length)
        {
            currBossState.currEventIndex++;
        } 
        else
        {
            currBossState.currEventIndex = 0; //börjar om från första eventet, går att overrida NextEvent() så att man tex stannar på sista eventet
        }
    }

    public virtual void RunNextState()
    {
        RunNextEvent(); //sätter igång kedjan av event i nuvarande state, kanske konstig syntax men så det funkar
        if (currStateIndex + 1 < bossState.Length) //overrida RunNextState() om man tex istället vill att den ska börja om alla states när den gått igenom listan
        {
            currStateIndex++; //nästa gång denna funktionen kallas så kommer nästa state att användas
        }
    }

    public void TestFunc()
    {
        StartCoroutine(TestCall());
    }

    IEnumerator TestCall()
    {
        //gör nånting
        yield return new WaitForSeconds(5);
        //Debug.Log(Time.time.ToString());
        RunNextEvent();
    }
}
