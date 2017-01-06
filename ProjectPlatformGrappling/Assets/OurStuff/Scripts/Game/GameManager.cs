using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : BaseClass {
    public GameObject gameUIPanel;
    public GameObject menuUIPanel;
    public GameObject settingsUIPanel;

    private Transform player;
    public Transform goal;
    public GameObject goalDisplay; //aktiveras när man går i mål
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        goalDisplay.SetActive(false);
        ToggleMenu(false);
    }

    // Update is called once per frame
    void Update () {
        deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);

        if (isLocked) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        if(Vector3.Distance(player.position, goal.position) < 45)
        {
            goalDisplay.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
        }
	}

    void ToggleMenu()
    {
        bool b;
        if(menuUIPanel.activeSelf == true || settingsUIPanel.activeSelf == true)
        {
            b = false;
        }
        else
        {
            b = true;
        }

        if(b)
        {
            Time.timeScale = 0;
            ToggleUI(menuUIPanel);
        }
        else
        {
            Time.timeScale = 1;
            ToggleUI(gameUIPanel);
        }
    }

    void ToggleMenu(bool b)
    {

        if (b)
        {
            Time.timeScale = 0;
            ToggleUI(menuUIPanel);
        }
        else
        {
            Time.timeScale = 1;
            ToggleUI(gameUIPanel);
        }
    }

    public void ToggleUI(GameObject uiM)
    {
        if (isLocked) return;

        if (uiM.gameObject == gameUIPanel.gameObject) { gameUIPanel.SetActive(true); Cursor.visible = false; Cursor.lockState = UnityEngine.CursorLockMode.Locked; } else { gameUIPanel.SetActive(false); Cursor.visible = true; Cursor.lockState = UnityEngine.CursorLockMode.None; }
        if (uiM.gameObject == settingsUIPanel.gameObject) settingsUIPanel.SetActive(true); else settingsUIPanel.SetActive(false);
        if (uiM.gameObject == menuUIPanel.gameObject) menuUIPanel.SetActive(true); else menuUIPanel.SetActive(false);
    }

    public void RestartLevel()
    {
        if (isLocked) return;
        LevelLoader lLoader = FindObjectOfType(typeof(LevelLoader)) as LevelLoader;
        string sceneToLoad = SceneManager.GetActiveScene().name;
        lLoader.LoadLevel(sceneToLoad);
    }

    public void ExitGame()
    {
        if (isLocked) return;
        Application.Quit();
    }
}
