using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using UnityEngine;

public static class AgonyHelper
{
	public static MapDataRebuild LoadMapData(string path, bool withBundle = false)
	{
		if (!File.Exists(path))
		{
			Debug.LogError("Failed to load map data from " + path);
			return null;
		}
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		MapHeader mapHeader = JsonConvert.DeserializeObject<MapHeader>(binaryReader.ReadString());
		Debug.Log("Loading map " + mapHeader.uniqueIdentifier);
		byte[] data = binaryReader.ReadBytes(mapHeader.thumbSize);
		Texture2D texture2D = null;
		if (mapHeader.thumbSize > 0)
		{
			texture2D = new Texture2D(640, 480);
			texture2D.LoadImage(data);
			texture2D.Apply();
		}
		MapDataRebuild mapDataRebuild = new MapDataRebuild
		{
			uniqueId = mapHeader.uniqueIdentifier,
			author = mapHeader.author,
			description = mapHeader.description,
			name = mapHeader.name,
			version = mapHeader.version,
			path = path,
			thumbnail = texture2D,
			lastModified = File.GetLastWriteTime(path),
			placeholderPrefabs = mapHeader.placeholderPrefabs,
			catalog = mapHeader.catalog,
			bundleName = mapHeader.bundleName
		};
		if (withBundle)
		{
			mapDataRebuild.bundle = binaryReader.ReadBytes(mapHeader.bundleSize);
		}
		binaryReader.Close();
		fileStream.Close();
		return mapDataRebuild;
	}

	public static CustomGameContent[] GetLocalMaps(string path, LocalSortMode sortMode)
	{
		List<CustomGameContent> list = new List<CustomGameContent>();
		foreach (string item in from f in Directory.GetFiles(path, "*", SearchOption.AllDirectories)
			where f.EndsWith(".blood") || f.EndsWith(".cum")
			select f)
		{
			try
			{
				MapDataRebuild mapDataRebuild = LoadMapData(Path.Combine(path, item));
				if (mapDataRebuild != null && mapDataRebuild.catalog != null)
				{
					list.Add(new MapDataRebuild
					{
						name = mapDataRebuild.name,
						bundleName = mapDataRebuild.bundleName,
						author = mapDataRebuild.author,
						description = mapDataRebuild.description,
						path = mapDataRebuild.path,
						thumbnail = mapDataRebuild.thumbnail,
						lastModified = mapDataRebuild.lastModified,
						placeholderPrefabs = mapDataRebuild.placeholderPrefabs
					});
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed to load map {item}: {arg}");
			}
		}
		switch (sortMode)
		{
		case LocalSortMode.Name:
			list.Sort((CustomGameContent a, CustomGameContent b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
			break;
		case LocalSortMode.Date:
			list.Sort((CustomGameContent a, CustomGameContent b) => b.lastModified.CompareTo(a.lastModified));
			break;
		}
		return list.ToArray();
	}

	public static async Task<Item?> FetchWorkshopItemInfo(PublishedFileId id)
	{
		Item? result = await SteamUGC.QueryFileAsync(id);
		if (result.HasValue)
		{
			return result;
		}
		Debug.LogError("Invalid item Id");
		return null;
	}
}
