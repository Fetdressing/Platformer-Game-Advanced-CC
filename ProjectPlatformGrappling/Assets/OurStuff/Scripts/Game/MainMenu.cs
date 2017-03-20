using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
    LevelManager levelManager;

    private string selectedScene = "Scene1SpiritWorld";

    public MenuPage[] menues;

    public string startCanvas;
    public GameObject levelSelectionCanvas;
	// Use this for initialization
	void Start () {
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
	}


    public void OpenMenu(string menuName)
    {
        for(int i = 0; i < menues.Length; i++)
        {
            if(string.Equals(menues[i].name, menuName))
            {
                menues[i].item.SetActive(true);
            }
            else
            {
                menues[i].item.SetActive(false);
            }
        }

    }

    public void Exit()
    {
        Application.Quit();
    }


    [System.Serializable]
    public struct MenuPage
    {
        public string name;
        public GameObject item;
    }
}
