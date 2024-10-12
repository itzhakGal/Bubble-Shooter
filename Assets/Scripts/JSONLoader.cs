using System.IO;
using UnityEngine;

public class JSONLoader : MonoBehaviour
{
    public string fileName = "levels.json"; 

    public LevelDataList levelDataList; 

    public void LoadLevelData()
    {
        string path = Path.Combine(Application.dataPath, "JsonLevelScript", fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            levelDataList = JsonUtility.FromJson<LevelDataList>(json);

            if (levelDataList != null && levelDataList.levels != null)
            {
                Debug.Log("Loaded levels: " + levelDataList.levels.Count);
            }
            else
            {
                Debug.LogError("Failed to parse levels from JSON.");
            }
        }
        else
        {
            Debug.LogError("File not found: " + path);
        }
    }

    public LevelData GetLevelData(int level)
    {
        if (levelDataList != null)
        {

            return levelDataList.levels.Find(l => l.level == level);
        }
        return null;
    }
}
