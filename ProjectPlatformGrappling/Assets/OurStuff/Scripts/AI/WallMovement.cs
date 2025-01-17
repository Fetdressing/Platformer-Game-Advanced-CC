﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMovement : BaseClass {
    Rigidbody m_Rigidbody;
    //public float maxSpeed = 100;

    public Transform checkerT; //vart man kastar raycastsen
    public LayerMask groundLM;
    public Transform rotater;

    bool isGrounded = false;
    [HideInInspector]public bool jumping = false;
    float maxYSpeed = 0.1f;
    float gravityBoost = 100;

    IEnumerator jumpIE = null;
    // Use this for initialization
    void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        if(checkerT == null)
        {
            checkerT = transform;
        }
        m_Rigidbody = GetComponent<Rigidbody>();

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        jumping = false;
        StopAllCoroutines();
        jumpIE = null;
    }

    //   // Update is called once per frame
    //   void Update () {
    //       Move(Vector3.right, Time.deltaTime * 20);
    //}

    void Update()
    {       
        if(Physics.Raycast(checkerT.position, Vector3.down, 8, groundLM))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
            
            m_Rigidbody.AddForce(Vector3.down * gravityBoost);
        }
         
        if (m_Rigidbody.velocity.y > maxYSpeed && !jumping) //hantera Y separat, behöver inte oroa mig om gravitationen för denna bara limitar om velocityn är positiv!
        {
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, maxYSpeed, m_Rigidbody.velocity.z);
        }
    }

    public void Jump(float jumpForce)
    {
        if (jumpIE != null) return;
        if (!isGrounded) return;
        if (jumping) return;

        jumpIE = PerformJump(jumpForce);
        StartCoroutine(jumpIE);
    }

    IEnumerator PerformJump(float jumpForce)
    {
        jumping = true;
        m_Rigidbody.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        float maxTime = 4;
        float timerT = Time.time + maxTime;
        //yield return new WaitForSeconds(maxTime);
        while (m_Rigidbody.velocity.y > 0.01f && Time.time < timerT)
        {
            yield return new WaitForEndOfFrame();
        }
        jumping = false;
        jumpIE = null;
    }

    public void Move(Vector3 dir, float speed) //keep me on wall
    {
        RaycastHit rHit;
        float checkDistance = 10;
        float maxSpeed = speed * 10;

        dir.y = 0; //den inkommna Y tar vi bort

        Vector3 lookDir = Vector3.zero;
        //if (!Physics.Raycast(checkerT.position, Vector3.down, out rHit, checkDistance, groundLM))
        //{
        //    dir = Vector3.down;
        //}
        if (Physics.Raycast(checkerT.position, dir, out rHit, checkDistance, groundLM))
        {
            Vector3 cross1 = Vector3.Cross(rHit.normal, Vector3.up);
            Vector3 cross2 = Vector3.Cross(cross1, rHit.normal); //kommer ge mig en vector upp/ned längs med normalens face

            dir = cross2;
        }
        else
        {
            Physics.Raycast(checkerT.position, Vector3.down, out rHit, checkDistance, groundLM); //så man får rHit nu normal för hur gubben ska titta, kanske blir knaaaas?
        }

        if(rotater != null)
        {
            Vector3 c = Vector3.Cross(Vector3.right, dir);
            //rotater.up = c;
            Vector3 upDir = Vector3.up;
            if(rHit.collider != null)
            {
                upDir = rHit.normal;
            }

            if (dir != Vector3.zero)
            {
                Quaternion newQuat = Quaternion.LookRotation(dir, upDir);
                if (newQuat != rotater.rotation)
                {
                    rotater.rotation = Quaternion.Lerp(rotater.rotation, newQuat, speed * 0.01f);
                }
            }
        }

        if(jumping)
        {
            dir.y = 0;
        }

        m_Rigidbody.AddForce(dir * speed * 1000);
        Vector3 forceXZ = new Vector3(m_Rigidbody.velocity.x, m_Rigidbody.velocity.y, m_Rigidbody.velocity.z);
        if(forceXZ.magnitude > maxSpeed)
        {
            forceXZ = m_Rigidbody.velocity.normalized * maxSpeed;
            m_Rigidbody.velocity = new Vector3(forceXZ.x, m_Rigidbody.velocity.y, forceXZ.z);

        }
    }
}
