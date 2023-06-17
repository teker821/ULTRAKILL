using System.Collections.Generic;

namespace GameConsole.Commands;

public class Prefs : CommandRoot
{
	public override string Name => "Prefs";

	public override string Description => "Interfaces with the PrefsManager.";

	public override string Command => "prefs";

	protected override void BuildTree(Console con)
	{
		Branch("get", delegate
		{
			Leaf("bool", delegate(string key)
			{
				con.PrintLine($"{key} = {MonoSingleton<PrefsManager>.Instance.GetBool(key)}");
			});
			Leaf("int", delegate(string key)
			{
				con.PrintLine($"{key} = {MonoSingleton<PrefsManager>.Instance.GetInt(key)}");
			});
			Leaf("float", delegate(string key)
			{
				con.PrintLine($"{key} = {MonoSingleton<PrefsManager>.Instance.GetFloat(key)}");
			});
			Leaf("string", delegate(string key)
			{
				con.PrintLine(key + " = " + MonoSingleton<PrefsManager>.Instance.GetString(key));
			});
		});
		Branch("set", delegate
		{
			Leaf("bool", delegate(string key, bool value)
			{
				con.PrintLine($"Set {key} to {value}");
				MonoSingleton<PrefsManager>.Instance.SetBool(key, value);
			});
			Leaf("int", delegate(string key, int value)
			{
				con.PrintLine($"Set {key} to {value}");
				MonoSingleton<PrefsManager>.Instance.SetInt(key, value);
			});
			Leaf("float", delegate(string key, float value)
			{
				con.PrintLine($"Set {key} to {value}");
				MonoSingleton<PrefsManager>.Instance.SetFloat(key, value);
			});
			Leaf("string", delegate(string key, string value)
			{
				con.PrintLine("Set " + key + " to " + value);
				MonoSingleton<PrefsManager>.Instance.SetString(key, value);
			});
		});
		Branch("get_local", delegate
		{
			Leaf("bool", delegate(string key)
			{
				con.PrintLine($"{key} = {MonoSingleton<PrefsManager>.Instance.GetBoolLocal(key)}");
			});
			Leaf("int", delegate(string key)
			{
				con.PrintLine($"{key} = {MonoSingleton<PrefsManager>.Instance.GetIntLocal(key)}");
			});
			Leaf("float", delegate(string key)
			{
				con.PrintLine($"{key} = {MonoSingleton<PrefsManager>.Instance.GetFloatLocal(key)}");
			});
			Leaf("string", delegate(string key)
			{
				con.PrintLine(key + " = " + MonoSingleton<PrefsManager>.Instance.GetStringLocal(key));
			});
		});
		Branch("set_local", delegate
		{
			Leaf("bool", delegate(string key, bool value)
			{
				con.PrintLine($"Set {key} to {value}");
				MonoSingleton<PrefsManager>.Instance.SetBoolLocal(key, value);
			});
			Leaf("int", delegate(string key, int value)
			{
				con.PrintLine($"Set {key} to {value}");
				MonoSingleton<PrefsManager>.Instance.SetIntLocal(key, value);
			});
			Leaf("float", delegate(string key, float value)
			{
				con.PrintLine($"Set {key} to {value}");
				MonoSingleton<PrefsManager>.Instance.SetFloatLocal(key, value);
			});
			Leaf("string", delegate(string key, string value)
			{
				con.PrintLine("Set " + key + " to " + value);
				MonoSingleton<PrefsManager>.Instance.SetStringLocal(key, value);
			});
		});
		Leaf("delete", delegate(string key)
		{
			con.PrintLine("Deleted " + key);
			MonoSingleton<PrefsManager>.Instance.DeleteKey(key);
		});
		Leaf("list_defaults", delegate
		{
			con.PrintLine("<b>Default Prefs:</b>");
			foreach (KeyValuePair<string, object> defaultValue in MonoSingleton<PrefsManager>.Instance.defaultValues)
			{
				con.PrintLine($"{defaultValue.Key} = {defaultValue.Value}");
			}
		});
		Leaf("list_cached", delegate
		{
			con.PrintLine("<b>Cached Prefs:</b>");
			foreach (KeyValuePair<string, object> item in MonoSingleton<PrefsManager>.Instance.prefMap)
			{
				con.PrintLine($"{item.Key} = {item.Value}");
			}
		});
		Leaf("list_cached_local", delegate
		{
			con.PrintLine("<b>Local Cached Prefs:</b>");
			foreach (KeyValuePair<string, object> item2 in MonoSingleton<PrefsManager>.Instance.localPrefMap)
			{
				con.PrintLine($"{item2.Key} = {item2.Value}");
			}
		});
		Leaf("last_played", delegate
		{
			con.PrintLine($"The game has been played {PrefsManager.monthsSinceLastPlayed} months ago last.\nThis is only valid per session.");
		});
	}
}
