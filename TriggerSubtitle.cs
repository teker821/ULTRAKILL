using UnityEngine;

public class TriggerSubtitle : MonoBehaviour
{
	[SerializeField]
	[TextArea]
	private string caption;

	[SerializeField]
	private bool activateOnEnableIfNoTrigger = true;

	private Collider col;

	private void Awake()
	{
		Collider component = GetComponent<Collider>();
		if ((bool)component && component.isTrigger)
		{
			col = component;
		}
	}

	private void OnEnable()
	{
		if (!col && activateOnEnableIfNoTrigger)
		{
			PushCaption();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PushCaption();
		}
	}

	public void PushCaption()
	{
		PushCaptionOverride(caption);
	}

	public void PushCaptionOverride(string caption)
	{
		if ((bool)MonoSingleton<SubtitleController>.Instance)
		{
			MonoSingleton<SubtitleController>.Instance.DisplaySubtitle(caption);
		}
	}
}
