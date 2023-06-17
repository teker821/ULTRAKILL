using UnityEngine;
using UnityEngine.UI;

namespace GameConsole;

public class LogLine : MonoBehaviour
{
	[SerializeField]
	private Text timestamp;

	[SerializeField]
	private Text message;

	[SerializeField]
	private Image mainPanel;

	[Space]
	[SerializeField]
	private CanvasGroup attentionFlashGroup;

	[Space]
	[SerializeField]
	private Color normalLogColor;

	[SerializeField]
	private Color warningLogColor;

	[SerializeField]
	private Color errorLogColor;

	[SerializeField]
	private Color cliLogColor;

	[Space]
	[SerializeField]
	private float normalHeight = 35f;

	[SerializeField]
	private float expandedHeight = 120f;

	private RectTransform rectTransform;

	private CapturedLog log;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void Wipe()
	{
		log = null;
		timestamp.text = "";
		message.text = "";
		mainPanel.color = normalLogColor;
		RefreshSize();
	}

	public void PopulateLine(CapturedLog log)
	{
		this.log = log;
		timestamp.text = $"{log.time:HH:mm:ss.f}";
		RefreshSize();
		if (log.expanded && !string.IsNullOrEmpty(log.stackTrace))
		{
			string stackTrace = log.stackTrace;
			stackTrace = stackTrace.Replace("\n", "");
			message.text = "<b>" + log.message + "</b>\n" + stackTrace;
		}
		else
		{
			message.text = log.message;
		}
		mainPanel.color = ((log.type == ConsoleLogType.Log) ? normalLogColor : ((log.type == ConsoleLogType.Warning) ? warningLogColor : ((log.type == ConsoleLogType.Cli) ? cliLogColor : errorLogColor)));
		if ((float)log.timeSinceLogged < 0.5f && base.gameObject.activeInHierarchy)
		{
			attentionFlashGroup.alpha = TimeSinceToFlashAlpha(log.timeSinceLogged);
		}
	}

	public void ToggleExpand()
	{
		log.expanded = !log.expanded;
		RefreshSize();
		PopulateLine(log);
	}

	private void RefreshSize()
	{
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		if (log == null || !log.expanded)
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, normalHeight);
		}
		else
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, expandedHeight);
		}
	}

	private void Update()
	{
		if (log != null)
		{
			if ((float)log.timeSinceLogged > 0.5f)
			{
				attentionFlashGroup.alpha = 0f;
			}
			else
			{
				attentionFlashGroup.alpha = TimeSinceToFlashAlpha(log.timeSinceLogged);
			}
		}
	}

	private float TimeSinceToFlashAlpha(float timeSinceLogged)
	{
		if (timeSinceLogged < 0.2f)
		{
			return timeSinceLogged / 0.2f;
		}
		return 1f - (timeSinceLogged - 0.2f) / 0.3f;
	}
}
