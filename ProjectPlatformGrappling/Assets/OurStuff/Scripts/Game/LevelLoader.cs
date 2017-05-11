using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour {
    //public static LevelLoader loader; //singleton

    LevelManager levelManager;
    ScoreManager scoreManager; //resettar den
    ReplayManager replayManager;
    //private static Level activeLevel;
    private static int activeLevelIndex; //index istället för själva objektet, så man kan hämta den senaste variationen från LevelManager

    private AsyncOperation async = null;
    private bool isLoading = false;

    public GameObject loadingScreenObject;
    public Slider loadBar;

    BaseClass[] allB;

    // Use this for initialization
    void Awake () {
        levelManager = GetComponent<LevelManager>();
        scoreManager = GetComponent<ScoreManager>();
        replayManager = GetComponent<ReplayManager>();
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

    public void LoadMainMenu()
    {
        LoadLevel(levelManager.startMenuLevel);
    }

    public void LoadNextLevel()
    {
        Level levelToBeLoaded = levelManager.GetNextLevel(GetCurrLevel());
        LoadLevel(levelToBeLoaded);
    }


    public bool LoadLevel(Level lv)
    {
        if (isLoading == true) return false;
        if (lv.isLocked) { print("Secretmap, this map will not be loaded"); return false; } //låst secret map
        replayManager.EndSimulation();
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

        //Level lvTemp = new Level(lv); //kopierar eftersom referensen kan försvinna när man deletar detta objekt o låter en annan instance sköta det: (if (potLevelLoaders.Length > 1) Destroy(this.gameObject);)
        activeLevelIndex = lv.levelIndex;
        yield return async;

        loadingScreenObject.SetActive(false);
        isLoading = false;
        async = null;

        //LevelLoader[] potLevelLoaders = FindObjectsOfType(typeof(LevelLoader)) as LevelLoader[]; //se till så att det bara finns en levelloader i scenen
        //if (potLevelLoaders.Length > 1) Destroy(this.gameObject);
        Debug.Log("Loading complete");
    }

    void LevelLoaded(Scene scene, LoadSceneMode sceneMode) //istället för OnLevelWasLoaded
    {

        Debug.Log("Ny scene");
        allB = FindObjectsOfType(typeof(BaseClass)) as BaseClass[];

        for (int i = 0; i < allB.Length; i++)
        {
            allB[i].isLocked = false;
            allB[i].NewLevel(); //kalla new-level-loaded funktioner
        }
    }

    public string GetCurrLevelName()
    {
        return GetCurrLevel().name;
    }

    public Level GetCurrLevel()
    {
        return levelManager.GetLevel(activeLevelIndex); //index så att man får den senaste variationen av leveln!
    }
}
