using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//[RequireComponent(typeof(CharacterController))]
public class StagMovement : BaseClass
{
    [System.NonSerialized]
    public bool isCCed = false;
    [System.NonSerialized]
    //public bool isLocked = false; //förhindrar alla actions, ligger i baseclass
    public Transform cameraHolder; //den som förflyttas när man rör sig med musen
    protected Transform cameraObj; //kameran själv
    protected CameraShaker cameraShaker;
    protected Camera mainCamera;
    public Camera unitDetectionCamera;
    protected CharacterController characterController;
    protected PowerManager powerManager;
    protected ControlManager controlManager;
    [System.NonSerialized]
    public Vector3 yMiddlePointOffset = new Vector3(0, 3, 0);

    protected int fUpdatesPassed = 0; //hur många fixedupdates det har gått sedan senaste updaten
    protected int maxFUpdatesOnUpdate = 5; //just för movement så man inte ska få sådana mega spike förflyttningar

    [System.NonSerialized] public StagSpeedBreaker speedBreaker;
    protected float speedBreakerActiveSpeed = 1.8f; //vid vilken fart den går igång
    protected float speedBreakerTime = 0.2f; //endel tid i själva StagSpeedBreaker scriptet
    protected float speedBreakerTimer = 0.0f;

    protected float distanceToGround = Mathf.Infinity;
    public Transform stagRootJoint; //den ska röra på sig i y-led
    protected float stagRootJointStartY; //krävs att animationen börjar i bottnen isåfall
    public Transform stagObject; //denna roteras så det står korrekt

    protected float startSpeed = 250; //acceleration
    protected float jumpSpeed = 110;
    protected float gravity = 180;
    float addedGravity = 0.0f; //läggs till gravity om man har mer momentum
    [System.NonSerialized]public float minimumGravity = -30;
    [System.NonSerialized]public float currGravityModifier = 1.0f;
    protected Vector3 yVector;
    protected float stagSpeedMultMax = 1.5f;
    protected float stagSpeedMultMin = 0.85f;

    protected float currMovementSpeed; //movespeeden, kan påverkas av slows
    [System.NonSerialized]public float currExternalSpeedMult = 1.0f; //100% movespeed, påverkar slows n shit
    protected float moveSpeedMultTimePoint = -5; //när extern slow/speed-up var applyat
    protected float moveSpeedMultDuration;
    [System.NonSerialized]public float ySpeed; //aktiv variable för vad som händer med gravitation/jump
    protected float jumpTimePoint = -5; //när man hoppas så den inte ska resetta stuff dirr efter man hoppat
    [System.NonSerialized] public int jumpAmount = 2; //hur många hopp man får
    protected int jumpsAvaible = 0; //så man kan hoppa i luften also, förutsatt att man resettat den på marken
    protected float jumpCooldown = 0.08f;
    protected float jumpGroundResetCooldown = 0.3f;
    public GameObject jumpEffectObject;
    private ParticleTimed[] jumpEffects;
    [System.NonSerialized] public float currFrameMovespeed = 0; //hur snabbt man rört sig denna framen
    protected Vector3 lastFramePos = Vector3.zero;

    private IEnumerator currDashIE; //så man kan avbryta den
    [System.NonSerialized]public IEnumerator staggDashIE; //sätts även ifrån andra script som StagSpeedBreaker
    [System.NonSerialized]public IEnumerator staggIE; //normala stag grejen
    [System.NonSerialized]public float dashTimePoint; //mud påverkar denna så att man inte kan dasha
    protected float dashGlobalCooldown = 0.0f;
    protected float dashGroundCooldown = 1f; //går igång ifall man dashar från marken
    protected float dashSpeed = 400;
    protected float modDashSpeed; //blir dashspeeden men kan sedan ökas när man närmar sig dashtargets
    protected float dashHelpDistance = 70; //avståndet som karaktären börjar försöka nå dashtarget mer (ökar speeden tex)
    int dashUpdates = 20; //hur många fixedupdates som dash ska köra, detta gör den consistent i hur långt den åker oavsett framerate. Kanske en skum lösning men det funkar asbra!
    protected float startMaxDashTime = 0.08f; //den går att utöka
    [System.NonSerialized] public float maxDashTime;
    protected float dashPowerCost = 0.058f; //hur mycket power det drar varje gång man dashar
    [System.NonSerialized]public bool dashUsed = false; //så att man måste bli grounded innan man kan använda den igen
    public GameObject dashEffectObject;
    public ParticleSystem dashReadyPS; //particlesystem som körs när dash är redo att användas
    protected int currDashCombo = 0; //hur många dashes som gjorts i streck, används för att öka kostnaden tex
    protected float dashComboMult = 0.0011f;
    protected float dashComboResetTime = 0.90f;
    protected float dashComboResetTimer = 0.0f;
    public LayerMask unitCheckLM; //fiender o liknande som dash ska styras mot
    //during dash
    [System.NonSerialized] public Transform lastUnitHit; //så att man inte träffar samma igen
    Transform dashTarget = null;
    HealthSpirit dashTargetHS = null;
    Vector3 dirMod = Vector3.zero;
    Vector3 groundOffset = new Vector3(0, 0.4f, 0);
    float currDashTime;
    float startDashTime = 0.0f;
    float extendedTime = 0.0f;
    int currDashUpdates = 0;

    protected float knockForceMovingPlatform = 420; //om man hamnar på fel sidan av moving platform så knuffas man bort lite

    //moving platform

    [System.NonSerialized] public Transform activePlatform;

    protected Vector3 activeGlobalPlatformPoint;
    protected Vector3 activeLocalPlatformPoint;

    protected float airbourneTime = 0.0f;
    //moving platform

    protected Vector3 horVector = new Vector3(0, 0, 0); //har dem här så jag kan hämta värdena via update
    protected Vector3 verVector = new Vector3(0, 0, 0);
    protected Vector3 lastHV_Vector = Vector3.zero; //senast som horVector och verVector hade ett värde (dvs inte vector3.zero)
    protected Vector3 lastH_Vector = Vector3.zero; //senast som horVector hade ett värde (dvs inte vector3.zero)
    protected Vector3 lastV_Vector = Vector3.zero; //senast som verVector hade ett värde (dvs inte vector3.zero)
    protected float hor, ver;
    [System.NonSerialized] public Vector3 dashVel = new Vector3(0, 0, 0); //vill kunna komma åt denna, så därför public
    private Vector3 lastDashVel = new Vector3(0,0,0); //när jag lerpar till dashtargets så vill jag fortfarande kunna ha en velocity att mata över till currmomentum
    Vector3 startDashDir = Vector3.zero; //används i dashupdates för att använda den senaste (den första nu istället) legit dashvelocityn
    protected Vector3 externalVel = new Vector3(0, 0, 0);
    [System.NonSerialized] public Vector3 currMomentum = Vector3.zero; //så man behåller fart även efter man släppt på styrning
    protected Vector3 nextMove = Vector3.zero; //denna sätts i fixedupdate, sen körs move i update av detta värdet. Så man får frame independency
    protected float startLimitSpeed = 60;
    [System.NonSerialized]public float currLimitSpeed; //max momentumen, hämtas från script som WallJumpObj
    protected Vector3 updateTrans;

    public Text moveStackText;
    protected float movementStackGroundedTime = 5.0f;
    protected float movementStackGroundedTimer = 0.0f;

    protected float movementStackResetTimer = 0.0f;
    protected float movementStackResetTime = 2.0f;
    //[System.NonSerialized]public int currMovementStacks.value = 0; //får mer stacks när man dashar och hoppar mycket
    [System.NonSerialized]
    public Pointer<int> currMovementStacks = new Pointer<int>();
    [System.NonSerialized]
    public Pointer<int> wantedMovementStacks;//den pekar på antingen hidden eller real movestacks och currMovementStacks lerpar till denna
    private float wantedTimepointChanged = 0.0f; //när wantedMovementStacks bytte vad den peka på senast
    int startLerpValue = 1; //så man får en linear interpolation istället för acceleration som fås när man lerpar från det aktiva värdet
    [System.NonSerialized]
    public Pointer<int> hiddenMovementStacks = new Pointer<int>();//används när man slutat använda så mkt input
    [System.NonSerialized]
    public Pointer<int> realMovementStacks = new Pointer<int>();//används när man dashar, hoppar och är aktiv
    private int minSoftMovementStacks = 10; //om man slutar hoppa/dasha så hamnar den snabbt här


    public PullField pullField; //som drar till sig grejer till spelaren, infinite gravity!

    [Header("Ground Check")]
    public Transform groundCheckObject;
    protected float groundedCheckOffsetY = 0.6f;
    protected const float groundedCheckDistance = 16f;
    [System.NonSerialized]
    public bool isGrounded;
    [System.NonSerialized]
    public bool isGroundedRaycast;
    protected Transform groundedRaycastObject; //objektet som man blev grounded med raycast på
    protected const float dotValueFaceplant = -0.9f; //vilket värde som mot en väggs normal räknas som en faceplant

    public LayerMask groundCheckLM;
    protected float groundedTimePoint = 0; //när man blev grounded
    protected float maxSlopeGrounded = 80; //vilken vinkel det som mest får skilja på ytan och vector3.down när man kollar grounded
    protected float staggForce = 40; //hur långt man knuffas av stagg
    protected float groundedSlope = 0;
    protected Vector3 groundedNormal = Vector3.zero;
    protected GroundChecker groundChecker; //så man kan resetta stuff till camerashake tex

    [Header("Animation")]
    public Animation animationH;
    public float runAnimSpeedMult = 2.0f;
    public float animationSpeedMult = 2.0f; //en overall speed som sätts i början

    public AnimationClip runForward;
    public AnimationClip runForwardRight;
    public AnimationClip runForwardLeft;
    public AnimationClip idle;
    public AnimationClip idleAir;
    public AnimationClip jump;

    //[Header("Audio")]
    //public AudioSource jumpAudioSource;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        characterController = transform.GetComponent<CharacterController>();
        powerManager = transform.GetComponent<PowerManager>();
        cameraHolder = GameObject.FindGameObjectWithTag("MainCamera").transform;
        cameraObj = cameraHolder.GetComponentsInChildren<Transform>()[1].transform;
        mainCamera = cameraObj.GetComponent<Camera>();
        cameraShaker = cameraObj.GetComponent<CameraShaker>();
        groundChecker = GetComponentsInChildren<GroundChecker>()[0];

        controlManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<ControlManager>();
        currGravityModifier = 1.0f;

        try
        {
            speedBreaker = GetComponentsInChildren<StagSpeedBreaker>()[0];
        }
        catch
        {
            Debug.Log("Hittade ingen Speedbreaker i mina children");
        }

        animationH[runForward.name].speed = runAnimSpeedMult;
        animationH[runForwardRight.name].speed = runAnimSpeedMult;
        animationH[runForwardLeft.name].speed = runAnimSpeedMult;
        animationH[idle.name].speed = animationSpeedMult;
        animationH[idleAir.name].speed = animationSpeedMult;
        animationH[jump.name].speed = animationSpeedMult;

        stagRootJointStartY = stagRootJoint.localPosition.y;

        InitJumpEffects();

        maxSlopeGrounded = characterController.slopeLimit;

        //resettar inte dessa vid reset då jag vill behålla mina stacks efter respawn
        realMovementStacks.value = 1;
        hiddenMovementStacks.value = 1;
        currMovementStacks.value = 1;
        //currMovementStacks = realMovementStacks;
        wantedMovementStacks = realMovementStacks;
        AddMovementStack(5); //startvärde

        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        //isLocked resettas inte då den kanske är viktig till stuff
        isCCed = false;
        Time.timeScale = 1.0f;
        ToggleDashEffect(false);
        speedBreaker.Disable();
        BreakDash();
        currMovementSpeed = startSpeed;

        dashVel = new Vector3(0, 0, 0);
        externalVel = new Vector3(0, 0, 0);
        lastHV_Vector = Vector3.zero;
        lastH_Vector = Vector3.zero; //senast som horVector hade ett värde (dvs inte vector3.zero)
        lastV_Vector = Vector3.zero;
        ySpeed = minimumGravity; //nollställer ej helt
        //currMomentum = Vector3.zero; nej den sätts i spawnmanagern istället
        //currGravityModifier = 1.0f; jag vill nog inte resetta denna vid spawn
        currExternalSpeedMult = 1.0f;
        currLimitSpeed = startLimitSpeed;

        maxDashTime = startMaxDashTime;
        dashTimePoint = 0;
        jumpTimePoint = -5; //behöver vara under 0 så att man kan hoppa dirr när spelet börjar
        //ToggleInfiniteGravity(false);
        dashUsed = false;
        jumpsAvaible = jumpAmount;

