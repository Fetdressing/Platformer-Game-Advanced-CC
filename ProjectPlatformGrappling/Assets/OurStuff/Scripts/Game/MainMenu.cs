using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    private string selectedScene = "Scene1SpiritWorld";
	// Use this for initialization
	void Start () {
	
	}


    public void StartGame(string sceneName)
    {
        LevelLoader lLoader = FindObjectOfType(typeof(LevelLoader)) as LevelLoader;
        lLoader.LoadLevel(sceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
