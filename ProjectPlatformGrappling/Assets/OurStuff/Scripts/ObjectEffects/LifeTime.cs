using UnityEngine;
using System.Collections;

public class LifeTime : MonoBehaviour {
    public bool destroy = false;
    public float lifeTime = 20;

    IEnumerator lifeIE;

    void Awake()
    {
        if (lifeIE != null)
        {
            StopCoroutine(lifeIE);
        }

        lifeIE = Life();
        StartCoroutine(lifeIE);
    }

    void OnEnable()
    {
        if(lifeIE != null)
        {
            StopCoroutine(lifeIE);
        }

        lifeIE = Life();
        StartCoroutine(lifeIE);
    }

    IEnumerator Life()
    {
        yield return new WaitForSeconds(lifeTime);

        if(destroy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
        lifeIE = null;
    }
}
