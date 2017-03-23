using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : BaseClass {
    LevelManager levelManager;
    ScoreManager scoreManager;

    public GameObject gameUIPanel;
    public GameObject menuUIPanel;
    public GameObject settingsUIPanel;

    private Transform player;
    public Transform goal;
    public GameObject goalDisplay; //aktiveras när man går i mål

    float lastTimeScale = 1.0f; //spara ned den timescalen som var innan man körde igång menyn
    bool lastFrame_was_Paused = false;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        scoreManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ScoreManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        goalDisplay.SetActive(false);
        ToggleMenu(false);
    }

    void FixedUpdate()
    {
        lastFixedUpdate_Timepoint = Time.time;
    }

    // Update is called once per frame
    void Update () {

        if (!lastFrame_was_Paused)
        {
            deltaTime = Mathf.Min(Time.deltaTime, maxDeltaTime);
            deltaTime_Unscaled = Mathf.Min(Time.unscaledDeltaTime, maxDeltaTime);
        }
        else
        {
            deltaTime_Unscaled = 0;
            deltaTime = 0;
        }

        if (Time.timeScale > 0.0001f)
        {
            ingame_Realtime += deltaTime / Time.timeScale;
        }

        if (isLocked) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }

        if(Vector3.Distance(player.position, goal.position) < 90)
        {
            Win();
        }

        if(Time.timeScale < 0.0001f)
        {
            lastFrame_was_Paused = true;
        }
        else
        {
            lastFrame_was_Paused = false;
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
            lastTimeScale = Time.timeScale;
            Time.timeScale = 0;
            ToggleUI(menuUIPanel);
        }
        else
        {
            Time.timeScale = lastTimeScale;
            ToggleUI(gameUIPanel);
        }
    }

    void ToggleMenu(bool b)
    {

        if (b)
        {
            lastTimeScale = Time.timeScale;
            Time.timeScale = 0;
            ToggleUI(menuUIPanel);
        }
        else
        {
            Time.timeScale = lastTimeScale;
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
        //string sceneToLoad = SceneManager.GetActiveScene().name;
        lLoader.LoadLevel(lLoader.GetCurrLevel());
    }

    public void ExitGame()
    {
        if (isLocked) return;
        LevelLoader lLoader = FindObjectOfType(typeof(LevelLoader)) as LevelLoader;
        //lLoader.LoadNextLevel();
        //return;

        SaveGame();
        lLoader.LoadMainMenu();
        //Application.Quit();
    }

    public void SaveGame()
    {
        print("Saving game");
        LevelLoader lLoader = FindObjectOfType(typeof(LevelLoader)) as LevelLoader;
        //lLoader.LoadNextLevel();
        //return;
        Level ll = lLoader.GetCurrLevel();
        ll.bestGlobesCollected = scoreManager.GetBestGlobesCollected(ll);
        //ll.bestTime = scoreManager.GetBestTime();
        levelManager.SetDataLevel(ll.levelIndex, ll);
    }

    public void Win() //sprang i mål
    {
        goalDisplay.SetActive(true);
        Time.timeScale = 0;
        Cursor.visible = true;

        LevelLoader lLoader = FindObjectOfType(typeof(LevelLoader)) as LevelLoader;
        //lLoader.LoadNextLevel();
        //return;
        Level ll = lLoader.GetCurrLevel();
        ll.bestGlobesCollected = scoreManager.GetBestGlobesCollected(ll);

        bool currIsBest = false;
        ll.bestTime = scoreManager.GetBestTime(ll, ref currIsBest);

        SaveGame();
    }
}
