
var target : Transform;
var currSpeed = 0.0f;
private var currFrameTargetPos = new Vector3();
private var lastFrameTargetPos = new Vector3();
 
var targetHeight = 12.0;
var distance = 5.0;
var extraDistance = 0.0f;
 
var maxDistance = 20;
var minDistance = 2.5;
 
var xSpeed = 250.0;
var ySpeed = 120.0;
 
var yMinLimit = -20;
var yMaxLimit = 80;
 
var zoomRate = 20;
 
var rotationDampening = 3.0;
 
var theta2 : float = 0.5;
 
private var x = 0.0;
private var y = 0.0;
 
private var fwd = new Vector3();
private var rightVector = new Vector3();
private var upVector = new Vector3();
private var movingVector = new Vector3();
private var collisionVector = new Vector3();
private var isColliding : boolean = false;
   
private var a1 = new Vector3();
private var b1 = new Vector3();
private var c1 = new Vector3();
private var d1 = new Vector3();
private var e1 = new Vector3();
private var f1 = new Vector3();
private var h1 = new Vector3();
private var i1 = new Vector3();
 
@script AddComponentMenu("Camera-Control/WoW Camera")
 
function Start () {
    var angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;
 
    // Make the rigid body not change rotation
    if (GetComponent.<Rigidbody>())
        GetComponent.<Rigidbody>().freezeRotation = true;

}
 
function LateUpdate () {
    if(!target)
        return;

    currFrameTargetPos = target.position;
    if(lastFrameTargetPos != Vector3.zero)
    {
        var valueSpeed = Mathf.Abs(Mathf.Abs(lastFrameTargetPos.magnitude) - Mathf.Abs(currFrameTargetPos.magnitude));
        extraDistance = Mathf.Lerp(extraDistance, valueSpeed, Time.deltaTime * 10f);;
    }
    else
    {
        extraDistance = Mathf.Lerp(extraDistance, 0, Time.deltaTime * 10f);
    }


    x += Input.GetAxis("Mouse X") * xSpeed * 0.02;
    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02;

   
    distance -= (Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime) * zoomRate * Mathf.Abs(distance);
    distance = Mathf.Clamp(distance, minDistance, maxDistance);
   
    y = ClampAngle(y, yMinLimit, yMaxLimit);
   
    var rotation:Quaternion = Quaternion.Euler(y, x, 0);
    var position = target.position - (rotation * Vector3.forward * (distance + extraDistance) + Vector3(0,-targetHeight,0));
   
    //// Check to see if we have a collision
    //collisionVector = AdjustLineOfSight(transform.position, position);
   
    //// Check Line Of Sight
    //if (collisionVector != Vector3.zero)
    //{
    //    Debug.Log("Check Line Of Sight");
    //    a1 = transform.position;
    //    b1 = position;
    //    c1 = AdjustLineOfSight(transform.position, position);
    //    d1 = c1 - a1;
    //    e1 = d1.normalized * -1;
    //    f1 = d1 + e1 * 1;
    //    g1 = f1 + a1;
    //    position = g1;
 
    //    // check distance player to camera
    //    h1 = position - a1;
    //    if (h1.magnitude < 10)
    //    {
    //        position = a1 - fwd * 4;
    //        //position.y = targetPlayer.y;
    //        theta2 = theta2 + .25;
    //    }
       
    //    // set new camera distance
    //    h1 = position - a1;
    //    distance = h1.magnitude;
    //}
 
    //// check collision
    //if (Physics.CheckSphere (position, .5) )
    //{
    //    a1 = transform.position;
       
    //    newPosition = a1 - fwd * 4;
    //    //newPosition.y = targetPlayer.y;
    //    theta2 = theta2 + .25;
       
    //    // set new camera distance
    //    h1 = position - a1;
    //    distance = h1.magnitude;
    //}  
   
    //position = Vector3.Slerp(transform.position, position, Time.deltaTime * 100);
   
    transform.rotation = rotation;
    transform.position = position;

    lastFrameTargetPos = target.position;
}
 
static function ClampAngle (angle : float, min : float, max : float) {
    if (angle < -360)
        angle += 360;
    if (angle > 360)
        angle -= 360;
    return Mathf.Clamp (angle, min, max);
}
 
    function AdjustLineOfSight (vecA: Vector3, vecB: Vector3)
        {
            var hit: RaycastHit;
     
            if (Physics.Linecast (vecA, vecB, hit))
            {
                Debug.Log("I hit something");
                return hit.point;
            }
     
            return Vector3.zero;
        }