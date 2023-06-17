using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomMusicFileBrowser : DirectoryTreeBrowser<FileInfo>
{
	[SerializeField]
	private CustomMusicPlaylistEditor playlistEditor;

	[SerializeField]
	private Text songName;

	[SerializeField]
	private Image songIcon;

	[SerializeField]
	private Sprite defaultIcon;

	private AudioClip selectedClip;

	protected Dictionary<string, AudioType> extensionTypeDict = new Dictionary<string, AudioType>
	{
		{
			".wav",
			AudioType.WAV
		},
		{
			".mp3",
			AudioType.MPEG
		},
		{
			".ogg",
			AudioType.OGGVORBIS
		}
	};

	private AudioClip currentSong;

	protected override int maxPageLength => 5;

	protected override IDirectoryTree<FileInfo> baseDirectory => new FileDirectoryTree(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "CyberGrind", "Music"));

	private void LoadSong(FileInfo file)
	{
		StartCoroutine(SongLoadRoutine(file));
	}

	private void SelectSong(AudioClip clip, string name)
	{
		songName.text = name.ToUpper();
		songIcon.sprite = defaultIcon;
		selectedClip?.UnloadAudioData();
		selectedClip = clip;
	}

	public void ConfirmSong()
	{
	}

	private void SetToDefault()
	{
		songName.text = "NO SONG SELECTED";
		songIcon.sprite = defaultIcon;
	}

	private IEnumerator SongLoadRoutine(FileInfo file)
	{
		SelectSong(null, "LOADING...");
		if (extensionTypeDict.TryGetValue(file.Extension.ToLower(), out var value))
		{
			AudioType audioType = value;
			using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(file.FullName.ToUpper(), audioType);
			yield return request.SendWebRequest();
			AudioClip content = DownloadHandlerAudioClip.GetContent(request);
			SelectSong(content, file.Name);
		}
		else
		{
			SelectSong(null, "ERROR: INVALID FILE FORMAT");
		}
	}

	protected override Action BuildLeaf(FileInfo file, int indexInPage)
	{
		if (extensionTypeDict.ContainsKey(file.Extension.ToLower()))
		{
			GameObject go = UnityEngine.Object.Instantiate(itemButtonTemplate, itemParent, worldPositionStays: false);
			go.GetComponentInChildren<Text>().text = file.Name;
			go.GetComponent<Button>().onClick.AddListener(delegate
			{
				LoadSong(file);
			});
			go.SetActive(value: true);
			return delegate
			{
				UnityEngine.Object.Destroy(go);
			};
		}
		return null;
	}
}
