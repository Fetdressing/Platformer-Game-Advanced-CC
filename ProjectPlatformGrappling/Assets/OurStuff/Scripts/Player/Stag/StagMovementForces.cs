using UnityEngine;
using System.Collections;

public class StagMovementForces : StagMovement {
    private Rigidbody m_rigidbody;

    private float currMaxSpeed = 0;
    private float normalMaxSpeed = 60;
    private float airMaxSpeed = 180;
    private float maxSpeedMultiplier = 1.0f;
    //[Header("Audio")]
    //public AudioSource jumpAudioSource;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        m_rigidbody = GetComponent<Rigidbody>();
        powerManager = transform.GetComponent<PowerManager>();
        cameraObj = cameraHolder.GetComponentsInChildren<Transform>()[1].transform;
        cameraShaker = cameraObj.GetComponent<CameraShaker>();
        groundChecker = GetComponentsInChildren<GroundChecker>()[0];

        animationH[runForward.name].speed = runAnimSpeedMult;
        animationH[runForwardRight.name].speed = runAnimSpeedMult;
        animationH[runForwardLeft.name].speed = runAnimSpeedMult;
        animationH[idle.name].speed = animationSpeedMult;
        animationH[idleAir.name].speed = animationSpeedMult;
        animationH[jump.name].speed = animationSpeedMult;

        stagRootJointStartY = stagRootJoint.localPosition.y;

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        ToggleDashEffect(false);
        currMovementSpeed = startSpeed;

        currExternalSpeedMult = 1.0f;

        dashTimePoint = 0;
        jumpTimePoint = -5; //behöver vara under 0 så att man kan hoppa dirr när spelet börjar
        dashUsed = true;
        jumpsAvaible = jumpAmount;

        isGrounded = false;
        isGroundedRaycast = false;

