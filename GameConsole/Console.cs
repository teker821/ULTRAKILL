using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameConsole.Commands;
using pcon;
using pcon.Messages.pcon;
using pcon.unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameConsole;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class Console : MonoSingleton<Console>
{
	public bool pinned;

	public bool consoleOpen;

	public List<CapturedLog> logs = new List<CapturedLog>();

	public ConsoleLogType filters = ConsoleLogType.All;

	public int errorCount;

	public int warningCount;

	public int infoCount;

	private readonly List<LogLine> logLinePool = new List<LogLine>();

	[SerializeField]
	private GameObject consoleContainer;

	[SerializeField]
	private CanvasGroup consoleBlocker;

	[SerializeField]
	private InputField consoleInput;

	[Space]
	[SerializeField]
	private LogLine logLine;

	[SerializeField]
	private GameObject logContainer;

	[Space]
	[SerializeField]
	private GameObject scroller;

	[SerializeField]
	private Text scrollText;

	[SerializeField]
	private Text openBindText;

	[Space]
	public ErrorBadge errorBadge;

	[Space]
	[SerializeField]
	private GameObject[] hideOnPin;

	[SerializeField]
	private GameObject[] hideOnPinNoReopen;

	[SerializeField]
	private Image[] backgrounds;

	[SerializeField]
	private CanvasGroup masterGroup;

	[Space]
	public ConsoleWindow consoleWindow;

	private const int MaxLogLines = 20;

	private bool openedDuringPause;

	private OptionsManager rememberedOptionsManager;

	public readonly Dictionary<string, ICommand> recognizedCommands = new Dictionary<string, ICommand>();

	public readonly HashSet<Type> registeredCommandTypes = new HashSet<Type>();

	private int scrollState;

	private UnscaledTimeSince timeSincePgHeld;

	private UnscaledTimeSince timeSinceScrollTick;

	private List<string> commandHistory = new List<string>();

	private int commandHistoryIndex = -1;

	public Action onError;

	public Binds binds;

	public static bool IsOpen
	{
		get
		{
			if (MonoSingleton<Console>.Instance != null && MonoSingleton<Console>.Instance.consoleContainer != null)
			{
				return MonoSingleton<Console>.Instance.consoleContainer.activeSelf;
			}
			return false;
		}
	}

	private List<CapturedLog> filteredLogs => logs.Where((CapturedLog l) => filters.HasFlag(l.type)).ToList();

	protected override void Awake()
	{
		if (MonoSingleton<Console>.Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		base.Awake();
		binds = new Binds();
		binds.Initialize();
		if (binds.registeredBinds != null && binds.registeredBinds.ContainsKey("open") && binds.registeredBinds["open"].Action != null)
		{
			openBindText.text = binds.registeredBinds["open"].Action.GetBindingDisplayString();
		}
		RegisterCommands(new ICommand[4]
		{
			new Help(),
			new Clear(),
			new Echo(),
			new Exit()
		});
		RegisterCommands(new ICommand[9]
		{
			new Prefs(),
			new Scenes(),
			new GameConsole.Commands.Scene(),
			new ConsoleCmd(),
			new Style(),
			new Buffs(),
			new MapVar(),
			new InputCommands(),
			new Rumble()
		});
		if (UnityEngine.Debug.isDebugBuild)
		{
			RegisterCommand(new GameConsole.Commands.Debug());
			RegisterCommand(new Pcon());
		}
		for (int i = 0; i < 20; i++)
		{
			LogLine logLine = UnityEngine.Object.Instantiate(this.logLine, logContainer.transform, worldPositionStays: false);
			logLine.Wipe();
			logLine.gameObject.SetActive(value: false);
			logLinePool.Add(logLine);
		}
		this.logLine.gameObject.SetActive(value: false);
		Application.logMessageReceived += HandleLog;
		if (!Consts.CONSOLE_ERROR_BADGE)
		{
			errorBadge.SetEnabled(enabled: false);
		}
		if (UnityEngine.Debug.isDebugBuild && MonoSingleton<PrefsManager>.Instance.GetBoolLocal("pcon.autostart"))
		{
			StartPcon();
		}
	}

	public void StartPcon()
	{
		PconClient.StartClient(new Handler
		{
			onExecute = ProcessUserInput
		});
	}

	public void UpdateDisplayString()
	{
		openBindText.text = binds.registeredBinds["open"].Action.GetBindingDisplayString();
	}

	public bool CheatBlocker()
	{
		if (MonoSingleton<CheatsController>.Instance == null && CheatsManager.KeepCheatsEnabled)
		{
			return false;
		}
		if (MonoSingleton<CheatsController>.Instance == null || !MonoSingleton<CheatsController>.Instance.cheatsEnabled)
		{
			PrintLine("Cheats aren't enabled!", ConsoleLogType.Error);
			return true;
		}
		return false;
	}

	public void RegisterCommands(IEnumerable<ICommand> commands)
	{
		foreach (ICommand command in commands)
		{
			RegisterCommand(command);
		}
	}

	public void RegisterCommand(ICommand command)
	{
		if (registeredCommandTypes.Contains(command.GetType()))
		{
			PrintLine("Command " + command.GetType().Name + " already registered!");
			return;
		}
		recognizedCommands.Add(command.Command.ToLower(), command);
		registeredCommandTypes.Add(command.GetType());
	}

	public void PrintLine(string text)
	{
		PrintLine(text, ConsoleLogType.Log);
	}

	public void Clear()
	{
		scrollState = 0;
		errorCount = 0;
		warningCount = 0;
		infoCount = 0;
		logs.Clear();
		RepopulateLogs();
		UpdateScroller();
		errorBadge.SetEnabled(enabled: false, hide: false);
	}

	private void IncrementCounters(ConsoleLogType type)
	{
		if (scrollState > 0)
		{
			scrollState++;
			UpdateScroller();
		}
		switch (type)
		{
		case ConsoleLogType.Error:
			errorCount++;
			break;
		case ConsoleLogType.Warning:
			warningCount++;
			break;
		case ConsoleLogType.Log:
		case ConsoleLogType.Cli:
			infoCount++;
			break;
		}
	}

	public void PrintLine(string text, ConsoleLogType type, string stackTrace = "")
	{
		CapturedLog capturedLog = new CapturedLog(text, stackTrace, type);
		logs.Add(capturedLog);
		if (type == ConsoleLogType.Error)
		{
			onError?.Invoke();
		}
		if (PconClient.Instance != null)
		{
			PconClient.SendMessage(new Log(text, stackTrace, GetPconLogLevel(type)));
		}
		InsertLog(capturedLog);
	}

	public static PConLogLevel GetPconLogLevel(ConsoleLogType type)
	{
		return type switch
		{
			ConsoleLogType.Log => PConLogLevel.Info, 
			ConsoleLogType.Error => PConLogLevel.Error, 
			ConsoleLogType.Warning => PConLogLevel.Warning, 
			ConsoleLogType.Cli => PConLogLevel.CLI, 
			_ => PConLogLevel.Info, 
		};
	}

	public void UpdateFilters(bool showErrors, bool showWarnings, bool showLogs)
	{
		scrollState = 0;
		UpdateScroller();
		if (showErrors && showWarnings && showLogs)
		{
			filters = ConsoleLogType.All;
		}
		else
		{
			filters = (showErrors ? ConsoleLogType.Error : ConsoleLogType.None);
			filters |= (ConsoleLogType)(showWarnings ? 2 : 0);
			filters |= (ConsoleLogType)(showLogs ? 1 : 0);
		}
		RepopulateLogs();
	}

	public void ProcessUserInput(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		string[] array = text.Split(' ');
		if (array.Length == 0)
		{
			return;
		}
		string text2 = array[0];
		text2 = text2.ToLower();
		PrintLine("> " + text, ConsoleLogType.Cli);
		if (text.ToLower() == "sv_cheats 1")
		{
			PrintLine("To enable cheats, you must enter the Konami code in-game.", ConsoleLogType.Warning);
			return;
		}
		if (recognizedCommands.ContainsKey(text2))
		{
			try
			{
				recognizedCommands[text2].Execute(this, array.Skip(1).ToArray());
				return;
			}
			catch (Exception ex)
			{
				PrintLine("Command <b>'" + text2 + "'</b> failed.\n" + ex.Message, ConsoleLogType.Error, ex.StackTrace);
				return;
			}
		}
		PrintLine("Unknown command: '" + text2 + "'", ConsoleLogType.Warning);
	}

	private void ScrollUp()
	{
		timeSinceScrollTick = 0f;
		scrollState++;
		if (scrollState > logs.Count - 1)
		{
			scrollState = logs.Count - 1;
		}
		if (logs.Count == 0)
		{
			scrollState = 0;
		}
		UpdateScroller();
		RepopulateLogs();
	}

	private void ScrollDown()
	{
		timeSinceScrollTick = 0f;
		scrollState--;
		if (scrollState < 0)
		{
			scrollState = 0;
		}
		UpdateScroller();
		RepopulateLogs();
	}

	private void DefaultDevConsoleOff()
	{
	}

	private void OnGUI()
	{
		DefaultDevConsoleOff();
	}

	private void LateUpdate()
	{
		DefaultDevConsoleOff();
	}

	private void Update()
	{
		DefaultDevConsoleOff();
		bool activeSelf = consoleContainer.activeSelf;
		if (binds.OpenPressed || (consoleOpen && Input.GetKeyDown(KeyCode.Escape)))
		{
			consoleOpen = !consoleOpen;
			if (consoleOpen)
			{
				GameStateManager.Instance.RegisterState(new GameState("console", hideOnPin)
				{
					cursorLock = LockMode.Unlock,
					playerInputLock = LockMode.Lock,
					cameraInputLock = LockMode.Lock,
					priority = 100
				});
			}
			else
			{
				GameStateManager.Instance.PopState("console");
			}
			if (pinned)
			{
				GameObject[] array = hideOnPin;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(consoleOpen);
				}
				if (!consoleOpen)
				{
					array = hideOnPinNoReopen;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].SetActive(value: false);
					}
				}
				Image[] array2 = backgrounds;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].enabled = consoleOpen;
				}
			}
			else
			{
				consoleContainer.SetActive(consoleOpen);
			}
			masterGroup.interactable = consoleOpen;
			bool flag = activeSelf;
			if (pinned)
			{
				flag = !consoleOpen;
			}
			if (flag)
			{
				if ((bool)MonoSingleton<OptionsManager>.Instance && binds.OpenPressed && !openedDuringPause && MonoSingleton<OptionsManager>.Instance == rememberedOptionsManager && SceneManager.GetActiveScene().name != "Main Menu")
				{
					MonoSingleton<OptionsManager>.Instance.UnPause();
				}
				StopAllCoroutines();
			}
			else
			{
				if ((bool)MonoSingleton<OptionsManager>.Instance && SceneManager.GetActiveScene().name != "Main Menu")
				{
					openedDuringPause = MonoSingleton<OptionsManager>.Instance.paused;
					rememberedOptionsManager = MonoSingleton<OptionsManager>.Instance;
					MonoSingleton<OptionsManager>.Instance.Pause();
				}
				consoleBlocker.alpha = 0f;
				StartCoroutine(FadeBlockerIn());
				consoleInput.ActivateInputField();
				errorBadge.Dismiss();
			}
		}
		if (!consoleOpen)
		{
			return;
		}
		if (binds.ScrollUpPressed || Input.mouseScrollDelta.y > 0f)
		{
			timeSincePgHeld = 0f;
			ScrollUp();
		}
		if (binds.ScrollDownPressed || Input.mouseScrollDelta.y < 0f)
		{
			timeSincePgHeld = 0f;
			ScrollDown();
		}
		if ((binds.ScrollUpHeld || binds.ScrollDownHeld) && (float)timeSincePgHeld > 0.5f)
		{
			bool scrollUpHeld = binds.ScrollUpHeld;
			if ((float)timeSinceScrollTick > 0.05f)
			{
				if (scrollUpHeld)
				{
					ScrollUp();
				}
				else
				{
					ScrollDown();
				}
			}
		}
		if (binds.ScrollToTopPressed)
		{
			scrollState = logs.Count - 1;
			UpdateScroller();
			RepopulateLogs();
		}
		if (binds.ScrollToBottomPressed)
		{
			scrollState = 0;
			UpdateScroller();
			RepopulateLogs();
		}
		if (binds.CommandHistoryUpPressed)
		{
			commandHistoryIndex++;
			if (commandHistoryIndex > commandHistory.Count - 1)
			{
				commandHistoryIndex = commandHistory.Count - 1;
			}
			consoleInput.text = ((commandHistoryIndex == -1) ? "" : commandHistory[commandHistoryIndex]);
			consoleInput.caretPosition = consoleInput.text.Length;
		}
		if (binds.CommandHistoryDownPressed)
		{
			commandHistoryIndex--;
			if (commandHistoryIndex < -1)
			{
				commandHistoryIndex = -1;
			}
			consoleInput.text = ((commandHistoryIndex == -1) ? "" : commandHistory[commandHistoryIndex]);
			consoleInput.caretPosition = consoleInput.text.Length;
		}
		if (binds.SubmitPressed)
		{
			consoleInput.ActivateInputField();
			if (!string.IsNullOrEmpty(consoleInput.text))
			{
				ProcessUserInput(consoleInput.text);
				commandHistory = commandHistory.Prepend(consoleInput.text).ToList();
				commandHistoryIndex = -1;
				consoleInput.text = string.Empty;
			}
		}
	}

	private void UpdateScroller()
	{
		if (scrollState == 0)
		{
			scroller.SetActive(value: false);
			return;
		}
		scroller.SetActive(value: true);
		scrollText.text = $"{scrollState} lines below";
	}

	private IEnumerator FadeBlockerIn()
	{
		consoleBlocker.alpha = 0f;
		while (consoleBlocker.alpha < 1f)
		{
			consoleBlocker.alpha += 0.2f;
			yield return new WaitForSecondsRealtime(0.03f);
		}
		consoleBlocker.alpha = 1f;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	private void InsertLog(CapturedLog log)
	{
		IncrementCounters(log.type);
		RepopulateLogs();
	}

	private void RepopulateLogs()
	{
		List<CapturedLog> list = ((filters == ConsoleLogType.All) ? logs : filteredLogs);
		for (int i = 0; i < logLinePool.Count; i++)
		{
			if (list.Count - i - 1 - scrollState < 0)
			{
				logLinePool[logLinePool.Count - i - 1].gameObject.SetActive(value: false);
			}
			else if (logLinePool.Count - i - 1 >= 0)
			{
				logLinePool[logLinePool.Count - i - 1].gameObject.SetActive(value: true);
				logLinePool[logLinePool.Count - i - 1].PopulateLine(list[list.Count - i - 1 - scrollState]);
			}
		}
	}

	private void HandleLog(string message, string stacktrace, LogType type)
	{
		DefaultDevConsoleOff();
		CapturedLog capturedLog = new CapturedLog(message, stacktrace, type);
		logs.Add(capturedLog);
		InsertLog(capturedLog);
		if (type == LogType.Error || type == LogType.Exception)
		{
			onError?.Invoke();
		}
	}
}
