using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JSONWriter : MonoBehaviour
{
    public string fileName = "levels.json";

    void Start()
    {
        // Create level data
        LevelDataList levelDataList = new LevelDataList();
        levelDataList.levels = new List<LevelData>();

        levelDataList.levels.Add(new LevelData { level = 1, bubbleColors = 2, hasBombs = true, bubbleSpeed =70f });
        levelDataList.levels.Add(new LevelData { level = 2, bubbleColors = 3, hasBombs = true, bubbleSpeed = 90f });
        levelDataList.levels.Add(new LevelData { level = 3, bubbleColors = 4, hasBombs = true, bubbleSpeed = 110f });
        levelDataList.levels.Add(new LevelData { level = 4, bubbleColors = 4, hasBombs = true, bubbleSpeed = 120f });
        levelDataList.levels.Add(new LevelData { level = 5, bubbleColors = 5, hasBombs = false, bubbleSpeed = 130f });
        levelDataList.levels.Add(new LevelData { level = 6, bubbleColors = 6, hasBombs = false, bubbleSpeed = 140f });
        levelDataList.levels.Add(new LevelData { level = 7, bubbleColors = 6, hasBombs = false, bubbleSpeed = 140f });
        levelDataList.levels.Add(new LevelData { level = 8, bubbleColors = 6, hasBombs = false, bubbleSpeed = 160f });
        // Convert the data to JSON format
        string json = JsonUtility.ToJson(levelDataList, true);
        Debug.Log(json);

        // Set the path to save the file inside your project directory
        string directory = Path.Combine(Application.dataPath, "JsonLevelScript");
        string path = Path.Combine(directory, fileName);

        // Ensure the directory exists
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Save the JSON file
        File.WriteAllText(path, json);
        Debug.Log("JSON file saved at: " + path);
    }
}

[System.Serializable]
public class LevelData
{
    public int level;
    public int bubbleColors;
    public bool hasBombs;
    public float bubbleSpeed;
   
}

[System.Serializable]
public class LevelDataList
{
    public List<LevelData> levels;
}