        //isGrounded = false;
        isGroundedRaycast = false;
    }

    void LateUpdate()
    {
        if (Time.timeScale == 0) return;
        if (isLocked) return;
        if (isCCed) return;

        if ((moveSpeedMultTimePoint + moveSpeedMultDuration) < Time.time)
        {
            currExternalSpeedMult = 1.0f;
        }
        else //ha igång någon effekt
        {
            Debug.Log("En effekt");
        }

        if (currMomentum.magnitude < 0.01f) return;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(currMomentum.x, 0, currMomentum.z));
        //Quaternion lookRotation = Quaternion.LookRotation(new Vector3(cameraHolder.forward.x, 0, cameraHolder.forward.z));
        stagObject.rotation = Quaternion.Slerp(stagObject.rotation, lookRotation, deltaTime * 20);
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0) return;
        if (isLocked) return;
        if (isCCed) return;

        CheckGroundTouch(); //hanterar jumpresets tex

        fUpdatesPassed++; //resettas vid varje vanlig update, så man vet hur många FixedUpdates som har passerat
        if (activePlatform != null)
        {
            Vector3 newGlobalPlatformPoint = activePlatform.TransformPoint(activeLocalPlatformPoint);
            Vector3 moveDistance = (newGlobalPlatformPoint - activeGlobalPlatformPoint);

            if (activeLocalPlatformPoint != Vector3.zero)
            {
                //transform.position = transform.position + moveDistance;
                characterController.Move(moveDistance);
            }
        }

        if (activePlatform != null)
        {
            activeGlobalPlatformPoint = transform.position;
            activeLocalPlatformPoint = activePlatform.InverseTransformPoint(transform.position);
        }

        if (GetGroundedTransform(groundCheckObject, 2) != activePlatform)
        {
            activePlatform = null; //kolla om platformen fortfarande finns under mig eller ej
        }

        if (fUpdatesPassed > maxFUpdatesOnUpdate) return; //så man inte får dessa mega spikes i movement

        Vector3 tempExternalVal = externalVel; //spara värdet innan man minskar för att kunna lägga på det på momentum
        //externalVel = Vector3.Lerp(externalVel, Vector3.zero, 0.01f * 5); //ta sakta bort den externa forcen
        Break(5, ref externalVel);

        Vector3 lostEVel = tempExternalVal - externalVel;
        lostEVel.y = 0;
        currMomentum += lostEVel; //lägg på det man tappat på momentum istället

        Vector3 tempMomentum = HandleMovement(); //moddar finalMoveDir
        currMomentum += tempMomentum;

        //**BREAK**
        Vector3 currMomXZ = new Vector3(currMomentum.x, 0, currMomentum.z);
        float currMomY = currMomentum.y; ///inkluderar Y nu oxå, testa

        if (currMomXZ.magnitude > currLimitSpeed) //dessa kanske bör ligga separat utanför denna funktionen eftersom jag ändrade om denna funktion
        {
            Break((25 - currMovementStacks.value * 0.3f), ref currMomXZ);
        }
        else
        {
            if (GetGrounded(groundCheckObject))
            {
                //Break((4.5f - currMovementStacks.value * 0.08f), ref currMomXZ);
                Break((2.5f + currMovementStacks.value * 0.03f), ref currMomXZ);
            }
            Break((0.15f), ref currMomXZ); //flat break, //börja breaka hela tiden, även i luften med
        }

        //////Break Y
        //if (currMomY > currLimitSpeed * 0.333f) //dessa kanske bör ligga separat utanför denna funktionen eftersom jag ändrade om denna funktion
        //{
        //    Break((25 - currMovementStacks.value * 0.3f), ref currMomY);
        //}
        //else
        //{
        //    Break((0.3f), ref currMomY); //flat break, //börja breaka hela tiden, även i luften med
        //}

        currMomentum = new Vector3(currMomXZ.x, currMomentum.y, currMomXZ.z);
        //**BREAK**

        Vector3 sideVecToMom = Vector3.Cross(currMomentum, transform.up).normalized; //ger en vektor som är åt höger/vänster av momentumen, (innan använde jag transform.right)

        Vector3 mainComparePoint = transform.position + new Vector3(0, characterController.stepOffset, 0);
        Vector3 firstComparePoint = transform.position + new Vector3(0, characterController.stepOffset, 0) + sideVecToMom * characterController.radius;
        Vector3 secondComparePoint = transform.position + new Vector3(0, characterController.stepOffset, 0) + -sideVecToMom * characterController.radius;
        AngleToAvoid(ref currMomentum, mainComparePoint, firstComparePoint, secondComparePoint, characterController.radius + 0.9f, true); //korrekt riktningen så man inte "springer in i väggar"
        ///man vill kanske tryckas ned oxå när man träffar väggar like this?

        ///en längre ned?
        //mainComparePoint = transform.position + new Vector3(0, 0.5f, 0);
        //firstComparePoint = transform.position + new Vector3(0, 0.5f, 0) + sideVecToMom * characterController.radius;
        //secondComparePoint = transform.position + new Vector3(0, 0.5f, 0) + -sideVecToMom * characterController.radius;
        //AngleToAvoid(ref currMomentum, mainComparePoint, firstComparePoint, secondComparePoint, characterController.radius + 0.9f, true); //korrekt riktningen så man inte "springer in i väggar"

        // YYYYY
        //Debug.Log(characterController.isGrounded);
        // apply gravity acceleration to vertical speed:
        addedGravity = currMomentum.magnitude * 0.4f; //0.1f
        if(ySpeed > 0)
        {
            addedGravity = 0;
        }

        if (activePlatform == null && !characterController.isGrounded)
        {
            ySpeed -= (gravity + addedGravity) * 0.01f * currGravityModifier;
        }
        else
        {
            //ySpeed = 0; //behöver inte lägga på gravity när man står på moving platform, varför funkar inte grounded? lol
        }

        DashFixedUpdate();

        nextMove += (currMomentum + dashVel + externalVel + yVector) * 0.01f;
        //print(nextMove);
        //Redirect(ref nextMove, transform.position + new Vector3(0, 2, 0), characterController.radius + 0.9f, 2);
        //print(nextMove + "   2");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Ta bort mig sen!");
            if(QualitySettings.vSyncCount == 2)
                QualitySettings.vSyncCount = 0;
            else
                QualitySettings.vSyncCount = 2;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            powerManager.Die();
        }

            //hämta alltid input, även om man är låst
        hor = controlManager.horAxis;
        ver = controlManager.verAxis;

        horVector = hor * cameraHolder.right;
        verVector = ver * cameraHolder.forward;

        if ((horVector + verVector) != Vector3.zero)
        {
            lastHV_Vector = (horVector + verVector);
            lastV_Vector = verVector;
            lastH_Vector = horVector;
        }

        if (Time.timeScale == 0) return;
        if (isLocked) return;
        if (isCCed) return;
        
        //Debug.Log(GetGroundedDuration().ToString());

        //Debug.Log(ingame_Realtime.ToString() + "\n" + Time.realtimeSinceStartup.ToString());

        if (movementStackResetTimer < ingame_Realtime)
        {
            AddMovementStack(-2); //ändrar realMovementStacks
        }

        float lerpSpeed = 1.0f;
        if(GetGroundedDuration() > 0.0f)
        {
            hiddenMovementStacks.value = CalculateHiddenStacks();
            lerpSpeed = 1.8f;

            if (wantedMovementStacks != hiddenMovementStacks)
            {
                wantedMovementStacks = hiddenMovementStacks;
                wantedTimepointChanged = Time.time;
                startLerpValue = currMovementStacks.value;
            }
            
        }
        else
        {
            if (wantedMovementStacks != realMovementStacks)
            {
                wantedMovementStacks = realMovementStacks;
                wantedTimepointChanged = Time.time;
                startLerpValue = currMovementStacks.value;
            }
            //currMovementStacks = realMovementStacks;
        }
        //else if(GetTimeSinceLastDash() < 5.0f)
        //{
        //    currMovementStacks = realMovementStacks;
        //}
        currMovementStacks.value = (int)Mathf.Lerp(startLerpValue, wantedMovementStacks.value, (Time.time - wantedTimepointChanged) * lerpSpeed);

        moveStackText.text = currMovementStacks.value.ToString();

        //if (isGroundedRaycast && (GetGroundedDuration() > movementStackGroundedTimer)) //efter att ha tappat ett poäng så fortsätter still GetGroundedDuration att öka, därför det minskar poäng snabbare o snabbare, för att man STILL är grounded
        //{
        //    AddMovementStack(-2);
        //}

        //hor = Input.GetAxis("Horizontal");
        //ver = Input.GetAxis("Vertical");

        distanceToGround = GetDistanceToGround(groundCheckObject);

        CheckGroundTouch(); //hanterar jumpresets tex

        if (Input.GetKeyDown(KeyCode.B))
        {
            Slam();
        }

        if (IsDashReady())
        {
            ToggleDashReadyPS(true); //visa att man kan dasha
        }
        else
        {
            ToggleDashReadyPS(false);
        }

        if (controlManager.didDash)
        {
            //Dash(transform.forward);
            if (ver < 0.0f) //bakåt
            {
                //Dash(-transform.forward);
                Dash(false, false);
            }
            else
            {
                Dash(false, false);
            }
        }

        //om man dashar
        DashUpdate();

        //// YYYYY SKA JU LIGGA INNAN MOVING PLATFORM OM MAN VILL HA Y-MOVING PLATFORMS
        ////Debug.Log(characterController.isGrounded);
        //// apply gravity acceleration to vertical speed:
        //if (activePlatform == null && !characterController.isGrounded)
        //{
        //    ySpeed -= gravity * deltaTime;            
        //}
        //else
        //{
        //    ySpeed = 0; //behöver inte lägga på gravity när man står på moving platform, varför funkar inte grounded? lol
        //}

        ////FUNKAAAAAAR EJ?!?!?!? kallas bara när man rör på sig wtf, kan funka ändå
        //if (characterController.isGrounded) //dessa if-satser skall vara separata
        //{
        //    if (jumpTimePoint < Time.time - 0.4f) //så den inte ska fucka och resetta dirr efter man hoppat
        //    {
        //        ySpeed = -gravity * 0.01f; //nollställer ej helt // grounded character has vSpeed = 0...
        //    }
        //}

        //if (Input.GetButtonDown("Jump"))
        //{
        //    Jump();
        //}

        //Vector3 yVector = new Vector3(0, ySpeed, 0);
        //characterController.Move((yVector) * deltaTime);

        // YYYYY
        if (characterController.isGrounded) //dessa if-satser skall vara separata
        {
            if (jumpTimePoint < Time.time - jumpGroundResetCooldown) //så den inte ska fucka och resetta dirr efter man hoppat
            {
                if (ySpeed < 0.0f) //man vill inte resetta om man har upforce
                {
                    ySpeed = minimumGravity; //nollställer ej helt // grounded character has vSpeed = 0...
                    currMomentum.y = 0; ///inte säker på om currmomentum ska hantera någon Y, men denna bör inte skada oavsett
                }
            }
        }
        if (controlManager.didJump)
        {
            Jump();
        }

        yVector = new Vector3(0, ySpeed, 0);
        AngleY(ref yVector, transform.position + new Vector3(0, characterController.height * 0.5f, 0), 10);
        // YYYYY

        //characterController.Move((currMomentum + dashVel + externalVel + yVector) * deltaTime);
        //Redirect(ref nextMove, transform.position + new Vector3(0, 2, 0), characterController.radius + 5f, 0.5f);
        characterController.Move(nextMove * Time.timeScale); //nextMove kalkuleras i fixedupdate
        nextMove *= 1 - Time.timeScale; //nollställer den varje update

        currFrameMovespeed = (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(lastFramePos.x, 0, lastFramePos.z)) * deltaTime) * 100;
        //Debug.Log((currFrameMovespeed).ToString());

        //if (currFrameMovespeed > speedBreakerActiveSpeed)
        //{
        //    Debug.Log((currFrameMovespeed).ToString());
        //    speedBreakerTimer = Time.time + speedBreakerTime;

        //}

        if (speedBreakerTimer > Time.time)
        {
            speedBreaker.Activate();
        }
        else
        {
            speedBreaker.Disable();
        }

        lastFramePos = transform.position;


        if (dashComboResetTimer < Time.time) //man tappar combon för man har varit för seg med och dasha
        {
            currDashCombo = 0;
        }

        PlayAnimationStates();

        fUpdatesPassed = 0;

    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Time.timeScale == 0) return;
        //fan viktigt :o ful hacks men still
        float slope = GetSlope(hit.normal);

        if (!hit.collider.isTrigger)
        {
            if (slope > maxSlopeGrounded)
            {
                //if (isGroundedRaycast || isGrounded)
                //{
                //    if (groundedSlope > maxSlopeGrounded)
                //    {
                //        //ApplyExternalForce(hit.normal * currMomentum.magnitude);
                //        currMomentum = hit.normal * currMomentum.magnitude; //BOUNCE!!
                //        //currMomentum = Vector3.zero;
                //    }
                //}
                //else //om man inte är grounded så använder man ju en gammal slope? denna kan vara farlig att ha här
                //{
                //    //ApplyExternalForce(hit.normal * 20); // så man glider för slopes
                //    //currMomentum = Vector3.zero;
                //}
            }
            else if (slope <= maxSlopeGrounded) //ingen slope, dvs man står på marken, resetta stuff!
            {
                if (ySpeed < 0 || jumpTimePoint < Time.time - jumpGroundResetCooldown) //så den inte ska fucka och resetta dirr efter man hoppat
                {
                    dashUsed = false; //den resettas även när man landar på marken nu! MEN om man dashar från marken så får man cd
                    AddJumpsAvaible(jumpAmount, jumpAmount);
                    //jumpsAvaible = jumpAmount;
                    if (ySpeed < 0.0f) //man vill inte resetta om man har upforce
                    {
                        ySpeed = minimumGravity; //nollställer ej helt // grounded character has vSpeed = 0...
                        currMomentum.y = 0; ///inte säker på om currmomentum ska hantera någon Y, men denna bör inte skada oavsett
                    }
                }
            }


            //if (hit.normal.y < 0.5f) //slå i taket
            //{
            //    ySpeed = -gravity * 0.01f; //nollställer ej helt
            //    //dashUsed = false; //när man blir grounded så kan man använda dash igen
            //    //if (jumpTimePoint < Time.time - 0.4f) //så den inte ska fucka och resetta dirr efter man hoppat
            //    //{
            //    //    ySpeed = -gravity * 0.01f; //nollställer ej helt // grounded character has vSpeed = 0...
            //    //}
            //}
        }

        if (hit.gameObject.tag == "MovingPlatform")
        {
            //MovingPlatform movingPlatform = hit.gameObject.GetComponent<MovingPlatform>();
            //Vector3 platToPlayer = (transform.position - hit.point).normalized;
            //transform.position = new Vector3(transform.position.x, hit.point.y - 0.2f, transform.position.z);
            if (hit.moveDirection.y < -0.9f && hit.normal.y > 0.5f)
            {
                if (activePlatform != hit.transform)
                {
                    activeGlobalPlatformPoint = Vector3.zero;
                    activeLocalPlatformPoint = Vector3.zero;
                }
                activePlatform = hit.transform;
            }

            //if (Vector3.Angle(movingPlatform.moveDirection, platToPlayer) < 80) //rör sig platformen mot spelaren va?//knuffa spelaren lite för denne kom emot en kant, kolla ifall den rör sig mot en? då knockar den ju bort en, bättre än att den alltid gör det?
            //{
            //    Debug.Log(Time.time.ToString());
            //    ApplyExternalForce((transform.position - hit.transform.position).normalized * knockForceMovingPlatform); //knocked away
            //}
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

    public void AngleY(ref Vector3 dir, Vector3 castPos, float raycastLength = 5)
    {
        RaycastHit rHit;

        float magnitude = dir.magnitude;
        if(dir.y > 0) //up
        {
            if (Physics.SphereCast(castPos, characterController.radius - 0.1f, Vector3.up, out rHit, raycastLength, groundCheckLM))
            {
                if (GetSlope(rHit.normal) < maxSlopeGrounded * 0.5f) return;
                Vector3 c = Vector3.Cross(Vector3.down, rHit.normal);
                Vector3 u = Vector3.Cross(c, rHit.normal);

                if (u != Vector3.zero)
                {
                    dir = u * magnitude;
                }
            }
        }
        else //down
        {
            if (Physics.SphereCast(castPos, characterController.radius - 0.1f, Vector3.down, out rHit, raycastLength, groundCheckLM))
            {
                if (GetSlope(rHit.normal) < maxSlopeGrounded * 0.5f) { return; }
                Vector3 c = Vector3.Cross(Vector3.up, rHit.normal);
                Vector3 u = Vector3.Cross(c, rHit.normal);

                if (u != Vector3.zero)
                {
                    dir = u * magnitude;
                }
            }
        }
    }

    public void Redirect(ref Vector3 incomingDir, Vector3 castPoint, float length, float sphereChechRadius)
    {
        float storedMag = incomingDir.magnitude;
        RaycastHit mainHit;
        //Debug.DrawLine(castPoint, castPoint + new Vector3(incomingDir.x, 0, incomingDir.z) * 30, Color.red);
        if (!Physics.SphereCast(castPoint, sphereChechRadius, new Vector3(incomingDir.normalized.x, 0, incomingDir.normalized.z), out mainHit, length, groundCheckLM))
        {
            return;
        }

        //Vector3 cross1 = Vector3.Cross(mainHit.normal, Vector3.up);
        //Vector3 cross2 = Vector3.Cross(cross1, mainHit.normal); //en vektor upp längs med normalen

        Vector3 c = Vector3.Cross(incomingDir.normalized, mainHit.normal);
        Vector3 u = Vector3.Cross(mainHit.normal, c);

        incomingDir = u.normalized * storedMag;
        Debug.DrawLine(castPoint, castPoint + new Vector3(incomingDir.x, incomingDir.y, incomingDir.z) * 30, Color.red);
    }

    public void AngleToAvoid(ref Vector3 dir, Vector3 mainComparePoint, Vector3 firstComparePoint, Vector3 secondComparePoint, float length, bool checkMaxSlope = false)
    {
        Vector3 dirN = dir.normalized;
        RaycastHit mainHit, FirstHit, secondHit;
        float hitLengthMain = Mathf.Infinity, hitLengthFirst = Mathf.Infinity, hitLengthSecond = Mathf.Infinity;

        //Debug.DrawRay(mainComparePoint, dirN * length, Color.red);
        //Debug.DrawRay(firstComparePoint, dirN * length, Color.red);
        //Debug.DrawRay(secondComparePoint, dirN * length, Color.red);

        if (Physics.Raycast(mainComparePoint, dirN, out mainHit, length, groundCheckLM))
        {
            hitLengthMain = Vector3.Distance(mainHit.point, mainComparePoint);
        }

        if (Physics.Raycast(firstComparePoint, dirN, out FirstHit, length, groundCheckLM))
        {
            hitLengthFirst = Vector3.Distance(FirstHit.point, firstComparePoint);
        }

        if (Physics.Raycast(secondComparePoint, dirN, out secondHit, length, groundCheckLM))
        {
            hitLengthSecond = Vector3.Distance(secondHit.point, secondComparePoint);
        }

        if (hitLengthMain == Mathf.Infinity && hitLengthFirst == Mathf.Infinity && hitLengthSecond == Mathf.Infinity) { return; } //alla är lika långa == ingen sne vägg eller liknande
        if (Vector3.Dot(dirN, mainHit.normal) < dotValueFaceplant) { dir = Vector3.zero; return; } //lutad nästan rakt mot väggen
        if (checkMaxSlope && GetSlope(mainHit.normal) < maxSlopeGrounded) return;

        if (hitLengthFirst > hitLengthSecond) //rotera mot second
        {
            Vector3 towards = firstComparePoint - mainComparePoint;
            dir = Vector3.RotateTowards(dir, towards, deltaTime * 50, 0.0F);
        }
        else if (hitLengthFirst < hitLengthSecond)
        {
            Vector3 towards = secondComparePoint - mainComparePoint;
            dir = Vector3.RotateTowards(dir, towards, deltaTime * 50, 0.0F);
        }

        //Debug.DrawRay(mainComparePoint, dir * Mathf.Infinity, Color.blue);
    }

    public void AngleToAvoid(ref Vector3 dir, Vector3 mainComparePoint, Vector3[] secondComparePoints, float length, bool smallest = true, bool checkMaxSlope = false)
    {
        Vector3 dirN = dir.normalized;
        RaycastHit mainHit;
        //List<RaycastHit> secondHits = new List<RaycastHit>() ;
        float hitLengthMain = Mathf.Infinity;
        List<float> hitLengths = new List<float>();

        bool noHits = true;
        for (int i = 0; i < secondComparePoints.Length; i++)
        {
            float hLen;
            if (smallest)
            {
                hLen = Mathf.Infinity;
            }
            else
            {
                hLen = -Mathf.Infinity;
            }

            RaycastHit rH;

            if (Physics.Raycast(secondComparePoints[i], dirN, out rH, length, groundCheckLM))
            {
                hLen = Vector3.Distance(rH.point, secondComparePoints[i]);
                noHits = false;
            }

            hitLengths.Add(hLen);
        }

        if (Physics.Raycast(mainComparePoint, dirN, out mainHit, length, groundCheckLM))
        {
            hitLengthMain = Vector3.Distance(mainHit.point, mainComparePoint);
        }

        if (noHits) return;
        //if (Vector3.Dot(dirN, mainHit.normal) < dotValueFaceplant) return; //lutad nästan rakt mot väggen/golvet ///vill kanske att den inte ska göra nått vid faceplant
        if (checkMaxSlope && GetSlope(mainHit.normal) < maxSlopeGrounded) return;

        if (smallest)
        {
            Vector3 smallestRay = Vector3.zero;
            float smallestRayLength = Mathf.Infinity;

            for (int i = 0; i < secondComparePoints.Length; i++) //hitta den minsta rayen, dvs den som träffats närmst
            {
                if (hitLengths[i] < smallestRayLength)
                {
                    smallestRayLength = hitLengths[i];
                    smallestRay = secondComparePoints[i];
                }
            }

            Vector3 towards = smallestRay - mainComparePoint;
            dir = Vector3.RotateTowards(dir, towards, deltaTime * 50, 0.0F);
        }
        else
        {
            Vector3 biggestRay = Vector3.zero;
            float biggestRayLength = -Mathf.Infinity;

            for (int i = 0; i < secondComparePoints.Length; i++) //hitta den minsta rayen, dvs den som träffats närmst
            {
                if (hitLengths[i] > biggestRayLength)
                {
                    biggestRayLength = hitLengths[i];
                    biggestRay = secondComparePoints[i];
                }
            }

            Vector3 towards = biggestRay - mainComparePoint;
            dir = Vector3.RotateTowards(dir, towards, deltaTime * 50, 0.0F);
        }

        Debug.DrawRay(transform.position, dir * 100, Color.blue);
    }

    public virtual Vector3 HandleMovement() //returnerar vektorn som ska flyttas
    {
        float stagSpeedMultiplier = 1.0f;
        if (characterController.isGrounded)
        {
            stagSpeedMultiplier = Mathf.Max(Mathf.Abs(stagRootJointStartY - stagRootJoint.localPosition.y), stagSpeedMultMin); //min värde
            stagSpeedMultiplier = Mathf.Min(stagSpeedMultiplier, stagSpeedMultMax); //max värde
        }

        currMovementSpeed = startSpeed * currExternalSpeedMult;

        Vector3 horVectorNoY = new Vector3(horVector.x, 0, horVector.z);
        Vector3 verVectorNoY = new Vector3(verVector.x, 0, verVector.z); //denna behöver vara under dash så att man kan dasha upp/ned oxå

        Vector3 momA = new Vector3(0, 0, 0); //accelerationen
        momA = (horVectorNoY + verVectorNoY).normalized * stagSpeedMultiplier * currMovementSpeed;

        if(!isGrounded)
        {
            momA *= 0.7f; //mindre aircontrol
        }

        //poängen i början ska dock vara värda mer!!
        int stackValueMod = Mathf.Max(8, currMovementStacks.value);
        float flatMoveStacksSpeedBonues = Mathf.Max(1, Mathf.Log(stackValueMod, 1.01f));
        flatMoveStacksSpeedBonues *= 0.0018f; //hjälper accelerationen
        //Debug.Log(flatMoveStacksSpeedBonues.ToString());

        float bonusStageSpeed = 1.0f; //ökar för vart X stacks
        bonusStageSpeed = stackValueMod / 3;
        bonusStageSpeed = Mathf.Floor(bonusStageSpeed);
        bonusStageSpeed = Mathf.Max(1, bonusStageSpeed);

        bonusStageSpeed *= 0.22f;
        bonusStageSpeed += 1;

        //Debug.Log(bonusStageSpeed.ToString());

        currLimitSpeed = startLimitSpeed * bonusStageSpeed * currExternalSpeedMult;

        //currMomentum += finalMoveDir * 0.01f * bonusStageSpeed * (1 + flatMoveStacksSpeedBonues); //om inte man är uppe i hög speed så kan man alltid köra currMomentum = finalMoveDir som vanligt
        Vector3 tempMomentum = momA * 0.0075f * bonusStageSpeed * (1 + flatMoveStacksSpeedBonues);
        float momY = currMomentum.y;

        Vector3 currMomXZ = new Vector3(currMomentum.x, 0, currMomentum.z);
        
        //görs inte här längre, utan utanför
        //Vector3 breakVec = Vector3.zero;

        //if (currMomXZ.magnitude > currLimitSpeed) //dessa kanske bör ligga separat utanför denna funktionen eftersom jag ändrade om denna funktion
        //{
        //    breakVec = Break((25 - currMovementStacks.value * 0.3f), currMomXZ);

        //}
        //else
        //{
        //    if (isGroundedRaycast) //släppt kontrollerna, då kan man deaccelerera snabbare! : finalMoveDir.magnitude <= 0.0f
        //    {
        //        breakVec = Break((6 - currMovementStacks.value * 0.2f), currMomXZ);
        //        //Break(2, ref currMomXZ);
        //    }
        //}
        //breakVec.y = 0;
        //tempMomentum += breakVec;
        return tempMomentum;
        //currMomentum = new Vector3(currMomXZ.x, momY, currMomXZ.z);

    }

    void Break(float breakamount, ref Vector3 vec) //brmosa
    {
        if (breakamount < 0)
        {
            breakamount = 0.1f;
        }
        vec = Vector3.Lerp(vec, Vector3.zero, 0.01f * breakamount); //detta är inte braa!
    }

    void Break(float breakamount, ref float f) //bromsa, fast bara ett flyttal
    {
        if (breakamount < 0)
        {
            breakamount = 0.1f;
        }
        f = Mathf.Lerp(f, 0, 0.01f * breakamount); //detta är inte braa!
    }

    Vector3 Break(float breakamount, Vector3 vec) //returnerar vektorn som breakas med
    {
        Vector3 vectemp = Vector3.Lerp(vec, Vector3.zero, 0.01f * breakamount);
        return vectemp - vec;
    }

    public void Stagger(float staggTime, Vector3 staggDir = default(Vector3)) //låser spelaren kvickt
    {
        cameraShaker.ShakeCamera(staggTime, 1f, true, true);

        if(staggIE != null)
        {
            StopCoroutine(staggIE);
        }

        staggIE = DoStagg(staggTime, staggDir);

        StartCoroutine(staggIE);
    }

    IEnumerator DoStagg(float staggTime, Vector3 staggDir = default(Vector3))
    {
        isCCed = true;
        Time.timeScale = 0.2f;
        yield return new WaitForSeconds(staggTime);
        Time.timeScale = 1.0f;
        isCCed = false;
        staggIE = null;
        ApplyExternalForce(staggDir, true);
    }

    void BreakNormalStagg() //breakar den normala staggen
    {
        if (staggIE != null)
        {
            StopCoroutine(staggIE);
        }
        Time.timeScale = 1.0f;
        isCCed = false;
        staggIE = null;
    }

    void CheckGroundTouch()
    {
        if (GetGrounded()) //använd endast GetGrounded här, annars kommer man få samma problem när gravitationen slutar verka pga lång raycast
        {
            if (ySpeed < 0 || jumpTimePoint < Time.time - jumpGroundResetCooldown) //så den inte ska fucka och resetta dirr efter man hoppat, kanske kolla ifall yspeed < 0 ??
            {
                //dessa resetsen görs här eftersom denna groundchecken är mycket mer pålitlig
                //dashUsed = true; //resettar bara med riktigt grounded så det ska vara mer "snällt"
                dashUsed = false;
                AddJumpsAvaible(jumpAmount, jumpAmount);
            }

            if (groundedSlope > maxSlopeGrounded) //denna checken görs här när man är grounded och i charactercontrollerhit när man INTE är grounded
            {
                //if(groundedRaycastObject.tag == "WallJump") görs nu i separat script
                //{
                //    AddJumpsAvaible(jumpAmount, jumpAmount);
                //}
                //ApplyExternalForce(groundedNormal * 20); // så man glider för slopes
                //currMomentum = Vector3.zero;
            }
        }
        else
        {
            groundChecker.Reset(groundedTimePoint);
        }
    }

    public void AddJumpsAvaible(int amount, int maxCount = 1000000000)
    {
        if ((jumpsAvaible + amount) < 0) { jumpsAvaible = 0; return; }
        if ((jumpsAvaible + amount) > maxCount)
        {
            if (maxCount < jumpsAvaible) return; //man vill ju inte minska nuvarande stacks
            jumpsAvaible = maxCount;
            return;
        }
        jumpsAvaible += amount;
    }

    public virtual void Jump()
    {
        if (jumpsAvaible > 0)
        {
            if (Time.time > jumpTimePoint + jumpCooldown)
            {
                BreakDash();
                AddMovementStack(1);
                PlayJumpEffect();
                
                float jumpGroundCheckTimeWindowCLOSE = 0.8f;
                float addedJumpSpeed = 0;

                if (dashTimePoint < jumpTimePoint) //se till så att man använde jump senast och inte dash
                {
                    //kanske bara låta en hoppa extra högt om man drar båda dirr efter varann
                    if ((jumpTimePoint + jumpGroundCheckTimeWindowCLOSE) > Time.time && GetGrounded(0.4f)) //man är fortfarande grounded efter förra hoppet, öka höjden!
                    {
                        addedJumpSpeed = 40;
                    }
                }

                //jumpsAvaible = Mathf.Max(0, (jumpsAvaible - 1));
                AddJumpsAvaible(-1);
                dashUsed = false; //när man blir grounded så kan man använda dash igen, men oxå när man hoppar, SKILLZ!!!
                activePlatform = null; //när man hoppar så är man ej längre attached till movingplatform
                jumpTimePoint = Time.time;

                currMomentum.y = 0; ///inte säker på om currmomentum ska hantera någon Y, men denna bör inte skada oavsett

                if (ySpeed < 0) //ska motverka gravitationen, behövs ej atm?
                    ySpeed = 0;

                if (groundedRaycastObject != null && groundedRaycastObject.tag == "BreakerObject") //breakar objekt om man hoppar på dem
                {
                    groundedRaycastObject.GetComponent<BreakerObject>().Break();
                }

                if (ySpeed <= 0)
                {
                    ySpeed += jumpSpeed + addedJumpSpeed;
                }
                else
                {
                    ySpeed += (jumpSpeed * 0.8f) + addedJumpSpeed; //mindre force när man redan har force
                }
                //animationH.Play(jump.name);
                //animationH[jump.name].weight = 1.0f;
            }
        }
    }

    public void Slam()
    {
        if (!characterController.isGrounded && !isGroundedRaycast && currMovementStacks.value < 5) return;

        RaycastHit rHit;
        float slamMaxDistance = 200;
        if (Physics.Raycast(transform.position, Vector3.down, out rHit, slamMaxDistance, groundCheckLM))
        {
            float dist = Mathf.Abs(transform.position.y - rHit.point.y);
            StartCoroutine(MoveSlam(dist));
        }

        StartCoroutine(MoveSlam(slamMaxDistance));
    }
    IEnumerator MoveSlam(float maxDistance)
    {
        Vector3 startP = transform.position;
        isCCed = true; //kan vara farligt att göra detta här
        yield return new WaitForSeconds(0.3f);
        isCCed = false;
        ySpeed = -170;
        while (!characterController.isGrounded && ySpeed < -2.0f && Vector3.Distance(startP, transform.position) < (maxDistance + 2))
        {
            currMomentum = Vector3.zero;
            ySpeed = -170;
            yield return new WaitForEndOfFrame();
        }
        Debug.Log(Time.time.ToString());
        cameraShaker.ShakeCamera(0.3f + (currMovementStacks.value * 0.1f), 2 + (currMovementStacks.value * 0.2f), true, true);
        //Slam!
        AddMovementStack(-currMovementStacks.value);
    }

    public virtual bool Dash(bool useCameraDir, float extraDashTime = 0)
    {
        float finalDashCost = dashPowerCost + ((float)(currDashCombo++) * dashComboMult);
        if (!IsDashReady(finalDashCost + (finalDashCost * 0.3f))) //lite marginal
        {
            cameraShaker.ShakeCamera(0.2f, 1, true);
            return false;
        }

        dashComboResetTimer = dashComboResetTime + Time.time;
        currDashCombo++; //lägg till det till nästa dash

        //if (!powerManager.SufficentPower(-finalDashCost, true)) return false; //camerashake, konstig syntax kanske du tycker, men palla göra det fancy!
        powerManager.AddPower(-finalDashCost);
        lastUnitHit = null; //resetta denna så att man återigen kan dasha på detta unit
        dashUsed = true;

        if (currDashIE != null)
        {
            StopCoroutine(currDashIE);
        }

        currDashIE = MoveDash(useCameraDir, extraDashTime);
        StartCoroutine(currDashIE);
        return true;
    }

    public virtual bool Dash(bool useCameraDir, bool free, float extraDashTime = 0) //dash utan nån cost eller liknande, alltid frammåt. Den kör inte dashUsed = true
    {
        if(!free)
        {
            float finalDashCost = dashPowerCost + ((float)(currDashCombo++) * dashComboMult);
            if (!IsDashReady(finalDashCost + (finalDashCost * 0.3f))) //lite marginal
            {
                cameraShaker.ShakeCamera(0.2f, 1, true);
                return false;
            }

            dashComboResetTimer = dashComboResetTime + Time.time;
            currDashCombo++; //lägg till det till nästa dash

            //if (!powerManager.SufficentPower(-finalDashCost, true)) return false; //camerashake, konstig syntax kanske du tycker, men palla göra det fancy!
            powerManager.AddPower(-finalDashCost);
            lastUnitHit = null; //resetta denna så att man återigen kan dasha på detta unit
            dashUsed = true;
        }

        if (currDashIE != null)
        {
            StopCoroutine(currDashIE);
        }

        currDashIE = MoveDash(useCameraDir, extraDashTime); //default frammåt
        StartCoroutine(currDashIE);
        return true;
    }

    public virtual bool IsDashReady()
    {
        if (!powerManager.SufficentPower(-dashPowerCost)) return false;
        if (dashTimePoint + dashGlobalCooldown > Time.time) return false;
        if (dashUsed) return false;

        return true;
    }

    public virtual bool IsDashReady(float modCost) //annan kostnad
    {
        if (!powerManager.SufficentPower(-modCost)) return false;
        if (dashTimePoint + dashGlobalCooldown > Time.time) return false;
        if (dashUsed) return false;

        return true;
    }

    public IEnumerator StaggDash(bool useCameraDir, float staggTime, float extraDashTime) //används när man träffar ett dashTarget mest, DASHAR OLIKA SNABBT MED VÄNTE-TIDEN? Stackar den?
    {
        //Debug.Log(" DASHAR OLIKA SNABBT MED VÄNTE-TIDEN? Stackar den?");
        BreakNormalStagg();
        if (currDashIE != null)
        {
            StopCoroutine(currDashIE); //avbryter nuvarande dash
        }
        currDashIE = null;
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"), gameObject.layer, true);
        //if (isLocked) yield break;
        cameraShaker.ShakeCamera(staggTime, 1f, true, true);

        

        //isLocked = true;
        Time.timeScale = 0.02f;
        //yield return null; /// vill kanske verkligen yielda här?
        float timer = staggTime + Time.time + Time.deltaTime;

        while (timer > Time.time && controlManager.didDash == false) //slowmotion tills man dashar
        {
            externalVel = Vector3.zero;
            ySpeed = 0;
            stagObject.transform.forward = cameraObj.forward;
            yield return new WaitForEndOfFrame();
        }
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Unit"), gameObject.layer, false);
        //yield return new WaitForSeconds(staggTime);
        Time.timeScale = 1.0f;
        //isLocked = false;

        Dash(useCameraDir, true, extraDashTime);
        staggDashIE = null;
    }

    public void BreakDash(bool instantDisable = true) //instant disable stänger av dashobjektet dirr
    {
        currDashTime = Mathf.Infinity;
        currDashUpdates = dashUpdates + 1; //så att den ska sluta köra DashFixedUpdate()
        ToggleDashEffect(false);
        unitDetectionCamera.transform.localRotation = Quaternion.identity; //nollställ
        unitDetectionCamera.transform.localPosition = Vector3.zero;
        speedBreaker.UnIgnoreLastUnitHit();
        
        if (dashVel.magnitude > 1.0f)
        {
            currMomentum = new Vector3(lastDashVel.x, 0, lastDashVel.z); ///vill jag ha med Y här? verkar inte vara jättelegit
        }

        dashVel = Vector3.zero;
        lastDashVel = Vector3.zero;

        if (currDashIE != null)
        {
            StopCoroutine(currDashIE);
        }

        currDashIE = null;

        if (instantDisable)
        {
            Time.timeScale = 1.0f;
            if (staggDashIE != null)
            {
                StopCoroutine(staggDashIE);
            }


            speedBreakerTimer = 0.0f;
            speedBreaker.InstantDisable();

        }

        dashTargetHS = null;
        dashTarget = null;
    }

    protected virtual IEnumerator MoveDash(bool useCameraDir, float extraDashTime = 0)
    {
        controlManager.didDash = false;
        maxDashTime = startMaxDashTime + extraDashTime; //den kan utökas sen
        AddMovementStack(1);
        cameraShaker.ChangeFOV(0.05f, 5);
        if(ySpeed > 0)
        {///kanske jag vill göra det oavsett vad ens tidigare ySpeed är!
            ySpeed = minimumGravity; //nollställer ej helt
        }
        //ySpeed = minimumGravity; //nollställer ej helt
        ToggleDashEffect(true);
        dashTimePoint = Time.time;
        dashVel = Vector3.zero;
        lastDashVel = Vector3.zero;
        unitDetectionCamera.transform.localRotation = Quaternion.identity; //nollställ
        unitDetectionCamera.transform.localPosition = Vector3.zero;

        if (GetGrounded(groundCheckObject, 0.7f) && extraDashTime == 0) //extra cooldown för att man dashar från marken! FY PÅ DEJ!! (varit airbourne i X sekunder)if(Mathf.Abs(jumpTimePoint - Time.time) > 0.08f)
        { //använder extraDashTime och antar att det är när man dashar vidare från objekt, behöver en mer solid check kanske?
            int dashground_Punishment = 4;
            //currDashCombo += dashComboStacks_Punishment; //blir mega dyrt på nästa dash!
            powerManager.AddPower(-dashPowerCost * dashground_Punishment, 100, 5);
            //dashTimePoint += dashGroundCooldown;
        }

        //HÄMTA RIKTNINGEN
        dirMod = Vector3.zero;
        Camera checkCamera = unitDetectionCamera;
        useCameraDir = true;
        if (useCameraDir) //sker efter tex dashhit i fiende
        {
            dirMod = cameraHolder.forward; //fast denna kastas från staggen, hmm
            checkCamera = mainCamera;
            //Debug.Log("fast denna kastas från staggen, Använd den vanliga kameran då? :)");
            //if (ver < 0) //frammåt eller bakåt? kanske inte ska kunna åka bakåt?
            //{
            //    //Debug.Log(ver.ToString());
            //    dirMod = -cameraHolder.forward;
            //    dirMod.y = 0; //man vill inte åka upp/ned beroende på kamera vinkel
            //}
            //else
            //{
            //    //dirMod = Vector3.RotateTowards(stagObject.forward, cameraHolder.forward, 4000 * deltaTime, 0); //kanske göra nån check ifall man är vänd frammåt, annars vill man kanske INTE åka i kameran riktning, men inte säker
            //    dirMod = cameraHolder.forward;
            //}
        }
        else
        {
            checkCamera = unitDetectionCamera;
            if ((horVector + verVector).magnitude < 0.02f)
            {
                dirMod = stagObject.forward; //om ingen input så används stagObject.forward
            }
            else //jag har input
            {
                dirMod = (horVector + verVector).normalized;
                if (Vector3.Dot(dirMod, cameraHolder.forward) > 0.0f) //vända mot varandra
                {
                    dirMod.y = 0; //om ingen input så används stagObject.forward
                }
            }

            //if(ver < 0 && Mathf.Abs(hor) < 0.2f) //man dashar mot kameran, då vill man använda satt riktning i Y, annars kommer den få reversed som kameran
            //{
            //    dirMod.y = 0;
            //}

            Quaternion lookRotation = Quaternion.LookRotation(dirMod);
            unitDetectionCamera.transform.rotation = lookRotation; //så att man gör dashstyrningnstestet åt det hållet <<------ denna roterar fel ibland, vilket gör att den hittar skumma grejer / dirMod blir wack ?
        }

        float distanceCheck = 140; //denna kan vara lite överdriven, mest för att culla. Kanske inte längre

        //if (Vector3.SqrMagnitude(dirMod - cameraHolder.forward) < 0.0001) //kolla ifall de är samma vektor, isåfall vill jag flytta utgångspunkten
        //{
        //    Debug.Log("Detta är förmodligen ingen fungerande ide förtillfället, se över!");
        //    unitDetectionCamera.transform.position = cameraHolder.position;
        //    distanceCheck *= 1.5f; //då är man längre bak med kameran
        //}
        //HÄMTA RIKTNINGEN

        //***DASHSTYRNIG***
        Vector3 biasedDir = Vector3.zero; //styr den mot fiender
        dashTarget = null;

        Collider[] potTargets = Physics.OverlapSphere(transform.position, distanceCheck, unitCheckLM); //att hitta ett dashTarget borde kanske bara göras i början av dash?
        //float closestDistance = Mathf.Infinity;
        float closeDistanceThreshhold = 3; //ifall den är för nära så skit i det
        //float closestToMidValue = Mathf.Infinity;

        float bestFinalValue = -Mathf.Infinity; //det dashTarget med högst värde är den som väljs

        //Vector3 horVectorNoY = new Vector3(horVector.x, 0, horVector.z);
        //Vector3 verVectorNoY = new Vector3(verVector.x, 0, verVector.z);

        Vector3 currViewPlayerPos = checkCamera.WorldToViewportPoint(transform.position); //spelarens position i kamera spacet

        HealthSpirit hSpirit = null;
        for (int i = 0; i < potTargets.Length; i++)
        {
            //if (Vector3.Distance(transform.position, potTargets[i].transform.position) < minDistance) continue; //om den är för nära så hoppa vidare
            hSpirit = potTargets[i].GetComponent<HealthSpirit>();
            if (hSpirit == null || hSpirit.IsAlive() == false) continue;
            if (potTargets[i].transform == lastUnitHit) { continue; }//så man inte fastnar på infinite dash
            if(!hSpirit.isDashTarget) { continue; }

            Vector3 TToTar = ((hSpirit.MiddlePoint) - (transform.position + groundOffset)).normalized;
            Vector3 CToTar = (hSpirit.MiddlePoint - cameraHolder.position).normalized;

            Vector3 currViewPos = checkCamera.WorldToViewportPoint(hSpirit.MiddlePoint); //använder en kamera för att se ifall den ser några fiender!

            if (currViewPlayerPos.z > currViewPos.z) continue; //ligger mellan kameran o spelaren och då ska man inte dasha

            float currDistance = Vector3.Distance(transform.position, hSpirit.MiddlePoint); //jämför med den senaste outputen
            float currToMidValue = (Mathf.Abs(0.5f - currViewPos.x) + Mathf.Abs(0.5f - currViewPos.y)); //hur nära mitten är den? ju lägra destu närmre

            float currDistanceValue = currDistance / distanceCheck; //denna kommer bli pissliten
            float currFinalValue = (1 - currDistanceValue) + (1 - currToMidValue * 1.8f); //ska vara så högt som möjligt

            if (currFinalValue < bestFinalValue) continue; //fortsätt bara om denna är närmre mitten

            if (currDistance < closeDistanceThreshhold) continue;

            float minDistanceFromEdges = 0.3f; //hur nära kanten den max får vara
            float maxDistanceFromCenter = 0.3f;

            //if (currViewPos.x < (1.0f - minDistanceFromEdges) && currViewPos.x > (0.0f + minDistanceFromEdges) && currViewPos.y < (1.0f - minDistanceFromEdges) && currViewPos.y > (0.0f + minDistanceFromEdges) && currViewPos.z > 0.0f) //kolla så att den är innanför viewen
            Vector2 viewPosFromCenter = new Vector2(Mathf.Abs(0.5f - currViewPos.x), Mathf.Abs(0.5f - currViewPos.y));
            float hypoDistance = Mathf.Sqrt(Mathf.Pow(viewPosFromCenter.x, 2) + Mathf.Pow(viewPosFromCenter.y, 2));
            if (currViewPos.z > 0.0f && hypoDistance < maxDistanceFromCenter)
            {
                //Debug.Log(potTargets[i].name + " " + currFinalValue.ToString());
                RaycastHit rHit;

                //kolla så att ingen miljö är i vägen
                if (!Physics.Raycast(transform.position + groundOffset, TToTar, out rHit, currDistance-2, groundCheckLM)) //kolla så att den inte träffar någon miljö bara
                {
                    //if(lastUnitHit != null) sätts i StagSpeedBreaker
                    bestFinalValue = currFinalValue;
                    biasedDir = TToTar;
                    dashTarget = potTargets[i].transform;
                    dashTargetHS = hSpirit;
                    //bestDashTransform = potTargets[i].transform; //denna måste dock resettas efter en kort tid så att man återigen kan dasha på denna, detta bör göras när man kör en vanlig dash, dvs en som går på cd o liknande
                }
            }
        }

        //if (bestDashTransform != null)
        //{
        //    lastUnitHit = bestDashTransform; //behöver göras här utanför, vill inte ha massa mellanvärden
        //}
        //else
        //{
        //    lastUnitHit = null;
        //}
        //***DASHSTYRNIG***

        //SJÄLVSTYRNING, FAN VA ENKELT ALLT ÄR!!
        speedBreakerTimer = Time.time + speedBreakerTime; //speedbreakern aktiveras sedan i update
        speedBreaker.Activate(true);

        float minimumFinalValue = 0.0f; //måste vara högre än denna för det ska gå
        if (biasedDir != Vector3.zero && bestFinalValue > minimumFinalValue) //har en fiende hittats som ska styras mot
        {
            dirMod = biasedDir;
        }
        modDashSpeed = dashSpeed;
        SetDashVelocity(dirMod * modDashSpeed);
        startDashDir = dirMod; //sätts kanske bara en gång så att man kan återgå till ursprungs riktningen

        currDashTime = 0.0f;

        startDashTime = Time.time;
        extendedTime = 0.0f;

        currDashUpdates = 0 - (int)(extraDashTime * 70); //lägger till extraDashTime

        yield break;

        //yield return new WaitForSeconds(maxDashTime);
        //while (currDashTime < maxDashTime)
        //{
        //    currMomentum = Vector3.zero;
        //    if (isLocked) //ifall den låses så skall fortfarande dashen vara igång efter
        //    {
        //        extendedTime += Time.deltaTime * 1.7f; //vet inte varför den behöver multipliceras, det borde bli samma tid ändå som den förlorade
        //        yield return null;
        //        continue;
        //    }

        //speedBreakerTimer = Time.time + speedBreakerTime; //speedbreakern aktiveras sedan i update

        //    if (dashTarget != null)
        //    {
        //        dirMod = ((dashTarget.position + groundOffset + dashTargetOffset) - (transform.position + groundOffset)).normalized;
        //    }
        //    dashVel = dirMod * dashSpeed; //styra under dashen
        //    stagObject.transform.forward = dashVel;

        //    Vector3 hitNormal = Vector3.zero;
        //    if (!IsWalkable(1.0f, characterController.radius + 1.0f, dashVel, maxSlopeGrounded, ref hitNormal)) //så den slutar dasha när den går emot en vägg
        //    {
        //        BreakDash(false);
        //        Stagger(0.12f);
        //        yield break;
        //    }
        //    currDashTime = Time.time - startDashTime - extendedTime;
        //    yield return null;
        //}
        //ToggleDashEffect(false);
        //unitDetectionCamera.transform.localRotation = Quaternion.identity; //nollställ
        //unitDetectionCamera.transform.localPosition = Vector3.zero;

        //currMomentum = new Vector3(dashVel.x, 0, dashVel.z);
        //dashVel = Vector3.zero;
        //BreakDash(false);
        //Debug.Log(cameraObj.forward.ToString() + " " + dirMod.ToString() + "  " + biasedDir.ToString() + " " + lastUnitHit.ToString());
    }

    void DashFixedUpdate() //körs medans man dashar i FixedUpdate
    {
        if (currDashIE == null) return;

        if (currDashUpdates < dashUpdates)
        {
            currDashUpdates++;
            currMomentum = Vector3.zero;

            speedBreakerTimer = Time.time + speedBreakerTime; //speedbreakern aktiveras sedan i update

            bool closeToTarget = false;

            if (dashTarget != null) //om jag har dashTarget så lerpa endast
            {
                dirMod = ((dashTargetHS.MiddlePoint + groundOffset) - (transform.position + groundOffset)).normalized;
                SetDashVelocity(dirMod * modDashSpeed);

                transform.position = Vector3.Lerp(transform.position, dashTargetHS.MiddlePoint, (modDashSpeed / 40) * Time.fixedDeltaTime);

                if (Vector3.Distance(transform.position, dashTargetHS.MiddlePoint) < dashHelpDistance) //nästan där! skynda! så att man ska träffa mer frekvent och inte stanna precis innan
                {                   
                    SetDashVelocity(startDashDir * modDashSpeed); //man är för nära för att leta efter en ny velocity
                    closeToTarget = true;
                }
                else
                {
                    modDashSpeed = dashSpeed + currMovementStacks.value * 0.15f;
                    //startDashDir = dirMod;
                }
            }
            else
            {
                //modDashSpeed += (currMovementStacks.value * 0.1f); //lägger på extra dashspeed när man har högre stacks
                SetDashVelocity(dirMod * modDashSpeed); //styra under dashen
            }
            currDashTime = Time.time - startDashTime - extendedTime;

            if(!closeToTarget)
            {
                stagObject.transform.forward = dashVel.normalized;
            }

            //ySpeed = -gravity * 0.01f; //nollställer ej helt
            if (IsFaceplant(1.0f, dirMod, characterController.radius + 1.0f))
            {
                BreakDash(false);
                Stagger(0.12f, -dirMod * staggForce);
            }

            //Vector3 hitNormal = Vector3.zero;
            //if (!IsWalkable(1.0f, characterController.radius + 1.0f, dashVel, maxSlopeDash, ref hitNormal)) //så den slutar dasha när den går emot en vägg
            //{
            //    BreakDash(false);
            //    Stagger(0.12f);
            //}
        }
        else
        {
            BreakDash(false);
        }

    }

    void DashUpdate() //körs i vanliga update, för att få en mer frekvent check på stuff
    {
        if (currDashIE == null) return;

        if (currDashUpdates < dashUpdates)
        {
            bool closeToTarget = false;
            if (dashTarget != null)
            {                
                if (Vector3.Distance(transform.position, dashTargetHS.MiddlePoint) < dashHelpDistance) //nästan där! skynda! så att man ska träffa mer frekvent och inte stanna precis innan
                {
                    closeToTarget = true;
                }
            }

            if (!closeToTarget)
            {
                stagObject.transform.forward = dashVel.normalized;
            }

            if(IsFaceplant(1.0f, dirMod, characterController.radius + 1.0f))
            {
                BreakDash(false);
                Stagger(0.12f, -dirMod * staggForce);
            }
            //Vector3 hitNormal = Vector3.zero;
            //if (!IsWalkable(1.0f, characterController.radius + 1.0f, dashVel, maxSlopeDash, ref hitNormal)) //så den slutar dasha när den går emot en vägg
            //{
            //    BreakDash(false);
            //    Stagger(0.12f);
            //}
        }
    }

    void SetDashVelocity(Vector3 newDashVel) //så man sätter lastdashvel oxå
    {
        dashVel = newDashVel;
        lastDashVel = dashVel;
    }

    void CenterRectangle(ref Rect someRect)
    {
        someRect.x = ( Screen.width  - someRect.width ) / 2;
        someRect.y = ( Screen.height - someRect.height ) / 2;
    }

    public void IgnoreCollider(float duration, Transform t_Ignore)
    {
        StartCoroutine(DoIgnoreCollider(duration, t_Ignore));
    }

    public void IgnoreCollider(bool ignore, Transform t_Ignore)
    {
        if (t_Ignore == null) return;
        Physics.IgnoreCollision(transform.GetComponent<Collider>(), t_Ignore.GetComponent<Collider>(), ignore);
        Physics.IgnoreCollision(speedBreaker.GetComponent<Collider>(), t_Ignore.GetComponent<Collider>(), ignore);
    }

    IEnumerator DoIgnoreCollider(float duration, Transform t_Ignore)
    {
        float startTime = Time.time;
        float extendedTime = 0.0f;
        Physics.IgnoreCollision(transform.GetComponent<Collider>(), t_Ignore.GetComponent<Collider>(), true);
        Physics.IgnoreCollision(speedBreaker.GetComponent<Collider>(), t_Ignore.GetComponent<Collider>(), true);
        while ((Time.time - startTime - extendedTime) < duration)
        {
            if (isLocked || isCCed) //ifall den låses så skall fortfarande vara igång efter
            {
                extendedTime += Time.deltaTime;
                yield return null;
                continue;
            }
            yield return null;
        }
        Physics.IgnoreCollision(transform.GetComponent<Collider>(), t_Ignore.GetComponent<Collider>(), false);
        Physics.IgnoreCollision(speedBreaker.GetComponent<Collider>(), t_Ignore.GetComponent<Collider>(), false);
    }

    public IEnumerator DoIgnoreLayer(float duration, int layer1, int layer2)
    {
        float startTime = Time.time;
        float extendedTime = 0.0f;
        Physics.IgnoreLayerCollision(layer1, layer2, true);
        while ((Time.time - startTime - extendedTime) < duration)
        {
            if (isLocked || isCCed) //ifall den låses så skall fortfarande vara igång efter
            {
                extendedTime += Time.deltaTime;
                yield return null;
                continue;
            }
            yield return null;
        }
        Physics.IgnoreLayerCollision(layer1, layer2, false);
    }

    public void IgnoreLayer(int layer1, int layer2, bool ignore)
    {
        Physics.IgnoreLayerCollision(layer1, layer2, ignore);
    }


    int CalculateHiddenStacks()
    {
        float dec_multiplier = 18;
        int minStacks = minSoftMovementStacks;

        if(realMovementStacks.value < minStacks)
        {
            minStacks = realMovementStacks.value;
        }

        int newHiddenStacks = realMovementStacks.value - (int)((GetGroundedDuration() * dec_multiplier));
        newHiddenStacks = Mathf.Max(minStacks, newHiddenStacks);
        return newHiddenStacks;
    }

    public void AddMovementStack(int i)
    {
        if(i > 0 && realMovementStacks.value < minSoftMovementStacks) //lättare att få stacks i början
        {
            i *= 2;
        }
        realMovementStacks.value += i;

        if (realMovementStacks.value < 1)
        {
            realMovementStacks.value = 1;
        }

        float timeReduceValue = Mathf.Max(0, Mathf.Pow(realMovementStacks.value, 1.4f)); //ju mindre upphöjt värde ju högra kommer den öka
        float groundedReduceValue = Mathf.Max(0, Mathf.Pow(realMovementStacks.value, 1.7f)); //den ska börja litet o bli större o större
        timeReduceValue *= 0.013f;
        groundedReduceValue *= 0.04f;
        if (float.IsNaN(timeReduceValue))
        {
            timeReduceValue = 0;
        }
        if (float.IsNaN(groundedReduceValue))
        {
            groundedReduceValue = 0;
        }
        //Debug.Log(groundedReduceValue.ToString());
        movementStackResetTimer = ingame_Realtime + movementStackResetTime - (timeReduceValue); //gör det svårare o svårare!
        movementStackGroundedTimer = GetGroundedDuration() + movementStackGroundedTime - (groundedReduceValue); //inte plus tid för den ligger redan inräknad

        //lite minimum värden, så man kan stacka högt
        //Debug.Log((timeReduceValue).ToString());
        float minimumTime = 0.4f;
        int softLimitStacks = 25;

        if(realMovementStacks.value > softLimitStacks)
        {
            minimumTime = 0.3f;
        }

        movementStackResetTimer = Mathf.Max(minimumTime + ingame_Realtime, movementStackResetTimer); //ska som minst vara x sekunder
        movementStackGroundedTimer = Mathf.Max(minimumTime, movementStackGroundedTimer); //ska som minst vara x sekunder

        //Debug.Log(currMovementStacks.value.ToString() + "  " + (movementStackResetTimer - Time.time).ToString());
        //Debug.Log(currMovementStacks.value.ToString() + "  " + (movementStackGroundedTimer).ToString());

        //moveStackText.text = currMovementStacks.value.ToString();
    }


    public virtual void PlayAnimationStates()
    {
        if (animationH == null) return;
        float fadeLengthA = 0.1f;

        if (characterController.isGrounded || GetGrounded(groundCheckObject))
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
            if (ySpeed > 0.01f)
            {
                animationH.CrossFade(jump.name, fadeLengthA);
            }
            else
            {
                animationH.CrossFade(idleAir.name, fadeLengthA);
            }
        }
    }

    public virtual void ApplyYForce(float velY) //till characterscontrollern, inte rigidbody
    {
        jumpTimePoint = Time.time;
        ySpeed += velY;
    }
    public virtual void ApplyYForce(float velY, float maxVel) //till characterscontrollern, inte rigidbody, med ett max värde
    {
        if (ySpeed >= maxVel) return;
        jumpTimePoint = Time.time;
        ySpeed += velY;
    }

    public virtual void ApplyExternalForce(Vector3 moveDir, bool resetForces = false)
    {
        if(resetForces)
        {
            ySpeed = 0;
            currMomentum = Vector3.zero;
            BreakDash();
        }
        externalVel = moveDir;
    }

    public virtual void InitJumpEffects()
    {
        Transform jumpEffectObjectTransform = jumpEffectObject.GetComponent<Transform>();
        int childCount = jumpEffectObjectTransform.childCount;
        ParticleTimed[] childSystems = new ParticleTimed[childCount];
        int numberOfSystems = 0;
        for (int i = 0; i < jumpEffectObjectTransform.childCount; i++)
        {
            ParticleTimed timedSystem = jumpEffectObjectTransform.GetChild(i).GetComponent<ParticleTimed>();
            if(timedSystem != null)
            {
                childSystems[i] = timedSystem;
                numberOfSystems++;
            } 
        }
        if(numberOfSystems > 0)
        {
            jumpEffects = new ParticleTimed[numberOfSystems];
            for(int i = 0; i<numberOfSystems; i++)
            {
                jumpEffects[i] = childSystems[i];
            }
        }
    }

    public virtual void PlayJumpEffect()
    {
        if (gameObject.activeSelf == false) return;

        AudioSource dAS = jumpEffectObject.GetComponent<AudioSource>();

        ParticleTimed psTimed = jumpEffectObject.GetComponentInChildren<ParticleTimed>();

        if (dAS != null)
        {
            dAS.Play();
        }

        if (jumpEffects.Length > 0)
        {
            for(int i = 0; i < jumpEffects.Length; i++)
            {
                    jumpEffects[i].StartParticleSystem(isGroundedRaycast);
            }
            
        }
        
    }

    public virtual void ToggleDashEffect(bool b)
    {
        if (gameObject.activeSelf == false) return;
        dashEffectObject.transform.rotation = cameraHolder.rotation;
        float trailOriginalTime = 0.05f;
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

        for(int i = 0; i < tR.Length; i++)
        {
            if(b)
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

        for(int i = 0; i < pS.Length; i++)
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
    public virtual IEnumerator ShutDownTrail(TrailRenderer tR)
    {
        while(tR.time > 0.0f)
        {
            tR.time -= 3 * deltaTime;
            tR.startWidth -= deltaTime;
            tR.endWidth -= deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public virtual void ApplySpeedMultiplier(float multiplier, float duration) //slows o liknande, updateras i LateUpdate()
    {
        currExternalSpeedMult = multiplier;
        moveSpeedMultTimePoint = Time.time;
        moveSpeedMultDuration = duration;
    }

    //void ToggleInfiniteGravity(bool b)
    //{
    //    pullField.enabled = b;
    //    ParticleSystem pullps = pullField.gameObject.GetComponent<ParticleSystem>();

    //    //pullps.emission.enabled = b;

    //    if (b)
    //    {
    //        pullps.Play();
    //    }
    //    else
    //    {
    //        pullps.Stop();
    //    }
    //}

    public virtual void ToggleDashReadyPS(bool b)
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

    public virtual bool IsWalkable(float yOffset, float distance, Vector3 direction, float maxSlope)
    {
        RaycastHit rHit;
        if(Physics.Raycast(transform.position + new Vector3(0, yOffset, 0), direction, out rHit, distance, groundCheckLM))
        {
            //Vector3 dirNoY = new Vector3(direction.x, 0, direction.y);
            //Vector3 normNoY = new Vector3(rHit.normal.x, 0, rHit.normal.z);

            float angleValue = Vector3.Angle(rHit.normal, Vector3.up);
            //Debug.Log(angleValue.ToString());

            if (angleValue > maxSlope)
            {
                return false;
            }

        }
        return true;
    }

    public virtual bool IsWalkable(float yOffset, float distance, Vector3 direction, float maxSlope, ref Vector3 hitNormal) //man kan få tillbaks väggens normal
    {
        RaycastHit rHit;
        if (Physics.Raycast(transform.position + new Vector3(0, yOffset, 0), direction, out rHit, distance, groundCheckLM))
        {
            //Vector3 dirNoY = new Vector3(direction.x, 0, direction.y);
            //Vector3 normNoY = new Vector3(rHit.normal.x, 0, rHit.normal.z);

            hitNormal = rHit.normal;
            float angleValue = Vector3.Angle(rHit.normal, Vector3.up);
            //Debug.Log(angleValue.ToString());

            if (angleValue > maxSlope)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsFaceplant(float yOffset, Vector3 direction, float distance, float threshholdValue = dotValueFaceplant)
    {
        RaycastHit rHit;
        if (Physics.Raycast(transform.position + new Vector3(0, yOffset, 0), direction, out rHit, distance, groundCheckLM))
        {
            if(Vector3.Dot(rHit.normal, direction) < threshholdValue)
            {
                return true;
            }
        }
        return false;
    }

    //de riktiga checkarna!
    public bool GetGrounded(Vector3 checkerPoint, float distance = groundedCheckDistance) //från en annan utgångspunkt
    {
        RaycastHit rHit;
        bool isGHit = false;

        if (Physics.SphereCast(checkerPoint, characterController.radius, Vector3.down, out rHit, distance, groundCheckLM))
        {
            groundedSlope = GetSlope(rHit.normal);
            groundedNormal = rHit.normal;

            if (rHit.transform == this.transform || rHit.normal.y < 0.5f) { isGHit = false; } //MEH DEN SKA EJ COLLIDA MED SIG SJÄLV
            else if (groundedSlope > maxSlopeGrounded) { isGHit = false; }
            else
            {
                groundedRaycastObject = rHit.transform;
                isGHit = true;
            }
        }
        else
        {
            isGHit = false;
        }

        if (!isGHit)
        {
            isGHit = GetGroundedSingleRay(checkerPoint); //kolla från en single ray oxå istället för spherecast
        }

        if (isGroundedRaycast == false) //om man inte var grounded innan
        {
            if (isGHit)
            {
                groundedTimePoint = Time.time;
            }
        }

        isGroundedRaycast = isGHit;

        return isGHit;
    }
    public bool GetGroundedSingleRay(Vector3 checkerPoint, float distance = groundedCheckDistance)
    {
        RaycastHit rHit;
        if (Physics.Raycast(checkerPoint, Vector3.down, out rHit, distance, groundCheckLM))
        {
            if (rHit.transform == this.transform || rHit.normal.y < 0.5f) { return false; } //MEH DEN SKA EJ COLLIDA MED SIG SJÄLV

            groundedSlope = GetSlope(rHit.normal);
            groundedNormal = rHit.normal;

            if (groundedSlope > maxSlopeGrounded) { return false; }

            groundedRaycastObject = rHit.transform;
            return true;
        }
        else
        {
            return false;
        }
    }
    //de riktiga checkarna!

    public bool GetGrounded()
    {
        Vector3 checkerPoint = transform.position + characterController.center + new Vector3(0, groundedCheckOffsetY, 0);

        return GetGrounded(checkerPoint);
    }

    public bool GetGrounded(Transform tChecker) //från en annan utgångspunkt
    {
        return GetGrounded(tChecker.position);
    }

    public bool GetGrounded(Transform tChecker, float distance) //från en annan utgångspunkt och med en specifik längd
    {
        return GetGrounded(tChecker.position, distance);
    }

    public virtual bool GetGrounded(float distance) //från en annan utgångspunkt och med en specifik längd
    {
        Vector3 checkerPoint = transform.position + characterController.center + new Vector3(0, groundedCheckOffsetY, 0);
        return GetGrounded(checkerPoint, distance);
    }


    public virtual Transform GetGroundedTransform(Transform tChecker) //får den transformen man står på, från en annan utgångspunkt
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

    public virtual Transform GetGroundedTransform(Transform tChecker, float distance) //får den transformen man står på, från en annan utgångspunkt
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

    public float GetSlope(Vector3 normalSurface)
    {
        if(normalSurface.y > 0.5f) //normalen är uppåt
        {
            return(Vector3.Angle(Vector3.down, -normalSurface));
        }
        else //normalen är neråt
        {
            return (Vector3.Angle(Vector3.down, normalSurface));
        }
    }

    public float GetGroundedDuration()
    {
        if (!GetGrounded())
            return 0;
        //if (Time.time - groundedTimePoint > 2)
        //    Debug.Log((Time.time - groundedTimePoint).ToString());
        return Time.time - groundedTimePoint;
    }

    public float GetTimeSinceLastDash()
    {
        float t = Time.time - dashTimePoint;
        return t;
    }

    public float GetDistanceToGround()
    {
        Vector3 checkerPoint = transform.position + characterController.center + new Vector3(0, groundedCheckOffsetY, 0); //viktigt att den här default funktionen använder samma som i GetGrounded's default
        return GetGroundedDistance(checkerPoint);
    }

    public float GetDistanceToGround(Transform tChecker)
    {
        return GetGroundedDistance(tChecker.position);
    }

    public float GetGroundedDistance(Vector3 checkerPoint)
    {
        RaycastHit rHit;
        if (Physics.Raycast(checkerPoint, Vector3.down, out rHit, 1000000, groundCheckLM))
        {
            return Vector3.Distance(checkerPoint, rHit.point);
        }
        else
        {
            return Mathf.Infinity;
        }
    }
}