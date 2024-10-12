
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public bool canShoot;
    public float speed = 25f;
    
    // Add the missing members
    private int bubbleColors;
    private float bubbleSpeed;
    private bool hasBombs;

    public Transform nextBubblePosition;
    public GameObject currentBubble;
    public GameObject nextBubble;
    public GameObject bottomShootPoint;

    private Vector2 lookDirection;
    private float lookAngle;
    private GameObject line;
    private GameObject limit;
    private LineRenderer lineRenderer;
    private Vector2 gizmosPoint;
    private bool levelSetupComplete = false;
    public void Awake()
    {
        line = GameObject.FindGameObjectWithTag("Line");
        limit = GameObject.FindGameObjectWithTag("Limit");
    }
    
    public void Update()
    {
        if (GameManager.instance.gameState == "play")
        {
            gizmosPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

            if (Input.GetMouseButton(0)
                && (Camera.main.ScreenToWorldPoint(Input.mousePosition).y > bottomShootPoint.transform.position.y)
                && (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < limit.transform.position.y))
            {
                line.transform.position = transform.position;
                line.transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90);
            }
            else
            {
                line.SetActive(false);
            }

            if (canShoot
                && Input.GetMouseButtonUp(0)
                && (Camera.main.ScreenToWorldPoint(Input.mousePosition).y > bottomShootPoint.transform.position.y)
                && (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < limit.transform.position.y))
            {
                canShoot = false;
                if (currentBubble == null)
                {
                    Debug.Log("No bubble available to shoot");
                }
                else
                {
                    Shoot();
                }
            }
        }
    }


    public void Shoot()
    {

       
        if (currentBubble == null) CreateNextBubble();
        ScoreManager.GetInstance().AddThrows();
        AudioManager.instance.PlaySound("shoot");
        transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
        currentBubble.transform.rotation = transform.rotation;
        currentBubble.GetComponent<CircleCollider2D>().enabled = true;
        Rigidbody2D rb = currentBubble.GetComponent<Rigidbody2D>();
        rb.AddForce(currentBubble.transform.up * speed, ForceMode2D.Impulse);
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0;
        currentBubble = null;
    }

    public void SwapBubbles()
    {
        List<GameObject> bubblesInScene = LevelManager.instance.bubblesInScene;
        if (bubblesInScene.Count < 1) return;

        currentBubble.transform.position = nextBubblePosition.position;
        nextBubble.transform.position = transform.position;
        GameObject temp = currentBubble;
        currentBubble = nextBubble;
        nextBubble = temp;
    }

    public void CreateNewBubbles()
    {
        if (nextBubble != null)
            Destroy(nextBubble);

        if (currentBubble != null)
            Destroy(currentBubble);

        nextBubble = null;
        currentBubble = null;

        CreateNextBubble();
        canShoot = true;
    }

    public void CreateNextBubble()
    {
        List<GameObject> bubblesInScene = LevelManager.instance.bubblesInScene;
       
        if (bubblesInScene.Count < 1) return;

        if (nextBubble == null)
        {
            nextBubble = InstantiateNewBubble(bubblesInScene);
        }

        if (currentBubble == null)
        {
            currentBubble = nextBubble;
            currentBubble.transform.position = transform.position;

            // יצירת בועה חדשה לבועה הבאה
            nextBubble = InstantiateNewBubble(bubblesInScene);

            // עדכון מהירות הבועה לפי נתוני השלב הנוכחי
            speed = GameManager.instance.jsonLoader.GetLevelData(GameManager.instance.currentLevel).bubbleSpeed;
        }
    }




    private GameObject InstantiateNewBubble(List<GameObject> bubblesInScene)
    {
        if (bubblesInScene == null || bubblesInScene.Count == 0)
        {
            Debug.LogError("אין בועות זמינות ליצירה.");
            return null;
        }

        GameObject newBubble = Instantiate(bubblesInScene[Random.Range(0, bubblesInScene.Count)]);
        newBubble.transform.position = nextBubblePosition.position;
        newBubble.GetComponent<Bubble>().isFixed = false;
        newBubble.GetComponent<CircleCollider2D>().enabled = false;
        Rigidbody2D rb2d = newBubble.AddComponent<Rigidbody2D>();
        rb2d.gravityScale = 0f;
        return newBubble;
    }



    // Method to set up the level
    public void SetupLevel(int bubbleColors, float bubbleSpeed, bool hasBombs)
    {
        if (levelSetupComplete) return; // אם השלב כבר נטען, לא לבצע שוב
        
        this.bubbleColors = bubbleColors;
        this.bubbleSpeed = bubbleSpeed;
        this.hasBombs = hasBombs;

        CreateNextBubble();
        levelSetupComplete = true; // סמן שהשלב נטען
    }

    public void ResetLevelSetup()
    {
        levelSetupComplete = false; // אפשר להפעיל מחדש בעת מעבר לשלב הבא
    }
}
