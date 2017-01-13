using UnityEngine;
using System.Collections;

public class ForcePusher : BaseClass {
    public bool isContinuous = false;
    public bool pushAway = false; //om true så pushar denna ifrån sig själv oavsett vilket riktning objekten kommer ifrån
    public float pushForce = 100;

    public AnimStandardPlayer animPlayer;
    public AnimationClip pushAnim;
    public float animationSpeed = 1.0f;

    public AudioSource audioSource;
    private CameraShaker cameraShaker;

    public bool canBeBlocked = false;
    public float powerAddPercentage = 0.0f; //hur mkt i procent den kommer sno power

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        cameraShaker = GameObject.FindGameObjectWithTag("Manager").GetComponent<CameraManager>().cameraPlayerFollow.GetComponent<CameraShaker>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (isContinuous) return;
        if(col.tag == "Player")
        {
            StagMovement sM = col.GetComponent<StagMovement>();
            if (canBeBlocked)
            {
                if(sM.speedBreaker.active == true)
                {
                    return;
                }
            }

            if(powerAddPercentage > 0.001f || powerAddPercentage < -0.001f)
            {
                col.GetComponent<PowerManager>().AddPowerPercentage(powerAddPercentage, true);
            }

            cameraShaker.ShakeCamera(0.2f, 1, true);

            if(audioSource != null)
            {
                audioSource.Play();
            }
            if(animPlayer != null)
            {
                animPlayer.PlayAnimation(pushAnim, 1, animationSpeed);
            }

            if(sM != null)
            {
                if (pushAway)
                {
                    Vector3 dir = (col.transform.position - transform.position).normalized;
                    sM.ApplyExternalForce(dir * pushForce);
                }
                else
                {
                    sM.ApplyExternalForce(transform.forward * pushForce);
                }
            }
            return;
        }

        //Rigidbody rbody = col.GetComponent<Rigidbody>();

        //if (rbody != null)
        //{
        //    if (pushAway)
        //    {
        //        Vector3 dir = (col.transform.position - transform.position).normalized;
        //        AddForceFastDrag(dir * pushForce, ForceMode.Impulse, rbody);
        //    }
        //    else
        //        AddForceFastDrag(transform.forward * pushForce, ForceMode.Impulse, rbody);
        //}
    }

    void OnTriggerStay(Collider col)
    {
        if (!isContinuous) return;

        if (col.tag == "Player")
        {
            StagMovement sM = col.GetComponent<StagMovement>();
            if (canBeBlocked)
            {
                if (sM.speedBreaker.active == true)
                {
                    return;
                }
            }

            if (sM != null)
            {
                if (pushAway)
                {
                    Vector3 dir = (col.transform.position - transform.position).normalized;
                    sM.ApplyExternalForce(dir * pushForce * Time.deltaTime);
                }
                else
                {
                    sM.ApplyExternalForce(transform.forward * Time.deltaTime * pushForce);
                }
            }
            return;
        }

        //Rigidbody rbody = col.GetComponent<Rigidbody>();

        //if (rbody != null)
        //{
        //    if (pushAway)
        //    {
        //        Vector3 dir = (col.transform.position - transform.position).normalized;
        //        AddForceFastDrag(dir * pushForce * Time.deltaTime, ForceMode.Force, rbody);
        //    }
        //    else
        //        AddForceFastDrag(transform.forward * pushForce * Time.deltaTime, ForceMode.Force, rbody);
        //}
    }
}
