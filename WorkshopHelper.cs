using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Steamworks;
using Steamworks.Ugc;
using UnityEngine;

public static class WorkshopHelper
{
	public static async Task<Item?> GetWorkshopItemInfo(ulong itemId)
	{
		return await SteamUGC.QueryFileAsync(itemId);
	}

	public static async Task<Item?> DownloadWorkshopMap(ulong itemId, [CanBeNull] Action promptForUpdate = null)
	{
		Item? item = await Item.GetAsync(itemId);
		if (!item.HasValue)
		{
			Debug.LogError("Failed to get workshop item info for " + itemId);
			return null;
		}
		Debug.Log("Title: " + item?.Title);
		Debug.Log($"IsInstalled: {item?.IsInstalled}");
		Debug.Log($"IsDownloading: {item?.IsDownloading}");
		Debug.Log($"IsDownloadPending: {item?.IsDownloadPending}");
		Debug.Log($"IsSubscribed: {item?.IsSubscribed}");
		Debug.Log($"NeedsUpdate: {item?.NeedsUpdate}");
		Debug.Log("Description: " + item?.Description);
		if (promptForUpdate != null && (item?.NeedsUpdate).Value)
		{
			promptForUpdate();
			return null;
		}
		if (!item.Value.IsInstalled)
		{
			Debug.Log($"Downloading workshop map {itemId}");
			if (!(await item.Value.DownloadAsync()))
			{
				Debug.LogError($"Failed to download workshop map {itemId}");
				return null;
			}
			Debug.Log($"Workshop map {itemId} downloaded successfully");
			item = await Item.GetAsync(itemId);
			if (!item.HasValue)
			{
				Debug.LogError("Failed to get workshop item info for " + itemId);
				return null;
			}
		}
		else
		{
			Debug.LogWarning($"Workshop map {itemId} was already downloaded");
		}
		return item;
	}
}
