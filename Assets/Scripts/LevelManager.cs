using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public Grid grid;
    public Transform bubblesArea;
    public List<GameObject> bubblesPrefabs;
    public List<GameObject> bubblesInScene;
    public List<string> colorsInScene;
    public GameObject level;

    private void Start()
    {
        grid = GetComponent<Grid>();
    }

    public void StartNewGame()
    {
        // ודא שה-StartUI מוסתר אוטומטית ולא מחכה ללחיצה
        if (GameManager.instance.startUI != null)
            GameManager.instance.startUI.SetActive(false);

        GameManager.instance.winMenu.SetActive(false);
        GameManager.instance.lossMenu.SetActive(false);
        StartCoroutine(LoadSingleLevel());
    }


    private IEnumerator LoadSingleLevel()
    {
        yield return new WaitForSeconds(0.1f);

        ScoreManager.GetInstance().Reset();
        ClearLevel();
        GameObject levelToLoad = Instantiate(level);
        FillWithBubbles(levelToLoad, bubblesPrefabs);

        SnapChildrensToGrid(bubblesArea);
        // No more special bubbles, so we remove InsertSpecialBubbles()

        UpdateListOfBubblesInScene();
        GameManager.instance.shootScript.CreateNewBubbles();
    }

    public void ClearLevel()
    {
        foreach (Transform t in bubblesArea)
            Destroy(t.gameObject);
    }

    #region Snap to Grid
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

    private void FillWithBubbles(GameObject go, List<GameObject> _prefabs)
    {
        foreach (Transform t in go.transform)
        {
            var bubble = Instantiate(_prefabs[Random.Range(0, _prefabs.Count)], bubblesArea);
            bubble.transform.position = t.position;
        }
        Destroy(go);
    }

    public void UpdateListOfBubblesInScene()
    {
        List<string> colors = new List<string>();
        List<GameObject> newListOfBubbles = new List<GameObject>();

        foreach (Transform t in bubblesArea)
        {
            Bubble bubbleScript = t.GetComponent<Bubble>();
            if (colors.Count < bubblesPrefabs.Count && !colors.Contains(bubbleScript.bubbleColor.ToString()))
            {
                string color = bubbleScript.bubbleColor.ToString();

                foreach (GameObject prefab in bubblesPrefabs)
                {
                    if (color.Equals(prefab.GetComponent<Bubble>().bubbleColor.ToString()))
                    {
                        colors.Add(color);
                        newListOfBubbles.Add(prefab);
                    }
                }
            }
        }

        colorsInScene = colors;
        bubblesInScene = newListOfBubbles;
    }

    public void SetAsBubbleAreaChild(Transform bubble)
    {
        SnapToNearestGridPosition(bubble);
        bubble.SetParent(bubblesArea);
    }
    
}