using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Conversation : MonoBehaviour {
    public bool isRepetable = false;
    public float repeatInterval = 20;
    private float repeatIntervalTimer = 0.0f;
    private bool isPlaying = false;
    private int nrPlays = 0; //hur många gånger den spelats

    public float startConversationDelay = 1.0f;
    
    public CharacterEvent[] characterEvents;

    public void BeginConversation()
    {
        if (repeatIntervalTimer < Time.time)
        {
            repeatIntervalTimer = Time.time + repeatInterval;
            StartCoroutine(PlayConversation());
        }
    }

    IEnumerator PlayConversation()
    {
        if (isPlaying == true) yield break;

        if (nrPlays > 0 && isRepetable == false) yield break;
        nrPlays++;
        isPlaying = true;
        for (int i = 0; i < characterEvents.Length; i++)
        {
            CharacterEvent cEvent = characterEvents[i];
            StartCoroutine(EventDelay(cEvent));

            if (cEvent.audioLine != null)
            {
                cEvent.audioSource.PlayOneShot(cEvent.audioLine);
            }

            if(cEvent.animation != null)
            {
                cEvent.animator.CrossFade(cEvent.animation.ToString(), 0.5f);
            }

            yield return new WaitForSeconds(characterEvents[i].lineTime);
        }
        isPlaying = false;
    }

    IEnumerator EventDelay(CharacterEvent cEvent)
    {
        yield return new WaitForSeconds(cEvent.eventDelayTime);
        cEvent.OnEvent.Invoke();
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Player")
        {
            BeginConversation();
        }
    }
}

[System.Serializable]
public class FunctionEvent : UnityEvent { }

[System.Serializable]
public class CharacterEvent //denne visar ju inte sina medlemsvariabler!!
{
    public float lineTime = 2.0f;
    public AudioSource audioSource;
    public Animator animator;

    public AudioClip audioLine;
    public Animation animation;

    public float eventDelayTime = 2.0f;
    public FunctionEvent OnEvent; //funktion som ska kallas
}
