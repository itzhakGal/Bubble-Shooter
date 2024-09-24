using System.Collections.Generic;
using UnityEngine;

public class BubblePool : MonoBehaviour
{
    private static BubblePool instance;
    public List<GameObject> bubblePrefabs; // רשימה של כל סוגי הבועות
    public int poolSize = 20;
    private Queue<GameObject> bubblePool;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        bubblePool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bubblePrefab = bubblePrefabs[Random.Range(0, bubblePrefabs.Count)]; // שליפת בועה אקראית
            GameObject bubble = Instantiate(bubblePrefab);
            bubble.SetActive(false);
            bubblePool.Enqueue(bubble);
        }
    }

    public GameObject GetBubble()
    {
        if (bubblePool.Count > 0)
        {
            GameObject bubble = bubblePool.Dequeue();
            bubble.SetActive(true);
            return bubble;
        }
        else
        {
            GameObject bubblePrefab = bubblePrefabs[Random.Range(0, bubblePrefabs.Count)]; // שליפת בועה אקראית
            GameObject newBubble = Instantiate(bubblePrefab);
            return newBubble;
        }
    }

    public void ReturnBubble(GameObject bubble)
    {
        bubble.SetActive(false);
        bubblePool.Enqueue(bubble);
    }
}
