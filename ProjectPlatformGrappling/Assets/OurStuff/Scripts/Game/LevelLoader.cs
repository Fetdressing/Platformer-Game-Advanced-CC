using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour {
    LevelManager levelManager;
    private static Level activeLevel;

    private AsyncOperation async = null;
    private bool isLoading = false;

    public GameObject loadingScreenObject;
    public Slider loadBar;

    BaseClass[] allB;

    // Use this for initialization
    void Awake () {
        DontDestroyOnLoad(transform);
        levelManager = GetComponent<LevelManager>();
        allB = FindObjectsOfType(typeof(BaseClass)) as BaseClass[];

        loadingScreenObject.SetActive(false);
        isLoading = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += this.LevelLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= this.LevelLoaded;
    }

    void Update()
    {
        if (isLoading == false) return;

        if (async != null)
        {
            loadBar.value = async.progress;
        }
    }


    public void LoadNextLevel()
    {
        Level levelToBeLoaded = levelManager.GetNextLevel(activeLevel);
        LoadLevel(levelToBeLoaded);
    }


    public bool LoadLevel(Level lv)
    {
        if (isLoading == true) return false;
        if (lv.isLocked) { print("Secretmap, this map will not be loaded"); return false; } //låst secret map
        loadingScreenObject.SetActive(true);
        isLoading = true;
        StartCoroutine(LoadScene(lv));
        return true;
    }

    IEnumerator LoadScene(Level lv)
    {

        for (int i = 0; i < allB.Length; i++)
        {
            allB[i].isLocked = true;
        }
        async = SceneManager.LoadSceneAsync(lv.loadName);
        activeLevel = lv;
        yield return async;

        loadingScreenObject.SetActive(false);
        isLoading = false;
        async = null;

        LevelLoader[] potLevelLoaders = FindObjectsOfType(typeof(LevelLoader)) as LevelLoader[]; //se till så att det bara finns en levelloader i scenen
        if (potLevelLoaders.Length > 1) Destroy(this.gameObject);
        Debug.Log("Loading complete");
    }

    void LevelLoaded(Scene scene, LoadSceneMode sceneMode) //istället för OnLevelWasLoaded
    {

        Debug.Log("Ny scene");
        allB = FindObjectsOfType(typeof(BaseClass)) as BaseClass[];

        for (int i = 0; i < allB.Length; i++)
        {
            allB[i].isLocked = false;
        }
    }

    public string GetCurrLevelName()
    {
        return activeLevel.name;
    }

    public Level GetCurrLevel()
    {
        return activeLevel;
    }
}
