using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JSONWriter : MonoBehaviour
{
    public string fileName = "levels.json";

    void Start()
    {
        // יוצרים אובייקטים של שלבים
        LevelDataList levelDataList = new LevelDataList();
        levelDataList.levels = new List<LevelData>();

        levelDataList.levels.Add(new LevelData { level = 1, bubbleColors = 3, hasBombs = false, bubbleSpeed = 1.0f });
        levelDataList.levels.Add(new LevelData { level = 2, bubbleColors = 4, hasBombs = false, bubbleSpeed = 1.2f });
        levelDataList.levels.Add(new LevelData { level = 3, bubbleColors = 4, hasBombs = true, bubbleSpeed = 1.5f });
        levelDataList.levels.Add(new LevelData { level = 4, bubbleColors = 5, hasBombs = true, bubbleSpeed = 2.0f });

        // ממירים את הנתונים לפורמט JSON
        string json = JsonUtility.ToJson(levelDataList, true);  // true ליצירת JSON עם תצוגה מסודרת
        Debug.Log(json);

        // שומרים את ה-JSON כקובץ
        string path = Path.Combine(Application.dataPath, fileName);
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
