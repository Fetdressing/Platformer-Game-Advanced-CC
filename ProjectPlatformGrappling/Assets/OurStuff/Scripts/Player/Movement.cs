using UnityEngine;
using System.Collections;

public class Movement : BaseRigidbody {
    public Transform cameraObj;
    public AudioSource movementAudioSource;
    private Transform thisTransform;
    private Health thisHealth;

    private float distanceToGround = 100000000;

    private float speed = 8000;
    [HideInInspector]
    private float jumpForce = 30;
    private float maxSpeed = 55;

    private Vector3 hor = new Vector3(0,0,0); //har dem här så jag kan hämta värdena via update
    private Vector3 ver = new Vector3(0, 0, 0);

    private LayerMask layermaskForces;
    public ParticleSystem slideGroundParticleSystem;
    public AudioClip slideGroundSound;

    public PullField pullField; //som drar till sig grejer till spelaren, infinite gravity!
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        thisTransform = this.transform;
        thisHealth = thisTransform.GetComponent<Health>();
        thisRigidbody = thisTransform.GetComponent<Rigidbody>();
        isGrounded = false;
        groundCheckLM = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("MagneticBall") | 1 << LayerMask.NameToLayer("Ragdoll"));
        layermaskForces = groundCheckLM;
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        ToggleInfiniteGravity(false);
        slideGroundParticleSystem.GetComponent<ParticleTimed>().isReady = true;
    }

    // Update is called once per frame
    void FixedUpdate () {
        
        ver = new Vector3(ver.x, 0, ver.z);
        //thisRigidbody.MovePosition(thisTransform.position + ((hor + ver) * Time.deltaTime * speed));
        float finalSpeed;

        //finalSpeed = speed * Mathf.Clamp(1 - (distanceToGround/100), 0.01f, 1.0f);

        if(isGrounded)
        {
            finalSpeed = speed;
        }
        else
        {
            finalSpeed = speed * 0.7f;
        }

        //thisRigidbody.AddForce(((hor + ver) * Time.deltaTime * finalSpeed), ForceMode.Force);
        if (isGrounded)
        {
            bool slowDragApplied = AddForceSlowDrag(((hor + ver) * Time.deltaTime * finalSpeed * 1.9f), ForceMode.Force, thisRigidbody);
            if(slowDragApplied && thisRigidbody.velocity.magnitude > maxSpeed)
            {
                Break();
            }
        }
        else
        {
            AddForceFastDrag(((hor + ver) * Time.deltaTime * finalSpeed), ForceMode.Force, thisRigidbody);
        }
        //thisRigidbody.MovePosition(thisTransform.position + (cameraObj.right * Time.deltaTime * speed * hor));
    }

    void Break()
    {
        thisRigidbody.velocity *= 0.96f;
    }

    void Update()
    {
        hor = Input.GetAxis("Horizontal") * cameraObj.right;
        ver = Input.GetAxis("Vertical") * cameraObj.forward;

        if(Input.GetKeyDown(KeyCode.C))
        {
            ToggleInfiniteGravity(!pullField.enabled);
        }
       
        isGrounded = GetGrounded();
        distanceToGround = GetDistanceToGround();

        if (isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                //thisRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                AddForceFastDrag(Vector3.up * jumpForce, ForceMode.Impulse, thisRigidbody);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (GetGrounded() && col.contacts[0].point.y < thisTransform.position.y)
        {
            float speedHit = col.relativeVelocity.magnitude;

            if (speedHit > thisHealth.speedDamageThreshhold * 0.7f)
            {
                ForcePush(speedHit);
            }
        }
    }

    void ForcePush(float speedF)
    {
        if (slideGroundParticleSystem != null)
        {
            movementAudioSource.PlayOneShot(slideGroundSound);
            //Vector3 vecDir = (thisTransform.position- col.contacts[0].point).normalized;
            //Vector3 dir = Vector3.RotateTowards(thisTransform.position, col.contacts[0].point,1,1);
            //Vector3 vecDir = thisRigidbody.velocity.normalized + col.contacts[0].normal * 0.1f;
            //Quaternion rotation = Quaternion.LookRotation(vecDir);
            //slideGroundParticleSystem.transform.rotation = rotation;
            float baseParSpeed = 1;
            slideGroundParticleSystem.startSpeed = baseParSpeed * speedF;
            //slideGroundParticleSystem.transform.LookAt(col.contacts[0].point + new Vector3(0,1,0));
            //slideGroundParticleSystem.transform.Rotate(dir);
            if (slideGroundParticleSystem.GetComponent<ParticleTimed>().isReady)
            {
                slideGroundParticleSystem.GetComponent<ParticleTimed>().StartParticleSystem();
            }

            //knocka iväg lite stuff
            Collider[] colliders;
            float range = 0.05f * speedF + 10;
            colliders = Physics.OverlapSphere(thisTransform.position, 0.5f * speedF, layermaskForces);
            foreach (Collider col in colliders)
            {
                Transform tr = col.transform;
                if (tr == thisTransform) continue;

                if (tr.GetComponent<Rigidbody>() != null)
                {
                    Rigidbody rigidbodyTemp = tr.GetComponent<Rigidbody>();
                    Vector3 dir;

                    dir = (tr.transform.position - thisTransform.position).normalized;
                    //rigidbodyTemp.AddForce((force * dir * Time.deltaTime) * (1 - distanceMultiplier / range), ForceMode.Force);
                    AddForceFastDrag((0.6f * speedF * dir), ForceMode.Impulse, rigidbodyTemp);
                }
            }
        }
    }

    void ToggleInfiniteGravity(bool b)
    {
        pullField.enabled = b;
        ParticleSystem pullps = pullField.gameObject.GetComponent<ParticleSystem>();

        //pullps.emission.enabled = b;

        if (b)
        {
            pullps.Play();
        }
        else
        {
            pullps.Stop();
        }


        if (b)
        {

        }
        else
        {

        }
    }
}