        currMaxSpeed = normalMaxSpeed;
    }

    void LateUpdate()
    {
        if (Time.timeScale == 0) return;
        if (isLocked) return;

        if (finalMoveDir.magnitude < 0.01f) return;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(finalMoveDir.x, 0, finalMoveDir.z));
        stagObject.rotation = Quaternion.Slerp(stagObject.rotation, lookRotation, deltaTime * 20);

        if ((moveSpeedMultTimePoint + moveSpeedMultDuration) < Time.time)
        {
            currExternalSpeedMult = 1.0f;
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0) return;
        if (isLocked) return;

        hor = Input.GetAxis("Horizontal");
        ver = Input.GetAxis("Vertical");
        horVector = hor * cameraHolder.right;
        verVector = ver * cameraHolder.forward;
        finalMoveDir = (horVector + verVector).normalized;
        
        isGroundedRaycast = GetGrounded(groundCheckObject);

        distanceToGround = GetDistanceToGround(groundCheckObject);

        if (isGroundedRaycast) //använd endast GetGrounded här, annars kommer man få samma problem när gravitationen slutar verka pga lång raycast
        {
            if (jumpTimePoint < Time.time - 0.4f) //så den inte ska fucka och resetta dirr efter man hoppat
            {
                //dessa resetsen görs här eftersom denna groundchecken är mycket mer pålitlig
                //dashUsed = true; //resettar bara med riktigt grounded så det ska vara mer "snällt"
                jumpsAvaible = jumpAmount;
            }

            if (groundedSlope > maxSlopeGrounded) //denna checken görs här när man är grounded och i charactercontrollerhit när man INTE är grounded
            {
                ApplyExternalForce(groundedNormal * 20); // så man glider för slopes
            }
        }
        else
        {
            groundChecker.Reset(groundedTimePoint);
        }

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (IsDashReady())
        {
            ToggleDashReadyPS(true); //visa att man kan dasha
        }
        else
        {
            ToggleDashReadyPS(false);
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.M))
        {
            //Dash();
            //if (ver < 0.0f) //bakåt
            //{
            //    Dash(-transform.forward);
            //    //Dash(-cameraHolder.forward);
            //}
            //else
            //{
            //    Dash(transform.forward);
            //}
        }

        HandleMovement();

        if (isGrounded)
        {
            if (m_rigidbody.velocity.magnitude > currMaxSpeed)
            {
                Break(0.99f);
            }
        }

        PlayAnimationStates();

    }

    void OnColliderStay(Collision col)
    {
        if (Time.timeScale == 0) return;
        //ContactPoint cPoint = col.contacts[0];
        //fan viktigt :o ful hacks men still
        if (!col.collider.isTrigger)
        {
            for (int i = 0; i < col.contacts.Length; i++)
            {
                if (col.contacts[i].normal.y > 0.5f)
                {
                    float slope = GetSlope(col.contacts[i].normal);
                    if (jumpTimePoint < Time.time - 0.4f) //så den inte ska fucka och resetta dirr efter man hoppat
                    {
                        isGrounded = true;
                        dashUsed = true;
                        jumpsAvaible = jumpAmount;
                        //ySpeed = -gravity * 0.01f; //nollställer ej helt // grounded character has vSpeed = 0...
                    }
                    break;
                }
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "BreakerObject")
        {
            col.GetComponent<BreakerObject>().Break(); //tar sönder objekten
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.tag == "BreakerObject")
        {
            cameraShaker.ShakeCamera(0.2f, 0.6f, true);
        }
    }

    public override void HandleMovement()
    {
        float stagSpeedMultiplier = 1.0f;
        if (isGrounded)
        {
            stagSpeedMultiplier = Mathf.Max(Mathf.Abs(stagRootJointStartY - stagRootJoint.localPosition.y), stagSpeedMultMin); //min värde
            stagSpeedMultiplier = Mathf.Min(stagSpeedMultiplier, stagSpeedMultMax); //max värde
        }

        currMovementSpeed = startSpeed * currExternalSpeedMult;

        verVector = new Vector3(verVector.x, 0, verVector.z); //denna behöver vara under dash så att man kan dasha upp/ned oxå

        finalMoveDir = (horVector + verVector).normalized * stagSpeedMultiplier * currMovementSpeed * (Mathf.Max(0.8f, powerManager.currPower) * 1.2f);
        if (IsWalkable(1.2f, 4, (horVector + verVector).normalized, maxSlopeGrounded)) //dessa värden kan behöva justeras
        {
            m_rigidbody.AddForce(finalMoveDir, ForceMode.Force);
        }
        else
        {
            finalMoveDir *= 0.1f;
        }
    }

    void Break(float breakingAmount) //bromsa!
    {
        m_rigidbody.velocity *= breakingAmount * Time.deltaTime; 
    }

    public override void Jump()
    {
        if (jumpsAvaible > 0)
        {
            if (Time.time > jumpTimePoint + jumpCooldown)
            {
                PlayJumpEffect();

                jumpsAvaible = Mathf.Max(0, (jumpsAvaible - 1));
                dashUsed = false; //när man blir grounded så kan man använda dash igen, men oxå när man hoppar, SKILLZ!!!
                jumpTimePoint = Time.time;


                if (groundedRaycastObject != null && groundedRaycastObject.tag == "BreakerObject") //breakar objekt om man hoppar på dem
                {
                    groundedRaycastObject.GetComponent<BreakerObject>().Break();
                }

                m_rigidbody.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);

                //if (ySpeed <= 0)
                //{
                //    ySpeed += jumpSpeed;
                //}
                //else
                //{
                //    ySpeed += (jumpSpeed * 0.8f); //mindre force när man redan har force
                //}
                //animationH.Play(jump.name);
                //animationH[jump.name].weight = 1.0f;
            }
        }
    }

    public override void PlayAnimationStates()
    {
        if (animationH == null) return;
        float fadeLengthA = 0.1f;

        if (isGrounded || GetGrounded(groundCheckObject))
        {
            if (ver > 0.1f || ver < -0.1f) //för sig frammåt/bakåt
            {
                if (hor > 0.1f) //rär sig sidledes
                {
                    animationH.CrossFade(runForwardRight.name, fadeLengthA);
                }
                else if (hor < -0.1f)
                {
                    animationH.CrossFade(runForwardLeft.name, fadeLengthA);
                }
                else
                {
                    animationH.CrossFade(runForward.name, fadeLengthA);
                }
            }
            else if (hor > 0.1f) //rär sig sidledes
            {
                animationH.CrossFade(runForwardRight.name, fadeLengthA);
            }
            else if (hor < -0.1f)
            {
                animationH.CrossFade(runForwardLeft.name, fadeLengthA);
            }
            else
            {
                animationH.CrossFade(idle.name, fadeLengthA);
            }
        }
        else //air
        {
            if (!isGrounded)
            {
                animationH.CrossFade(jump.name, fadeLengthA);
            }
            else
            {
                animationH.CrossFade(idleAir.name, fadeLengthA);
            }
        }
    }

    //public override void ApplyYForce(float velY) //till characterscontrollern, inte rigidbody
    //{
    //    jumpTimePoint = Time.time;
    //    ySpeed += velY;
    //}
    //public override void ApplyYForce(float velY, float maxVel) //till characterscontrollern, inte rigidbody, med ett max värde
    //{
    //    if (ySpeed >= maxVel) return;
    //    jumpTimePoint = Time.time;
    //    ySpeed += velY;
    //}

    //public override bool Dash(bool b)
    //{
    //    if (!IsDashReady()) return false;
    //    powerManager.SufficentPower(-dashPowerCost, true); //camerashake, konstig syntax kanske du tycker, men palla göra det fancy!
    //    StartCoroutine(MoveDash(false));
    //    return true;
    //}

    public override bool IsDashReady()
    {
        if (!powerManager.SufficentPower(-dashPowerCost)) return false;
        if (dashTimePoint + dashCooldown > Time.time) return false;
        if (dashUsed) return false;

        return true;
    }

    public virtual IEnumerator MoveDash(Vector3 dir, bool setDir)
    {
        yield break;
        //ySpeed = -gravity * 0.01f; //nollställer ej helt
        //dashUsed = true;
        //ToggleDashEffect(true);
        //powerManager.AddPower(-dashPowerCost);
        //dashTimePoint = Time.time;
        //currDashTime = 0.0f;
        //float startDashTime = Time.time;
        //while (currDashTime < maxDashTime)
        //{
        //    dashVel = dir * dashSpeed;
        //    if (!IsWalkable(0, 1, dashVel)) //så den slutar dasha när den går emot en vägg
        //    {
        //        ToggleDashEffect(false);
        //        dashVel = Vector3.zero;
        //        yield break;
        //    }
        //    currDashTime = Time.time - startDashTime;
        //    yield return null;
        //}
        //ToggleDashEffect(false);
        //dashVel = Vector3.zero;

    }

    public override void ApplyExternalForce(Vector3 moveDir, bool mu)
    {
        Debug.Log("Force!");
        m_rigidbody.AddForce(moveDir);
    }

    public override void PlayJumpEffect()
    {
        if (gameObject.activeSelf == false) return;

        AudioSource dAS = jumpEffectObject.GetComponent<AudioSource>();
        ParticleTimed psTimed = jumpEffectObject.GetComponentInChildren<ParticleTimed>();

        if (dAS != null)
        {
            dAS.Play();
        }

        if (psTimed != null)
        {
            psTimed.StartParticleSystem();
        }

    }

    public override void ToggleDashEffect(bool b)
    {
        if (gameObject.activeSelf == false) return;
        dashEffectObject.transform.rotation = cameraHolder.rotation;
        float trailOriginalTime = 1.0f;
        float startWidth = 1;
        float endWidth = 0.1f;
        TrailRenderer[] tR = dashEffectObject.GetComponentsInChildren<TrailRenderer>();
        ParticleSystem[] pS = dashEffectObject.GetComponentsInChildren<ParticleSystem>();
        AudioSource dAS = dashEffectObject.GetComponent<AudioSource>();

        if (b)
        {
            if (dAS != null)
            {
                dAS.Play();
            }
        }

        for (int i = 0; i < tR.Length; i++)
        {
            if (b)
            {
                tR[i].time = trailOriginalTime;
                tR[i].startWidth = startWidth;
                tR[i].endWidth = endWidth;
            }
            else
            {
                StartCoroutine(ShutDownTrail(tR[i]));
            }
        }

        for (int i = 0; i < pS.Length; i++)
        {
            if (b)
            {
                pS[i].Play();
            }
            else
            {
                pS[i].Stop();
            }
        }
    }
    public override IEnumerator ShutDownTrail(TrailRenderer tR)
    {
        while (tR.time > 0.0f)
        {
            tR.time -= 3 * deltaTime;
            tR.startWidth -= deltaTime;
            tR.endWidth -= deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public override void ApplySpeedMultiplier(float multiplier, float duration) //slows o liknande, updateras i LateUpdate()
    {
        currExternalSpeedMult = multiplier;
        moveSpeedMultTimePoint = Time.time;
        moveSpeedMultDuration = duration;
    }

    public override void ToggleDashReadyPS(bool b)
    {
        if (dashReadyPS == null) return;
        if (b)
        {
            if (!dashReadyPS.isPlaying)
                dashReadyPS.Play();
        }
        else
        {
            if (dashReadyPS.isPlaying)
                dashReadyPS.Stop();
        }
    }

    public override bool IsWalkable(float yOffset, float distance, Vector3 direction, float maxSlope)
    {
        //if(isGroundedRaycast)
        //{
        //    if (groundedSlope > maxSlopeGrounded) //lutning
        //    {
        //        if (Physics.Raycast(transform.position + new Vector3(0, yOffset, 0), direction, distance, groundCheckLM))
        //        {
        //            return false;
        //        }
        //    }
        //    return true; //man står på marken utan lutning, då ska man kunna gå
        //}
        RaycastHit rHit;
        if (Physics.Raycast(transform.position + new Vector3(0, yOffset, 0), direction, out rHit, distance, groundCheckLM))
        {

            float angleValue = Vector3.Angle(rHit.normal, Vector3.up);
            //Debug.Log(angleValue.ToString());

            if (angleValue > maxSlopeGrounded)
            {
                return false;
            }
        }
        return true;
    }

    public override bool GetGrounded()
    {
        RaycastHit rHit;

        if (Physics.Raycast(this.transform.position + new Vector3(0, groundedCheckOffsetY, 0), Vector3.down, out rHit, groundedCheckDistance, groundCheckLM))
        {
            if (rHit.transform == this.transform || rHit.normal.y < 0.5f) { return false; } //MEH DEN SKA EJ COLLIDA MED SIG SJÄLV

            groundedSlope = GetSlope(rHit.normal);
            groundedNormal = rHit.normal;

            if (groundedSlope > maxSlopeGrounded) { return false; }

            if (isGrounded == false) //om man inte var grounded innan
            {
                groundedTimePoint = Time.time;
            }

            groundedRaycastObject = rHit.transform;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool GetGrounded(Transform tChecker) //från en annan utgångspunkt
    {
        RaycastHit rHit;
        if (Physics.Raycast(tChecker.position + new Vector3(0, groundedCheckOffsetY, 0), Vector3.down, out rHit, groundedCheckDistance, groundCheckLM))
        {
            if (rHit.transform == this.transform || rHit.normal.y < 0.5f) { return false; } //MEH DEN SKA EJ COLLIDA MED SIG SJÄLV

            groundedSlope = GetSlope(rHit.normal);
            groundedNormal = rHit.normal;

            if (groundedSlope > maxSlopeGrounded) { return false; }

            if (isGrounded == false) //om man inte var grounded innan
            {
                groundedTimePoint = Time.time;
            }
            groundedRaycastObject = rHit.transform;
            return true;
        }
        else
        {
            return false;
        }
    }


    public override Transform GetGroundedTransform(Transform tChecker) //får den transformen man står på, från en annan utgångspunkt
    {
        RaycastHit rHit;
        if (Physics.Raycast(tChecker.position + new Vector3(0, groundedCheckOffsetY, 0), Vector3.down, out rHit, groundedCheckDistance, groundCheckLM))
        {
            if (rHit.transform == this.transform || rHit.normal.y < 0.5f) { return transform; } //MEH DEN SKA EJ COLLIDA MED SIG SJÄLV

            groundedSlope = GetSlope(rHit.normal);
            groundedNormal = rHit.normal;

            if (groundedSlope > maxSlopeGrounded) { return transform; }

            groundedRaycastObject = rHit.transform;
            return rHit.transform;
        }
        else
        {
            return transform;
        }
    }

    public override Transform GetGroundedTransform(Transform tChecker, float distance) //får den transformen man står på, från en annan utgångspunkt
    {
        RaycastHit rHit;
        if (Physics.Raycast(tChecker.position + new Vector3(0, groundedCheckOffsetY, 0), Vector3.down, out rHit, distance, groundCheckLM))
        {
            if (rHit.transform == this.transform || rHit.normal.y < 0.5f) { return transform; } //MEH DEN SKA EJ COLLIDA MED SIG SJÄLV

            groundedSlope = GetSlope(rHit.normal);
            groundedNormal = rHit.normal;

            if (groundedSlope > maxSlopeGrounded) { return transform; }

            groundedRaycastObject = rHit.transform;
            return rHit.transform;
        }
        else
        {
            return transform;
        }
    }
}