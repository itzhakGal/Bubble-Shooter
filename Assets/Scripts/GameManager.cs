using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	#region Singleton
	public static GameManager instance;

	private void Awake()
	{
		if (instance == null)
			instance = this;

		DontDestroyOnLoad(gameObject);

		// אתחול הרשימות
		sequenceBubbles = new List<Transform>();
		connectedBubbles = new List<Transform>();
		bubblesToDrop = new List<Transform>();
		bubblesToDissolve = new List<Transform>();
	}

	void Start()
	{
		// ודא שה-jsonLoader מאותחל
		jsonLoader = GetComponent<JSONLoader>();

		if (jsonLoader == null)
		{
			Debug.LogError("JSONLoader is not attached or found on the GameManager.");
		}
		else
		{
			jsonLoader.LoadLevelData();  // טען את נתוני השלבים קודם כל
			
			currentLevel = 1;  // קבע שאנחנו מתחילים מהשלב הראשוןOnLevelComplete
            LoadLevel(currentLevel);
		
		}
    }
	


	private void LoadLevel(int level)
    {
        LevelData levelData = jsonLoader.GetLevelData(level);

		if (levelData != null)
        {
			// עדכון הבועות לפי השלב החדש
			LevelManager.instance.ClearLevel();
			LevelManager.instance.UpdateListOfBubblesInScene();
			GameManager.instance.shootScript.CreateNewBubbles(); // יצירת בועות לשלב החדש
			shootScript.SetupLevel(levelData.bubbleColors, levelData.bubbleSpeed, levelData.hasBombs);
			GameManager.instance.gameState = "play";
		}
		else
		{
			Debug.LogError("Level not found.");
		}
	}


	#endregion

	private const int SEQUENCE_SIZE = 3;

	private List<Transform> sequenceBubbles;
	private List<Transform> connectedBubbles;
	private List<Transform> bubblesToDrop;
	private List<Transform> bubblesToDissolve;
	public Shooter shootScript;
	public GameObject winMenu;
	public GameObject lossMenu;
	public GameObject winScore;
	public GameObject winThrows;
	public GameObject playBtn;
	public GameObject startUI;
	public GameObject explosionPrefab;
    public Transform bottomLimit;
	public Text score;
	public Text throws;
	public float dropSpeed = 50f;
	public string gameState = "play";
	public bool isDissolving = false;
	private bool hitABomb = false;
	public float dissolveSpeed = 2f;
	public float rayDistance = 200f;
	public int currentLevel = 1;
	public JSONLoader jsonLoader;  // משתנה להחזיק את ה-JSONLoader שמכיל את נתוני השלבים
	private void Update()
	{
		if (isDissolving)
		{
			foreach (Transform bubble in bubblesToDissolve)
			{

				if (bubble == null)
				{
					//make sure every bubble disappeared before ending the dissolve
					if (bubblesToDissolve.IndexOf(bubble) == bubblesToDissolve.Count - 1)
					{
						isDissolving = false;
						EmptyDissolveList();
						break;
					}
					else continue;
				}

				SpriteRenderer spriteRenderer = bubble.GetComponent<SpriteRenderer>();
				float dissolveAmount = spriteRenderer.material.GetFloat("_DissolveAmount");

				if (dissolveAmount >= 0.99f)
				{
					isDissolving = false;
					EmptyDissolveList();
					break;
				}
				else
				{
					float newDissolve = dissolveAmount + dissolveSpeed * Time.deltaTime;
					spriteRenderer.material.SetFloat("_DissolveAmount", newDissolve);
				}
			}
		}
	}

	private void EmptyDissolveList()
	{
		foreach (Transform bubble in bubblesToDissolve)
			if (bubble != null) Destroy(bubble.gameObject);

		bubblesToDissolve.Clear();
	}

	public void ToggleGameState()
	{
		if (gameState == "play")
		{
			playBtn.GetComponent<Image>().color = Color.gray;
			gameState = "pause";
			PauseGame();
		}
		else if (gameState == "pause")
		{
			playBtn.GetComponent<Image>().color = Color.white;
			gameState = "play";
			ResumeGame();
		}
	}

	public void RestartGame()
	{
		LevelManager.instance.ClearLevel();
		shootScript.canShoot = false;
		startUI.SetActive(true);
		lossMenu.SetActive(false); // הסתרת תפריט ההפסד במקרה של הפסד קודם
		winMenu.SetActive(false);  // הסתרת תפריט הניצחון במקרה של ניצחון קודם
        gameState = "play";  // עדכון המצב ל-play
		currentLevel = 1;

	}

    private void PauseGame()
	{
		Time.timeScale = 0f;
	}

	private void ResumeGame()
	{
		Time.timeScale = 1f;
	}

	public void ResetGame()
	{
		currentLevel = 1;
		
		startUI = GameObject.Find("StartUI");
		if (startUI != null)
			startUI.SetActive(true);

		winMenu = GameObject.Find("WinMenu");
		if (winMenu != null)
			winMenu.SetActive(false);

		lossMenu = GameObject.Find("WinMenu");
		if (lossMenu != null)
        {
			lossMenu.SetActive(false);
		}
		

		winScore = GameObject.Find("WinScore");
		winThrows = GameObject.Find("WinThrows");
		playBtn = GameObject.Find("PlayButton");

		bottomLimit = GameObject.Find("BottomLimit").transform;

		// אתחול הסקריפט של הירי
		shootScript = GameObject.FindObjectOfType<Shooter>();
		if (shootScript != null)
		{
			shootScript.canShoot = true; // אפשר ירי
			shootScript.CreateNextBubble(); // מכין את הבועה הבאה לירי
		}
		
		gameState = "play";
		
		sequenceBubbles = new List<Transform>();
		connectedBubbles = new List<Transform>();
		bubblesToDrop = new List<Transform>();
		bubblesToDissolve = new List<Transform>();
		
		ResumeGame();

		// עדכון מצב של LevelManager
		if (LevelManager.instance != null)
		{
			LevelManager.instance.ClearLevel();
			LevelManager.instance.UpdateListOfBubblesInScene();
		}
	}

	IEnumerator CheckSequence(Transform currentBubble)
	{
		yield return new WaitForSeconds(0.1f);

		sequenceBubbles.Clear();
		CheckBubbleSequence(currentBubble);
		ProcessSpecialBubbles(currentBubble);

		if ((sequenceBubbles.Count >= SEQUENCE_SIZE) || hitABomb)
		{
			ProcessBubblesInSequence();
			ProcessDisconnectedBubbles();
		}

		sequenceBubbles.Clear();
		LevelManager.instance.UpdateListOfBubblesInScene();
		hitABomb = false;

		if (LevelManager.instance.bubblesInScene.Count == 0)
		{
			currentLevel++;
			
			if (currentLevel <= jsonLoader.levelDataList.levels.Count)
            {
				if (currentLevel < 1)
					currentLevel = 1;
				
				LoadLevel(currentLevel); 
								
				LevelManager.instance.UpdateListOfBubblesInScene();  // עדכון רשימת הבועות לאחר הטעינה
				LevelManager.instance.StartNewGame();  // הפעלת המשחק מחדש ומילוי בבועות
				
            }
            else
			{
                // Optionally, show a final victory message here
                winMenu.SetActive(true);
                score.text = (ScoreManager.GetInstance().GetScore()).ToString();
				throws.text = (ScoreManager.GetInstance().GetThrows()).ToString();
				currentLevel = 1;
			}
        }
		else
		{
			shootScript.CreateNextBubble();
			shootScript.canShoot = true;
		}

		ProcessBottomLimit();
	}

	
	public void ProcessTurn(Transform currentBubble)
	{
		StartCoroutine(CheckSequence(currentBubble));
	}

	private void ProcessBottomLimit()
	{
		// בדיקת בועות רק אם מצב המשחק הוא "play"
		if (gameState != "play")
			return;

		foreach (Transform t in LevelManager.instance.bubblesArea)
		{
			if (t == null || !t.GetComponent<Bubble>().isConnected)
				continue;

			// בדיקה אם בועה עברה את הגבול התחתון
			if (t.position.y < bottomLimit.position.y)
			{
				lossMenu.SetActive(true);
                gameState = "loss";  // עדכון מצב המשחק להפסד
				currentLevel = 1;

				break;
			}
		}
	}

	private void CheckBubbleSequence(Transform currentBubble)
	{
		sequenceBubbles.Add(currentBubble);

		Bubble bubbleScript = currentBubble.GetComponent<Bubble>();
		List<Transform> neighbours = bubbleScript.GetNeighbours();

		foreach (Transform t in neighbours)
		{
			if (!sequenceBubbles.Contains(t))
			{
				Bubble bScript = t.GetComponent<Bubble>();

				if (bScript.bubbleColor == bubbleScript.bubbleColor)
				{
					CheckBubbleSequence(t);
				}
			}
		}
	}

	private void ProcessSpecialBubbles(Transform currentBubble)
	{
		Bubble bubbleScript = currentBubble.GetComponent<Bubble>();
		List<Transform> neighbours = bubbleScript.GetNeighbours();

		foreach (Transform t in neighbours)
		{
			Bubble bScript = t.GetComponent<Bubble>();

			if (bScript.bubbleColor == Bubble.BubbleColor.Bomb)
			{
				hitABomb = true;

				//create explosion effect
				GameObject explosion = Instantiate(explosionPrefab, t.position, Quaternion.identity);
				explosion.transform.localScale = new Vector3(25f, 25f, 1f);
				Destroy(explosion, 0.5f);

				//destroy the bomb
				Destroy(t.gameObject);
				
				foreach (Transform t2 in bScript.GetNeighbours())
				{
					if (sequenceBubbles.Contains(t2))
						sequenceBubbles.Remove(t2);

					Destroy(t2.gameObject);
				}

				ScoreManager.GetInstance().AddScore(10);
			}

		}
	}
	private void ProcessBubblesInSequence()
	{
		if (hitABomb)
			AudioManager.instance.PlaySound("explosion");
		else
			AudioManager.instance.PlaySound("destroy");

		foreach (Transform t in sequenceBubbles)
		{
			if (!bubblesToDissolve.Contains(t))
			{
				ScoreManager.GetInstance().AddScore(1);
				t.tag = "Untagged";
				t.SetParent(null);
				t.GetComponent<CircleCollider2D>().enabled = false;
				bubblesToDissolve.Add(t);
			}
		}
		isDissolving = true;
	}

	#region Drop Disconected Bubbles

	private void ProcessDisconnectedBubbles()
	{
		
		SetAllBubblesConnectionToFalse();
		SetConnectedBubblesToTrue();
		CheckDisconnectedBubbles();
		DropAll();
	}

	private void SetAllBubblesConnectionToFalse()
	{
		foreach (Transform bubble in LevelManager.instance.bubblesArea)
		{
			bubble.GetComponent<Bubble>().isConnected = false;
		}
	}

	private void SetConnectedBubblesToTrue()
	{
		connectedBubbles.Clear();

		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right, rayDistance);

		foreach (var hit in hits)
		{
			if (hit.transform.gameObject.tag.Equals("Bubble"))
				SetNeighboursConnectionToTrue(hit.transform);
		}
	}

	private void SetNeighboursConnectionToTrue(Transform bubble)
	{
		connectedBubbles.Add(bubble);

		Bubble bubbleScript = bubble.GetComponent<Bubble>();
		bubbleScript.isConnected = true;

		foreach (Transform t in bubbleScript.GetNeighbours())
		{
			if (!connectedBubbles.Contains(t))
			{
				SetNeighboursConnectionToTrue(t);
			}
		}
	}

	private void CheckDisconnectedBubbles()
	{
		foreach (Transform bubble in LevelManager.instance.bubblesArea)
		{
			Bubble bubbleScript = bubble.GetComponent<Bubble>();
			if (!bubbleScript.isConnected)
			{
				if (!bubblesToDrop.Contains(bubble))
				{
					ScoreManager.GetInstance().AddScore(2);
					bubble.tag = "Untagged";
					bubblesToDrop.Add(bubble);
				}
			}
		}
	}

	private void DropAll()
	{
		foreach (Transform bubble in bubblesToDrop)
		{
			bubble.SetParent(null);
			//Destroy(bubble.gameObject);
			bubble.gameObject.GetComponent<CircleCollider2D>().enabled = false;
			if (!bubble.GetComponent<Rigidbody2D>())
			{
				Rigidbody2D rig = (Rigidbody2D)bubble.gameObject.AddComponent(typeof(Rigidbody2D));
				rig.gravityScale = dropSpeed;
			}
		}
		bubblesToDrop.Clear();
	}
	public void OnLevelComplete()
	{
		
		if (currentLevel <= jsonLoader.levelDataList.levels.Count)
		{
			Debug.Log("Level complete, moving to the next level.");
		}
		else
		{
			Debug.Log("All levels completed!");
            winMenu.SetActive(true);
		}
    }
	public void StartNextLevel()
	{
		if (currentLevel <= jsonLoader.levelDataList.levels.Count)
		{
			Debug.Log("Moving to level " + currentLevel);
			LoadLevel(currentLevel);  // טוען את השלב החדש
			shootScript.CreateNewBubbles();  // אתחול מחדש של הבועות לשלב החדש

			// ודא שהמשחק חוזר למצב 'play'
			gameState = "play";
		}
		else
		{
			Debug.Log("All levels completed!");
			// אפשר להוסיף מסך ניצחון כאן, או פעולה אחרת לסיום המשחק
		}
	}
	#endregion
}