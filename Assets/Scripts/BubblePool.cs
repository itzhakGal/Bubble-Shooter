//using System.Collections.Generic;
//using UnityEngine;

//public class BubblePool : MonoBehaviour
//{
//    private static BubblePool instance;
//    public List<GameObject> bubblePrefabs; // רשימה של כל סוגי הבועות
//    public int poolSize = 20;
//    private Queue<GameObject> bubblePool;
//    public GameObject bombPrefab; // הוספת פצצה לרשימת הפריפאבים


//    private void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }

//        bubblePool = new Queue<GameObject>();
//        for (int i = 0; i < poolSize; i++)
//        {
//            GameObject bubblePrefab = bubblePrefabs[Random.Range(0, bubblePrefabs.Count)]; // שליפת בועה אקראית
//            GameObject bubble = Instantiate(bubblePrefab);
//            bubble.SetActive(false);
//            bubblePool.Enqueue(bubble);
//        }
//    }

//    public GameObject GetBubble()
//    {
//        if (bubblePool.Count > 0)
//        {
//            GameObject bubble = bubblePool.Dequeue();
//            bubble.SetActive(true);
//            return bubble;
//        }
//        else
//        {
//            GameObject bubblePrefab = bubblePrefabs[Random.Range(0, bubblePrefabs.Count)]; // שליפת בועה אקראית
//            GameObject newBubble = Instantiate(bubblePrefab);
//            return newBubble;
//        }
//    }

//    public void ReturnBubble(GameObject bubble)
//    {
//        bubble.SetActive(false);
//        bubblePool.Enqueue(bubble);
//    }
//}
//using System.Collections.Generic;
//using UnityEngine;

//public class BubblePool : MonoBehaviour
//{
//    // Instance עבור Singleton pattern
//    private static BubblePool instance;

//    // רשימות עבור בועות ופצצות
//    public List<GameObject> bubblePrefabs;
//    public GameObject bombPrefab;

//    // גודל הבריכה (pool) עבור הבועות והפצצות
//    public int poolSize = 20;
//    private Queue<GameObject> bubblePool; // בריכת אובייקטים לבועות

//    // Singleton Pattern - רק מופע אחד של BubblePool יתקיים
//    private void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//        }
//        else
//        {
//            Destroy(gameObject); // אם קיים מופע נוסף, הורסים אותו
//        }

//        // אתחול בריכת האובייקטים
//        InitializePool();
//    }

//    // אתחול בריכת הבועות והפצצות
//    private void InitializePool()
//    {
//        bubblePool = new Queue<GameObject>();

//        for (int i = 0; i < poolSize; i++)
//        {
//            // יצירת פצצה או בועה בהתבסס על הסיכויים
//            GameObject objectToPool = Random.value < 0.3f ? bombPrefab : bubblePrefabs[Random.Range(0, bubblePrefabs.Count)];
//            GameObject pooledObject = Instantiate(objectToPool);
//            pooledObject.SetActive(false); // כיבוי האובייקט בבריכה עד שהוא יידרש
//            bubblePool.Enqueue(pooledObject); // הוספת האובייקט לבריכה
//        }
//    }

//    // קבלת אובייקט מהבריכה (בועה או פצצה)
//    public GameObject GetBubble()
//    {
//        if (bubblePool.Count > 0)
//        {
//            GameObject bubble = bubblePool.Dequeue();
//            bubble.SetActive(true); // הפעלת האובייקט
//            return bubble;
//        }
//        else
//        {
//            // אם הבריכה ריקה, יוצרים אובייקט חדש ומחזירים אותו
//            return CreateNewBubbleOrBomb();
//        }
//    }

//    // החזרת אובייקט לבריכה
//    public void ReturnBubble(GameObject bubble)
//    {
//        bubble.SetActive(false); // כיבוי האובייקט
//        bubblePool.Enqueue(bubble); // החזרת האובייקט לבריכה
//    }

//    // יצירת אובייקט חדש (בועה או פצצה) כאשר הבריכה ריקה
//    private GameObject CreateNewBubbleOrBomb()
//    {
//        GameObject objectToCreate = Random.value < 0.3f ? bombPrefab : bubblePrefabs[Random.Range(0, bubblePrefabs.Count)];
//        GameObject newObject = Instantiate(objectToCreate);
//        return newObject;
//    }
//}
