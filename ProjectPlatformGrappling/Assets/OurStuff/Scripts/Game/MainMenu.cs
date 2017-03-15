using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    LevelManager levelManager;

    private string selectedScene = "Scene1SpiritWorld";
	// Use this for initialization
	void Start () {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
	}


    public void StartGame(string sceneName)
    {
        StartGame();
        
        //LevelLoader lLoader = FindObjectOfType(typeof(LevelLoader)) as LevelLoader;
        //lLoader.LoadLevel(sceneName);
    }

    public void StartGame()
    {
        LevelLoader lLoader = FindObjectOfType(typeof(LevelLoader)) as LevelLoader;
        lLoader.LoadLevel(levelManager.standardLevels[0]);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
