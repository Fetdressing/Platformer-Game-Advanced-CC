using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour {
    private AsyncOperation async = null;
    private bool isLoading = false;

    public GameObject loadingScreenObject;
    public Slider loadBar;

    BaseClass[] allB;

    // Use this for initialization
    void Awake () {
        DontDestroyOnLoad(transform);
        allB = FindObjectsOfType(typeof(BaseClass)) as BaseClass[];

        loadingScreenObject.SetActive(false);
        isLoading = false;
    }

    void Update()
    {
        if (isLoading == false) return;

        if (async != null)
        {
            loadBar.value = async.progress;
        }
    }

    public void LoadLevel(string name)
    {
        if (isLoading == true) return;
        loadingScreenObject.SetActive(true);
        isLoading = true;
        StartCoroutine(LoadScene(name));
    }

    IEnumerator LoadScene(string name)
    {

        for (int i = 0; i < allB.Length; i++)
        {
            allB[i].isLocked = true;
        }
        async = SceneManager.LoadSceneAsync(name);
        yield return async;

        loadingScreenObject.SetActive(false);
        isLoading = false;
        async = null;

        LevelLoader[] potLevelLoaders = FindObjectsOfType(typeof(LevelLoader)) as LevelLoader[]; //se till så att det bara finns en levelloader i scenen
        if (potLevelLoaders.Length > 1) Destroy(this.gameObject);
        Debug.Log("Loading complete");
    }

    void OnLevelWasLoaded()
    {

        Debug.Log("Ny scene");
        allB = FindObjectsOfType(typeof(BaseClass)) as BaseClass[];

        for (int i = 0; i < allB.Length; i++)
        {
            allB[i].isLocked = false;
        }
    }
}
