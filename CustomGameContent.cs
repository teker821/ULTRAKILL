using System;
using System.IO;
using UnityEngine;

public class CustomGameContent
{
	public string name;

	public string uniqueId;

	public string path;

	public Texture2D thumbnail;

	public DateTime lastModified;

	public string author;

	public string shortPath
	{
		get
		{
			if (!(this is CustomCampaign))
			{
				return Path.GetFileName(path);
			}
			return Path.GetFileName(path) + "/";
		}
	}
}
