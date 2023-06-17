using UnityEngine;

namespace GameConsole;

public class ConsoleFilters : MonoBehaviour
{
	[SerializeField]
	private float defaultOpacity = 1f;

	[SerializeField]
	private float hiddenOpacity = 0.1f;

	[Space]
	[SerializeField]
	private FilterButton errorsFilter;

	[SerializeField]
	private FilterButton warningsFilter;

	[SerializeField]
	private FilterButton logsFilter;

	private void Awake()
	{
		errorsFilter.SetOpacity(defaultOpacity);
		warningsFilter.SetOpacity(defaultOpacity);
		logsFilter.SetOpacity(defaultOpacity);
	}

	private void Update()
	{
		errorsFilter.text.text = $"errors ({MonoSingleton<Console>.Instance.errorCount})";
		warningsFilter.text.text = $"warnings ({MonoSingleton<Console>.Instance.warningCount})";
		logsFilter.text.text = $"logs ({MonoSingleton<Console>.Instance.infoCount})";
	}

	public void TogglePopup()
	{
		base.gameObject.SetActive(!base.gameObject.activeSelf);
	}

	private void UpdateFilters()
	{
		MonoSingleton<Console>.Instance.UpdateFilters(errorsFilter.active, warningsFilter.active, logsFilter.active);
	}

	public void ToggleErrorFiltering()
	{
		errorsFilter.active = !errorsFilter.active;
		errorsFilter.SetOpacity(errorsFilter.active ? defaultOpacity : hiddenOpacity);
		errorsFilter.SetCheckmark(errorsFilter.active);
		UpdateFilters();
	}

	public void ToggleWarningFiltering()
	{
		warningsFilter.active = !warningsFilter.active;
		warningsFilter.SetOpacity(warningsFilter.active ? defaultOpacity : hiddenOpacity);
		warningsFilter.SetCheckmark(warningsFilter.active);
		UpdateFilters();
	}

	public void ToggleLogFiltering()
	{
		logsFilter.active = !logsFilter.active;
		logsFilter.SetOpacity(logsFilter.active ? defaultOpacity : hiddenOpacity);
		logsFilter.SetCheckmark(logsFilter.active);
		UpdateFilters();
	}
}
