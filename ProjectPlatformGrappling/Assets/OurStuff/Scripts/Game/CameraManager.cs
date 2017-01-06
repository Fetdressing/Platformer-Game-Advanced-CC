using UnityEngine;
using System.Collections;

public class CameraManager : BaseClass {
    public Camera cameraPlayerFollow;
    [HideInInspector]
    public Camera currCamera;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        currCamera = cameraPlayerFollow;
    }

}
