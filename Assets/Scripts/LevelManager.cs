using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    #region Singleton
    public static LevelManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public JSONLoader jsonLoader;
    public Grid grid;
    public Transform bubblesArea;
    public List<GameObject> bubblesPrefabs;
    public List<GameObject> bubblesInScene;
    public List<string> colorsInScene;
    public List<GameObject>level;
    public GameObject bombPrefab;

    private void Start()
    {
        grid = GetComponent<Grid>();
    }

    public void StartNewGame()
    {
        if (GameManager.instance.startUI != null)
            GameManager.instance.startUI.SetActive(false);

        GameManager.instance.winMenu.SetActive(false);
        GameManager.instance.lossMenu.SetActive(false);

        StartCoroutine(LoadSingleLevel());
    }

    private IEnumerator LoadSingleLevel()
    {
        LevelData currentLevelData = GameManager.instance.jsonLoader.GetLevelData(GameManager.instance.currentLevel);
        if (currentLevelData == null)
            Debug.Log("null");

        yield return new WaitForSeconds(0.1f);
        if (GameManager.instance.currentLevel == 1)
        {
            ScoreManager.GetInstance().Reset();
        }
        ClearLevel();
        Debug.Log("loading level " + currentLevelData.level);
            GameObject levelToLoad = Instantiate(level[currentLevelData.level-1]);
      
        FillWithBubbles(levelToLoad, bubblesPrefabs);

        SnapChildrensToGrid(bubblesArea);

        // עדכון רשימת הבועות לאחר יצירת הבועות לשלב
        UpdateListOfBubblesInScene();
        GameManager.instance.shootScript.CreateNewBubbles();
    }


    // פונקציה לניקוי השלב הנוכחי
    public void ClearLevel()
    {
        foreach (Transform t in bubblesArea)
        {
            Destroy(t.gameObject);
        }
        bubblesInScene.Clear();  // לוודא שניקית גם את רשימת הבועות
    }

    #region Snap to Grid
    // הצמדת כל הבועות לגריד
    private void SnapChildrensToGrid(Transform parent)
    {
        foreach (Transform t in parent)
        {
            SnapToNearestGridPosition(t);
        }
    }

    public void SnapToNearestGridPosition(Transform t)
    {
        Vector3Int cellPosition = grid.WorldToCell(t.position);
        t.position = grid.GetCellCenterWorld(cellPosition);
        t.rotation = Quaternion.identity;
    }
    #endregion

    // פונקציה למילוי השלב בבועות
    private void FillWithBubbles(GameObject go, List<GameObject> _prefabs)
    {
       
        LevelData currentLevelData = GameManager.instance.jsonLoader.GetLevelData(GameManager.instance.currentLevel);

        if (currentLevelData == null)
        {
            Debug.LogError("Level data is missing!");
            return;
        }

        Debug.Log("Filling level with bubbles. Colors: " + currentLevelData.bubbleColors);

        int bubbleColors = currentLevelData.bubbleColors;
        bool hasBombs = currentLevelData.hasBombs;
       
     
        // בחירת בועות על פי מספר הצבעים המותר
        List<GameObject> availableBubbles = _prefabs.GetRange(0, bubbleColors);

        foreach (Transform t in go.transform)
        {
           
            GameObject bubbleToCreate;

            // יצירת פצצה בסבירות 10%
            if (hasBombs && Random.value < 0.1f)
            {
                bubbleToCreate = bombPrefab;
                Debug.Log("Creating bomb at position " + t.position);
            }
            else
            {
                bubbleToCreate = availableBubbles[Random.Range(0, availableBubbles.Count)];
                Debug.Log("Creating bubble at position " + t.position);
            }

            var bubble = Instantiate(bubbleToCreate, bubblesArea);
            bubble.transform.position = t.position;
        }

        Destroy(go);  // הסרת התבנית הזמנית של הבועות
    }

    // פונקציה לעדכון רשימת הבועות שנמצאות בסצנה הנוכחית
    public void UpdateListOfBubblesInScene()
    {
        List<string> colors = new List<string>();
       
        List<GameObject> newListOfBubbles = new List<GameObject>();

        foreach (Transform t in bubblesArea)
        {
            Bubble bubbleScript = t.GetComponent<Bubble>();
            if (bubbleScript != null)
            {
                string color = bubbleScript.bubbleColor.ToString();
                Debug.Log("Found bubble with color: " + color);

                if (colors.Count < bubblesPrefabs.Count && !colors.Contains(color))
                {
                    colors.Add(color);

                    foreach (GameObject prefab in bubblesPrefabs)
                    {
                        if (color.Equals(prefab.GetComponent<Bubble>().bubbleColor.ToString()))
                        {
                            newListOfBubbles.Add(prefab);
                        }
                    }
                }
            }
        }

        colorsInScene = colors;
        bubblesInScene = newListOfBubbles;
 Debug.Log(colors.Count);
        Debug.Log("Updated bubble list in scene. Number of colors: " + colors.Count);
    }

    // פונקציה לקביעת בועה כתת-אובייקט של האזור של הבועות
    public void SetAsBubbleAreaChild(Transform bubble)
    {
        SnapToNearestGridPosition(bubble);
        bubble.SetParent(bubblesArea);
    }
}
