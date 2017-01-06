using UnityEngine;
using System.Collections;

public class CameraShaker : BaseClass {
    private Camera t_Camera;
    private Vector3 originalPos;
    private bool smoothShake = false;
    private bool isShaking = false;

    private IEnumerator currCamerShakeIE;
    private IEnumerator currFOVChangeIE;

    private bool overrideShake; //sätts som argument

    private float startFOV;
    // Use this for initialization
    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        t_Camera = GetComponent<Camera>();
        startFOV = t_Camera.fieldOfView;
        originalPos = transform.localPosition;

        Reset();
        //StartCoroutine(Shake(2, 2));
    }

    public override void Reset()
    {
        base.Reset();
        isShaking = false;
        smoothShake = false;
        transform.localPosition = originalPos;
    }

    public void ShakeCamera(float duration, float magnitude, bool smooth)
    {
        if (isShaking == false)
        {
            smoothShake = smooth;
            overrideShake = false; //default på att den inte ska göra det
            transform.localPosition = originalPos;

            if (currCamerShakeIE != null)
            {
                StopCoroutine(currCamerShakeIE);
            }
            currCamerShakeIE = Shake(duration, magnitude);
            StartCoroutine(currCamerShakeIE);
        }
    }

    public void ShakeCamera(float duration, float magnitude, bool smooth, bool overrideCurrShake)
    {
        if (overrideCurrShake)
        {
            smoothShake = smooth;
            transform.localPosition = originalPos;
            overrideShake = overrideCurrShake;

            if (currCamerShakeIE != null)
            {
                StopCoroutine(currCamerShakeIE);
            }
            currCamerShakeIE = Shake(duration, magnitude);
            StartCoroutine(currCamerShakeIE);
        }
        else if(isShaking == false)
        {
            smoothShake = smooth;
            transform.localPosition = originalPos;
            overrideShake = overrideCurrShake;

            if (currCamerShakeIE != null)
            {
                StopCoroutine(currCamerShakeIE);
            }
            currCamerShakeIE = Shake(duration, magnitude);
            StartCoroutine(currCamerShakeIE);
        }
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        //if (isShaking)
        //{
        //    if (!overrideShake)
        //    {
        //        yield break;
        //    }
        //}
        isShaking = true;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {

            elapsed += Time.deltaTime;

            float percentComplete = elapsed / duration;
            float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

            // map value to [-1, 1]
            float x, y;

            if (smoothShake)
            {
                x = Mathf.PerlinNoise(Random.value, Random.value) * 2.0f - 1.0f;
                y = Mathf.PerlinNoise(Random.value, Random.value) * 2.0f - 1.0f;
            }
            else
            {
                x = Random.value * 2.0f - 1.0f;
                y = Random.value * 2.0f - 1.0f;
            }

            x *= magnitude * damper;
            y *= magnitude * damper;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            yield return null;
        }

        transform.localPosition = originalPos;
        overrideShake = false;
        isShaking = false;
    }

    public void ChangeFOV(float duration, float newFOV)
    {
        if (Mathf.Abs(t_Camera.fieldOfView - startFOV) > 0.1f) return; //då håller den redan på att ändra FOVen

        t_Camera.fieldOfView = startFOV;

        if (currFOVChangeIE != null)
        {
            StopCoroutine(currFOVChangeIE);
        }
        currFOVChangeIE = ApplyChangeFOV(duration, startFOV + newFOV);
        StartCoroutine(currFOVChangeIE);
    }

    IEnumerator ApplyChangeFOV(float duration, float newFOV)
    {
        float elapsed = 0.0f;
        
        float vel = 0;
        while (Mathf.Abs(t_Camera.fieldOfView - newFOV) > 0.1f)
        {
            elapsed += Time.deltaTime;

            float percentComplete = elapsed / duration;
            
            float change = Mathf.SmoothDamp(t_Camera.fieldOfView, newFOV, ref vel, duration);
            t_Camera.fieldOfView = change;

            yield return null;
        }

        vel = 0;
        while(Mathf.Abs(t_Camera.fieldOfView - startFOV) > 0.1f) //tillbaks igen
        {
            float change = Mathf.SmoothDamp(t_Camera.fieldOfView, startFOV, ref vel, duration * 0.5f);
            t_Camera.fieldOfView = change;
            yield return null;
        }

        t_Camera.fieldOfView = startFOV;
    }

}
