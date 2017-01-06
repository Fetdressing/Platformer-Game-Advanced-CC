using UnityEngine;
using System.Collections;
using InControl;

public class ControlManager : MonoBehaviour {

    MyCharacterActions characterActions;

    [HideInInspector]
    public bool didJump;
    [HideInInspector]
    public bool didDash;

    [HideInInspector]
    public float horAxis = 0;
    [HideInInspector]
    public float verAxis = 0;

    [HideInInspector]
    public float horAxisView = 0;
    [HideInInspector]
    public float verAxisView = 0;

    void Start()
    {
        characterActions = new MyCharacterActions();
        //move
        characterActions.Left.AddDefaultBinding(Key.A);
        characterActions.Left.AddDefaultBinding(InputControlType.DPadLeft);
        characterActions.Left.AddDefaultBinding(InputControlType.LeftStickLeft);

        characterActions.Right.AddDefaultBinding(Key.D);
        characterActions.Right.AddDefaultBinding(InputControlType.DPadRight);
        characterActions.Right.AddDefaultBinding(InputControlType.LeftStickRight);

        characterActions.Forward.AddDefaultBinding(Key.W);
        characterActions.Forward.AddDefaultBinding(InputControlType.DPadUp);
        characterActions.Forward.AddDefaultBinding(InputControlType.LeftStickUp);

        characterActions.Back.AddDefaultBinding(Key.S);
        characterActions.Back.AddDefaultBinding(InputControlType.DPadDown);
        characterActions.Back.AddDefaultBinding(InputControlType.LeftStickDown);
        //move

        //view
        characterActions.LeftView.AddDefaultBinding(InputControlType.RightStickLeft);
        characterActions.LeftView.AddDefaultBinding(Mouse.NegativeX);

        characterActions.RightView.AddDefaultBinding(InputControlType.RightStickRight);
        characterActions.RightView.AddDefaultBinding(Mouse.PositiveX);

        characterActions.ForwardView.AddDefaultBinding(InputControlType.RightStickUp);
        characterActions.ForwardView.AddDefaultBinding(Mouse.PositiveY);

        characterActions.BackView.AddDefaultBinding(InputControlType.RightStickDown);
        characterActions.BackView.AddDefaultBinding(Mouse.NegativeY);
        //view

        characterActions.Jump.AddDefaultBinding(Key.Space);
        characterActions.Jump.AddDefaultBinding(InputControlType.Action1);
        characterActions.Jump.AddDefaultBinding(InputControlType.LeftTrigger);
        characterActions.Jump.AddDefaultBinding(InputControlType.LeftBumper);

        characterActions.Dash.AddDefaultBinding(Mouse.LeftButton);
        characterActions.Dash.AddDefaultBinding(InputControlType.RightTrigger);
        characterActions.Dash.AddDefaultBinding(InputControlType.RightBumper);
    }

    void Update()
    {
        didJump = false;
        didDash = false;

        if (characterActions.Jump.WasPressed)
        {
            didJump = true;
        }

        if (characterActions.Dash.WasPressed)
        {
            didDash = true;
        }

        // We use the aggregate filter action here.
        // It combines Left and Right into a single axis value
        // in the range -1 to +1.
        horAxis = characterActions.MoveHor.Value;
        verAxis = characterActions.MoveVer.Value;

        horAxisView = characterActions.ViewHor.Value;
        verAxisView = characterActions.ViewVer.Value;
    }
}

public class MyCharacterActions : PlayerActionSet
{
    public PlayerAction Left;
    public PlayerAction Right;
    public PlayerAction Forward;
    public PlayerAction Back;
    public PlayerOneAxisAction MoveHor;
    public PlayerOneAxisAction MoveVer;

    public PlayerAction LeftView;
    public PlayerAction RightView;
    public PlayerAction ForwardView;
    public PlayerAction BackView;
    public PlayerOneAxisAction ViewHor;
    public PlayerOneAxisAction ViewVer;

    public PlayerAction Jump;
    public PlayerAction Dash;

    public MyCharacterActions()
    {
        Left = CreatePlayerAction("Move Left");
        Right = CreatePlayerAction("Move Right");
        Forward = CreatePlayerAction("Move forward");
        Back = CreatePlayerAction("Move back");
        MoveHor = CreateOneAxisPlayerAction(Left, Right);
        MoveVer = CreateOneAxisPlayerAction(Back, Forward);

        LeftView = CreatePlayerAction("View Left");
        RightView = CreatePlayerAction("View Right");
        ForwardView = CreatePlayerAction("View forward");
        BackView = CreatePlayerAction("View back");
        ViewHor = CreateOneAxisPlayerAction(LeftView, RightView);
        ViewVer = CreateOneAxisPlayerAction(BackView, ForwardView);

        Jump = CreatePlayerAction("Jump");
        Dash = CreatePlayerAction("Dash");
    }
}
