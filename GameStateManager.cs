using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-500)]
public class GameStateManager : MonoBehaviour
{
	public CustomGameDetails currentCustomGame;

	private readonly Dictionary<string, GameState> activeStates = new Dictionary<string, GameState>();

	private readonly List<string> stateOrder = new List<string>();

	public static GameStateManager Instance { get; private set; }

	public Vector3 defaultGravity { get; private set; }

	public bool CameraLocked { get; private set; }

	public bool PlayerInputLocked { get; private set; }

	public bool CursorLocked { get; private set; }

	public float TimerModifier { get; private set; } = 1f;


	public static bool CanSubmitScores
	{
		get
		{
			if (!MonoSingleton<StatsManager>.Instance.majorUsed)
			{
				return !MonoSingleton<AssistController>.Instance.cheatsEnabled;
			}
			return false;
		}
	}

	public static bool ShowLeaderboards => CanSubmitScores;

	public bool IsStateActive(string stateKey)
	{
		return activeStates.ContainsKey(stateKey);
	}

	public void RegisterState(GameState newState)
	{
		if (activeStates.ContainsKey(newState.key))
		{
			Debug.LogWarning("State " + newState.key + " is already registered");
			return;
		}
		activeStates.Add(newState.key, newState);
		int num = stateOrder.Count;
		for (int num2 = stateOrder.Count - 1; num2 >= 0; num2--)
		{
			string key = stateOrder[num2];
			GameState gameState = activeStates[key];
			num = num2;
			if (gameState.priority > newState.priority)
			{
				num++;
				break;
			}
		}
		stateOrder.Insert(num, newState.key);
		EvaluateState();
	}

	public void PopState(string stateKey)
	{
		if (!activeStates.ContainsKey(stateKey))
		{
			Debug.Log("Tried to pop state " + stateKey + ", but it was not registered");
			return;
		}
		activeStates.Remove(stateKey);
		stateOrder.Remove(stateKey);
		EvaluateState();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "Main Menu" || scene.name == "Custom Content" || scene.name == "Endless")
		{
			Physics.gravity = defaultGravity;
		}
	}

	private void EvaluateState()
	{
		float num = 1f;
		for (int num2 = stateOrder.Count - 1; num2 >= 0; num2--)
		{
			string key = stateOrder[num2];
			GameState gameState = activeStates[key];
			if (gameState.cursorLock != 0)
			{
				CursorLocked = gameState.cursorLock == LockMode.Lock;
			}
			if (gameState.playerInputLock != 0)
			{
				PlayerInputLocked = gameState.playerInputLock == LockMode.Lock;
			}
			if (gameState.cameraInputLock != 0)
			{
				CameraLocked = gameState.cameraInputLock == LockMode.Lock;
			}
			if (gameState.timerModifier.HasValue)
			{
				num *= gameState.timerModifier.Value;
			}
		}
		Cursor.lockState = (CursorLocked ? CursorLockMode.Locked : CursorLockMode.None);
		Cursor.visible = !CursorLocked;
		TimerModifier = num;
	}

	private void Update()
	{
		for (int num = stateOrder.Count - 1; num >= 0; num--)
		{
			string text = stateOrder[num];
			if (!activeStates[text].IsValid())
			{
				activeStates.Remove(text);
				stateOrder.Remove(text);
				EvaluateState();
				break;
			}
		}
	}

	private void Awake()
	{
		if (Instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		base.transform.SetParent(null);
		Object.DontDestroyOnLoad(base.gameObject);
		defaultGravity = Physics.gravity;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
