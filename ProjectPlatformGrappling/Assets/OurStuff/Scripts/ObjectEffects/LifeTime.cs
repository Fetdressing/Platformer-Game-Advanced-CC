using UnityEngine;
using System.Collections;

public class LifeTime : MonoBehaviour {
    public bool destroy = false;
    public float lifeTime = 20;

    IEnumerator lifeIE;

    public GameObject deathPar;

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

        if (deathPar != null)
        {
            GameObject temp = Instantiate(deathPar.gameObject);
            temp.transform.position = transform.position;
            Destroy(temp.gameObject, 3);
        }

        lifeIE = null;
    }
}
