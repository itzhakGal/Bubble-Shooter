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

        levelDataList.levels.Add(new LevelData { level = 1, bubbleColors = 2, hasBombs = true, bubbleSpeed = 200f });
        levelDataList.levels.Add(new LevelData { level = 2, bubbleColors = 3, hasBombs = true, bubbleSpeed = 210f });
        levelDataList.levels.Add(new LevelData { level = 3, bubbleColors = 4, hasBombs = true, bubbleSpeed = 220f });
        levelDataList.levels.Add(new LevelData { level = 4, bubbleColors = 4, hasBombs = true, bubbleSpeed = 230f });
        levelDataList.levels.Add(new LevelData { level = 5, bubbleColors = 5, hasBombs = false, bubbleSpeed = 230f });
        levelDataList.levels.Add(new LevelData { level = 6, bubbleColors = 6, hasBombs = false, bubbleSpeed = 240f });
        levelDataList.levels.Add(new LevelData { level = 7, bubbleColors = 6, hasBombs = false, bubbleSpeed = 250f });
        levelDataList.levels.Add(new LevelData { level = 8, bubbleColors = 6, hasBombs = false, bubbleSpeed = 260f });
        // Convert the data to JSON format
        string json = JsonUtility.ToJson(levelDataList, true);

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
