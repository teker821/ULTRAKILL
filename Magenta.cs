using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class Magenta
{
	public string path;

	public string version;

	public readonly Dictionary<string, List<string>> bundles;

	public readonly Dictionary<string, string> assetsToBundles;

	public readonly Dictionary<string, List<string>> sceneDependencies;

	public readonly List<string> agonyDependencies;

	public Magenta(string path, bool loadUnhardened = true)
	{
		this.path = path;
		if (!Directory.Exists(path))
		{
			throw new DirectoryNotFoundException();
		}
		if (!File.Exists(Path.Combine(path, "magenta.json")))
		{
			throw new Exception("magenta.json not found");
		}
		Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(path, "magenta.json")));
		version = dictionary["version"];
		string value = File.ReadAllText(Path.Combine(path, "bundles.json"));
		bundles = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
		assetsToBundles = new Dictionary<string, string>();
		foreach (KeyValuePair<string, List<string>> bundle in bundles)
		{
			foreach (string item in bundle.Value)
			{
				if (!assetsToBundles.ContainsKey(item))
				{
					assetsToBundles.Add(item, bundle.Key);
				}
			}
		}
		if (loadUnhardened)
		{
			Dictionary<string, List<string>> unhardenedBundles = GetUnhardenedBundles();
			if (unhardenedBundles != null)
			{
				foreach (KeyValuePair<string, List<string>> item2 in unhardenedBundles)
				{
					if (bundles.ContainsKey(item2.Key))
					{
						continue;
					}
					bundles.Add(item2.Key, item2.Value);
					foreach (string item3 in item2.Value.Where((string asset) => !assetsToBundles.ContainsKey(asset)))
					{
						assetsToBundles.Add(item3, item2.Key);
					}
				}
			}
		}
		string text = Path.Combine(path, "sceneDependencies.json");
		if (File.Exists(text))
		{
			string value2 = File.ReadAllText(text);
			sceneDependencies = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value2);
		}
		string text2 = Path.Combine(path, "agonyDependencies.json");
		if (File.Exists(text2))
		{
			string value3 = File.ReadAllText(text2);
			agonyDependencies = JsonConvert.DeserializeObject<List<string>>(value3);
		}
	}

	public Dictionary<string, List<string>> GetUnhardenedBundles()
	{
		string text = Path.Combine(path, "unhardenedBundles.json");
		if (!File.Exists(text))
		{
			return null;
		}
		return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(text));
	}
}
