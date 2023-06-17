using System.IO;
using UnityEngine;

public class BloodMapInstance
{
	public static string CatalogPath => Path.Combine(Application.temporaryCachePath, "BloodMapInstance", "catalog.json");

	public static string AddressablesPath => Path.Combine(Application.temporaryCachePath, "BloodMapInstance");

	public BloodMapInstance(string catalog, byte[] bundle, string bundleName)
	{
		Debug.Log("Unpacking Blood " + AddressablesPath);
		string text = Path.Combine(Application.temporaryCachePath, "BloodMapInstance");
		if (Directory.Exists(text))
		{
			Directory.Delete(text, recursive: true);
		}
		Directory.CreateDirectory(text);
		File.WriteAllText(Path.Combine(text, "catalog.json"), catalog);
		File.WriteAllBytes(Path.Combine(text, bundleName), bundle);
		Debug.Log("Unpacking Blood - Done");
	}

	public void Close()
	{
		Debug.Log("Removing blood temp files");
		string path = Path.Combine(Application.temporaryCachePath, "BloodMapInstance");
		if (Directory.Exists(path))
		{
			Directory.Delete(path, recursive: true);
		}
	}
}
