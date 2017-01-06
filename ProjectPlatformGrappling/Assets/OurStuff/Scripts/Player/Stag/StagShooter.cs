using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StagShooter : BaseClass {
    private PowerManager powerManager;
    private Transform cameraObj;
    private Camera mainCamera;
    public Transform shooterObj;

    public LayerMask targetLM;
    private float shootForce = 350;
    private float cooldown_Time = 0.1f;
    private float cooldonwTimer = 0.0f;
    private float projectilePowerCost = 0.03f;

    public GameObject projectileType;
    private int poolSize = 100;
    private List<GameObject> projectilePool = new List<GameObject>();

    private AudioSource audioSource;
    public AudioClip shootSound;
    public float volume = 0.7f;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        cameraObj = GameObject.FindGameObjectWithTag("MainCamera").gameObject.transform;
        mainCamera = cameraObj.GetComponentsInChildren<Transform>()[1].GetComponent<Camera>();
        powerManager = transform.GetComponent<PowerManager>();
        audioSource = shooterObj.GetComponent<AudioSource>();

        for(int i = 0; i < poolSize; i++)
        {
            GameObject temp = Instantiate(projectileType.gameObject);
            projectilePool.Add(temp);
            temp.SetActive(false);
        }

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        isLocked = false;
        cooldonwTimer = 0.0f;
    }

    public override void Dealloc()
    {
        base.Dealloc();
        for (int i = 0; i < projectilePool.Count; i++)
        {
            projectilePool[i].GetComponent<ProjectileBase>().Dealloc();
            Destroy(projectilePool[i]);
        }
        projectilePool.Clear();
    }

    // Update is called once per frame
    void Update () {
        if (Time.timeScale == 0) return;
        if (isLocked) return;
        if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.N))
        {
            Fire();
        }
	}

    void Fire()
    {
        if (cooldonwTimer > Time.time) return;
        cooldonwTimer = Time.time + cooldown_Time;

        if (!powerManager.SufficentPower(-projectilePowerCost * 4, true)) {  return; } //* ett värde för att ha lite marginal
        powerManager.AddPower(-projectilePowerCost);

        audioSource.PlayOneShot(shootSound, volume);

        GameObject currProj = null;
        for(int i = 0; i < projectilePool.Count; i++)
        {
            if(projectilePool[i].activeSelf == false)
            {
                currProj = projectilePool[i];
                break;
            }
        }
        if (currProj == null) return;
        Rigidbody currRig = currProj.GetComponent<Rigidbody>();
        ProjectileBase currProjectile = currProj.GetComponent<ProjectileBase>();

        currProj.transform.position = shooterObj.position;
        currProj.transform.forward = shooterObj.forward;
        currProjectile.Fire(2, currProjectile.transform.forward * shootForce);

        //RaycastHit raycastHit;
        //if (Physics.Raycast(mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)), out raycastHit, Mathf.Infinity, targetLM)) //kasta från mitten av skärmen!
        //{
        //    if (Vector3.Dot(raycastHit.normal, mainCamera.transform.position - raycastHit.point) > 0) //normalen mot eller från sig?
        //    {
        //        if (Vector3.Distance(raycastHit.point, mainCamera.transform.position) > (Vector3.Distance(transform.position, mainCamera.transform.position))) //ligger raycasthit framför spelaren? man vill ju ej skjuta bakåt
        //        {
        //            currProj.transform.position = shooterObj.position; //kommer från en random riktning kanske?
        //            currProj.transform.LookAt(raycastHit.point);
        //            currProjectile.Fire(2, currProjectile.transform.forward * shootForce, projectilePowerCost, transform);
        //        }
        //        else
        //        {
        //            currProj.transform.position = shooterObj.position; //kommer från en random riktning kanske?
        //            currProj.transform.forward = cameraObj.forward;
        //            currProjectile.Fire(2, currProjectile.transform.forward * shootForce, projectilePowerCost, transform);
        //        }

        //    }
        //    else
        //    {
        //        currProj.transform.position = shooterObj.position; //kommer från en random riktning kanske?
        //        currProj.transform.forward = cameraObj.forward;
        //        currProjectile.Fire(2, currProjectile.transform.forward * shootForce, projectilePowerCost, transform);
        //    }
        //}
        //else
        //{
        //    currProj.transform.position = shooterObj.position; //kommer från en random riktning kanske?
        //    currProj.transform.forward = cameraObj.forward;
        //    currProjectile.Fire(2, currProjectile.transform.forward * shootForce, projectilePowerCost, transform);
        //}

    }
}
