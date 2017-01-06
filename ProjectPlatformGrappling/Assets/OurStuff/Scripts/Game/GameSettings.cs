using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DigitalRuby.SimpleLUT;

public class GameSettings : BaseClass {
    public GameObject settingsPanel;

    public Slider gammaSlider;
    public Slider mouseSpeedSlider;

    private string mouseSpeedSaveName = "MouseSpeed";
    private string gammaSaveName = "Gamma";

    private float startGamma;

    private float currGamma;
    private float currMouseSpeed;

    private WoWCCamera wowcCamera;
    private Camera mCamera;
    private SimpleLUT simpleLUT;

	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();

        mCamera = GameObject.FindGameObjectWithTag("Manager").GetComponent<CameraManager>().cameraPlayerFollow;
        wowcCamera = GameObject.FindGameObjectWithTag("MainCamera").transform.GetComponent<WoWCCamera>();
        simpleLUT = mCamera.GetComponent<SimpleLUT>();
        
        startGamma = 0;

        //om det är första gången man startar så set värdena till nått värde
        float gCheck = PlayerPrefs.GetFloat(gammaSaveName);
        if(gCheck == 0) { gCheck = 0.3f; }
        else
        {
            startGamma = simpleLUT.Brightness;
        }

        float msCheck = PlayerPrefs.GetFloat(mouseSpeedSaveName);
        if (msCheck == 0) { msCheck = 0.3f; }

        SetGamma(gCheck); //hämta från playerprefs
        SetMouseSpeed(msCheck);
        
        mouseSpeedSlider.value = currMouseSpeed;

        gammaSlider.onValueChanged.AddListener(delegate { GammaSliderChange(); });
        mouseSpeedSlider.onValueChanged.AddListener(delegate { MouseSpeedSlider(); });
    }

    // Update is called once per frame
 //   void Update () {
	//    if(Input.GetKeyDown(KeyCode.Escape))
 //       {
 //           ToggleGameSettingsPanel(!settingsPanel.activeSelf);
 //       }
	//}

    void GammaSliderChange()
    {
        float value = gammaSlider.value;
        SetGamma(value);
    }

    void MouseSpeedSlider()
    {
        float value = mouseSpeedSlider.value;
        SetMouseSpeed(value);
    }

    //public void ToggleGameSettingsPanel(bool b) //denna ska kanske inte modda tiden sen heh :)
    //{
    //    if(b)
    //    {
    //        settingsPanel.SetActive(true);
    //    }
    //    else
    //    {
    //        settingsPanel.SetActive(false);
    //    }
    //}

    public void SetGamma(float f)
    {
        currGamma = f;
        gammaSlider.value = currGamma;
        //RenderSettings.ambientLight = new Color(startGamma.r + currGamma, startGamma.g + currGamma, startGamma.b + currGamma, startGamma.a); //förmodligen använda *
        simpleLUT.Brightness = startGamma + currGamma; //förmodligen använda *
        PlayerPrefs.SetFloat(gammaSaveName, currGamma);
    }

    public void SetMouseSpeed(float f)
    {
        currMouseSpeed = f;
        mouseSpeedSlider.value = currMouseSpeed;
        wowcCamera.speedMultiplier = currMouseSpeed * 2; //lite snabbare ska den kunna vara
        PlayerPrefs.SetFloat(mouseSpeedSaveName, currMouseSpeed);
    }
}
