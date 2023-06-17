using System;
using System.Collections.Generic;
using Logic;

namespace GameConsole.Commands;

public class MapVar : CommandRoot
{
	public override string Name => "MapVar";

	public override string Description => "Map variables";

	protected override void BuildTree(Console con)
	{
		Leaf("reset", delegate
		{
			MonoSingleton<MapVarManager>.Instance.ResetStores();
			con.PrintLine("Stores have been reset.");
		}, requireCheats: true);
		Leaf("stash_info", delegate
		{
			bool hasStashedStore = MonoSingleton<MapVarManager>.Instance.HasStashedStore;
			con.PrintLine("Stash exists: " + hasStashedStore);
		}, requireCheats: true);
		Leaf("stash_stores", delegate
		{
			MonoSingleton<MapVarManager>.Instance.StashStore();
			con.PrintLine("Stores have been stashed.");
		}, requireCheats: true);
		Leaf("restore_stash", delegate
		{
			MonoSingleton<MapVarManager>.Instance.RestoreStashedStore();
			con.PrintLine("Stores have been restored.");
		}, requireCheats: true);
		Leaf("list", delegate
		{
			List<VariableSnapshot> allVariables = MonoSingleton<MapVarManager>.Instance.GetAllVariables();
			foreach (VariableSnapshot item in allVariables)
			{
				con.PrintLine($"{item.name} ({GetFriendlyTypeName(item.type)}) - <color=orange>{item.value}</color>");
			}
			if (allVariables.Count == 0)
			{
				con.PrintLine("No map variables have been set.");
			}
		}, requireCheats: true);
		BuildBoolMenu("logging", () => MapVarManager.LoggingEnabled, delegate(bool value)
		{
			MapVarManager.LoggingEnabled = value;
		});
		Leaf("set_int", delegate(string variableName, int value)
		{
			MonoSingleton<MapVarManager>.Instance.SetInt(variableName, value);
		}, requireCheats: true);
		Leaf("set_bool", delegate(string variableName, bool value)
		{
			MonoSingleton<MapVarManager>.Instance.SetBool(variableName, value);
		}, requireCheats: true);
		Leaf("toggle_bool", delegate(string variableName)
		{
			MonoSingleton<MapVarManager>.Instance.SetBool(variableName, !(MonoSingleton<MapVarManager>.Instance.GetBool(variableName) ?? false));
		}, requireCheats: true);
		Leaf("set_float", delegate(string variableName, float value)
		{
			MonoSingleton<MapVarManager>.Instance.SetFloat(variableName, value);
		}, requireCheats: true);
		Leaf("set_string", delegate(string variableName, string value)
		{
			MonoSingleton<MapVarManager>.Instance.SetString(variableName, value);
		}, requireCheats: true);
	}

	public static string GetFriendlyTypeName(Type type)
	{
		if (type == typeof(int))
		{
			return "int";
		}
		if (type == typeof(float))
		{
			return "float";
		}
		if (type == typeof(string))
		{
			return "string";
		}
		if (type == typeof(bool))
		{
			return "bool";
		}
		return type.Name;
	}
}
