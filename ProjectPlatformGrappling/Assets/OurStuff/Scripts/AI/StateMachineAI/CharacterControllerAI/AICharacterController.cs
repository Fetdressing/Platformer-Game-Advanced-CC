using UnityEngine;
using System.Collections;

public class AICharacterController : AIMoveable {
    protected CharacterController cController;

    public override void Init()
    {
        base.Init();
        cController = transform.GetComponent<CharacterController>();
    }

    public override void Move(Vector3 pos, float speed)
    {

    }

}
