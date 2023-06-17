using System.Collections.Generic;
using System.Reflection;

namespace GameConsole.Commands;

public class Rumble : CommandRoot
{
	public override string Name => "Rumble";

	public override string Description => "Command for managing ULTRAKILL's controller rumble system";

	protected override void BuildTree(Console con)
	{
		Leaf("status", delegate
		{
			con.PrintLine($"Pending Vibrations ({MonoSingleton<RumbleManager>.Instance.pendingVibrations.Count}):");
			foreach (KeyValuePair<string, PendingVibration> pendingVibration in MonoSingleton<RumbleManager>.Instance.pendingVibrations)
			{
				con.PrintLine($" - {pendingVibration.Key} ({pendingVibration.Value.Intensity}) for {pendingVibration.Value.Duration} seconds");
			}
			con.PrintLine(string.Empty);
			con.PrintLine($"Current Intensity: {MonoSingleton<RumbleManager>.Instance.currentIntensity}");
		});
		Leaf("list", delegate
		{
			con.PrintLine("Available Keys:");
			PropertyInfo[] properties = typeof(RumbleProperties).GetProperties();
			for (int i = 0; i < properties.Length; i++)
			{
				string text = properties[i].GetValue(null) as string;
				con.PrintLine(" - " + text);
			}
		});
		Leaf("vibrate", delegate(string key)
		{
			MonoSingleton<RumbleManager>.Instance.SetVibration(key);
		});
		Leaf("stop", delegate(string key)
		{
			MonoSingleton<RumbleManager>.Instance.StopVibration(key);
		});
		Leaf("stop_all", delegate
		{
			MonoSingleton<RumbleManager>.Instance.StopAllVibrations();
		});
		Leaf("toggle_preview", delegate
		{
			DebugUI.previewRumble = !DebugUI.previewRumble;
		});
	}
}
