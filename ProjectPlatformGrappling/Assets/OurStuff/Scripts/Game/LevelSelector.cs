using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : BaseClass {
    LevelManager levelManager;
    LevelLoader levelLoader;

    public GameObject prefabButton;
	// Use this for initialization
	void Start () {
        Init();
	}

    public override void Init()
    {
        base.Init();
        levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        levelLoader = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelLoader>();

        transform.GetComponent<GridLayoutGroup>().cellSize = new Vector2(Screen.height * 0.2f, Screen.height * 0.2f);

        InstantiateMapButtons();
    }


    void InstantiateMapButtons()
    {
        for(int i = 0; i < levelManager.standardLevels.Length; i++)
        {
            Level lv = levelManager.standardLevels[i];
            GameObject tempB = (GameObject)Instantiate(prefabButton);
            tempB.transform.SetParent(transform, false);
            tempB.transform.localScale = new Vector3(1, 1, 1);

            if (lv.image != null)
            {
                tempB.GetComponent<Image>().sprite = lv.image;
            }

            Button tempButton = tempB.GetComponent<Button>();
            tempButton.onClick.AddListener(() => levelLoader.LoadLevel(lv));

            tempB.GetComponentInChildren<Text>().text = (i + 1).ToString();

            tempB.GetComponent<HoverText>().SetText("Time: " + lv.bestTime.ToString("F1") + "\nGlobes: " + lv.bestGlobesCollected.ToString());
        }
    }
}
