using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

	private void Start()
	{
		// קריאה אוטומטית להתחלת המשחק לאחר ש-LevelManager כבר קיים
		if (LevelManager.instance != null)
		{
			LevelManager.instance.StartNewGame();
		}
		else
		{
			Debug.LogError("LevelManager instance is not found!");
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
	public Transform bottomLimit;
	public float dropSpeed = 50f;
	public string gameState = "play";
	public bool isDissolving = false;
	public float dissolveSpeed = 2f;
	public float rayDistance = 200f;

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
		gameState = "play";
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
		// ודאי שכל האובייקטים שהוגדרו ב-GameManager מאתחלים את עצמם בצורה נכונה
		startUI = GameObject.Find("StartUI");
		if (startUI != null)
			startUI.SetActive(true);

		winMenu = GameObject.Find("WinMenu");
		if (winMenu != null)
			winMenu.SetActive(false);
		
		lossMenu = GameObject.Find("WinMenu");
		if (lossMenu != null)
			lossMenu.SetActive(false);
		
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

		// ודאי שהמצב של המשחק הוא play
		gameState = "play";

		// אתחול הרשימות
		sequenceBubbles = new List<Transform>();
		connectedBubbles = new List<Transform>();
		bubblesToDrop = new List<Transform>();
		bubblesToDissolve = new List<Transform>();

		// ודאי שהזמן לא מופסק (אם היה במצב pause)
		ResumeGame();

		// עדכון מצב של LevelManager
		if (LevelManager.instance != null)
		{
			LevelManager.instance.ClearLevel();
			LevelManager.instance.UpdateListOfBubblesInScene();
		}

		Debug.Log("Game has been reset successfully");
	}


	IEnumerator CheckSequence(Transform currentBubble)
	{
		yield return new WaitForSeconds(0.1f);

		sequenceBubbles.Clear();
		CheckBubbleSequence(currentBubble);
		//ProcessSpecialBubbles(currentBubble);

		if (sequenceBubbles.Count >= SEQUENCE_SIZE)
		{
			ProcessBubblesInSequence();
			ProcessDisconnectedBubbles();
		}

		sequenceBubbles.Clear();
		LevelManager.instance.UpdateListOfBubblesInScene();

		if (LevelManager.instance.bubblesInScene.Count == 0)
		{
			ScoreManager man = ScoreManager.GetInstance();
			winScore.GetComponent<Text>().text = man.GetScore().ToString();
			winThrows.GetComponent<Text>().text = man.GetThrows().ToString();
			winMenu.SetActive(true);
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
		foreach (Transform t in LevelManager.instance.bubblesArea)
		{
			if (t.GetComponent<Bubble>().isConnected && t.position.y < bottomLimit.position.y)
			{
				lossMenu.SetActive(true);
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

	private void ProcessBubblesInSequence()
	{

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

	#endregion
}