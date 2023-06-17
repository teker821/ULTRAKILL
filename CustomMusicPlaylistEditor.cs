using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CustomMusicPlaylistEditor : DirectoryTreeBrowser<string>
{
	[SerializeField]
	private CustomMusicSoundtrackBrowser browser;

	[SerializeField]
	private Sprite defaultIcon;

	[SerializeField]
	private Sprite loopSprite;

	[SerializeField]
	private Sprite loopOnceSprite;

	[Header("UI Elements")]
	[SerializeField]
	private Image loopModeImage;

	[SerializeField]
	private Image shuffleImage;

	[SerializeField]
	private RectTransform selectedControls;

	[SerializeField]
	private List<Transform> anchors;

	public Playlist playlist = new Playlist();

	private Coroutine moveControlsRoutine;

	private Dictionary<Transform, Coroutine> changeAnchorRoutines = new Dictionary<Transform, Coroutine>();

	private List<Transform> buttons = new List<Transform>();

	public string PlaylistJsonPath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Preferences", "Playlist.json");

	protected override int maxPageLength => 4;

	protected override IDirectoryTree<string> baseDirectory => new FakeDirectoryTree<string>("Songs", playlist.ids);

	private string selectedSongId => playlist.ids[playlist.selected];

	private CustomContentButton currentButton => buttons.ElementAtOrDefault(playlist.selected % maxPageLength)?.GetComponent<CustomContentButton>();

	private void Start()
	{
		try
		{
			LoadPlaylist();
		}
		catch (JsonReaderException ex)
		{
			Debug.LogError("Error loading Playlist.json: '" + ex.Message + "'. Recreating file.");
			File.Delete(PlaylistJsonPath);
			LoadPlaylist();
		}
		Select(playlist.selected);
		SetLoopMode(playlist.loopMode);
		SetShuffle(playlist.shuffled);
		playlist.OnChanged += SavePlaylist;
	}

	private void OnDestroy()
	{
		playlist.OnChanged -= SavePlaylist;
	}

	public void SavePlaylist()
	{
		File.WriteAllText(PlaylistJsonPath, JsonConvert.SerializeObject(playlist));
	}

	public void LoadPlaylist()
	{
		Debug.Log("Loading Playlist");
		Playlist playlist = null;
		using (StreamReader streamReader = new StreamReader(File.Open(PlaylistJsonPath, FileMode.OpenOrCreate)))
		{
			playlist = JsonConvert.DeserializeObject<Playlist>(streamReader.ReadToEnd());
		}
		if (playlist == null)
		{
			Debug.Log("No saved playlist found. Creating default...");
			foreach (AssetReferenceSoundtrackSong item in browser.rootFolder)
			{
				AsyncOperationHandle<SoundtrackSong> handle = item.LoadAssetAsync();
				SoundtrackSong song = handle.WaitForCompletion();
				this.playlist.AddSoundtrackSong(item.AssetGUID, song);
				Addressables.Release(handle);
			}
		}
		else
		{
			this.playlist.loopMode = playlist.loopMode;
			this.playlist.selected = playlist.selected;
			this.playlist.shuffled = playlist.shuffled;
			foreach (string id in playlist.ids)
			{
				AsyncOperationHandle<SoundtrackSong> handle2 = new AssetReferenceSoundtrackSong(id).LoadAssetAsync();
				handle2.WaitForCompletion();
				this.playlist.AddSoundtrackSong(id, handle2.Result);
				Addressables.Release(handle2);
			}
		}
		Rebuild();
	}

	public void Remove()
	{
		playlist.Remove(playlist.selected);
		if (playlist.selected >= playlist.ids.Count)
		{
			Select(playlist.Count - 1);
		}
		Rebuild(setToPageZero: false);
	}

	public void MoveUp()
	{
		Move(-1);
	}

	public void MoveDown()
	{
		Move(1);
	}

	public void Move(int amount)
	{
		int num = playlist.selected % maxPageLength;
		int index = num + amount;
		bool flag = PageOf(playlist.selected) == PageOf(playlist.selected + amount);
		if (playlist.selected + amount >= 0 && playlist.selected + amount < playlist.ids.Count)
		{
			playlist.Swap(playlist.selected, playlist.selected + amount);
			if (flag)
			{
				ChangeAnchorOf(buttons[num], anchors[index]);
				ChangeAnchorOf(selectedControls, anchors[index]);
				ChangeAnchorOf(buttons[index], anchors[num]);
				CustomContentButton customContentButton = currentButton;
				buttons.RemoveAt(num);
				buttons.Insert(index, customContentButton.transform);
				Select(playlist.selected + amount, rebuild: false);
			}
			else
			{
				selectedControls.gameObject.SetActive(value: false);
				Select(playlist.selected + amount);
			}
		}
	}

	public void ToggleLoopMode()
	{
		SetLoopMode((playlist.loopMode == Playlist.LoopMode.Loop) ? Playlist.LoopMode.LoopOne : Playlist.LoopMode.Loop);
	}

	private void SetLoopMode(Playlist.LoopMode mode)
	{
		playlist.loopMode = mode;
		loopModeImage.sprite = ((playlist.loopMode == Playlist.LoopMode.Loop) ? loopSprite : loopOnceSprite);
	}

	public void ToggleShuffle()
	{
		SetShuffle(!playlist.shuffled);
	}

	private void SetShuffle(bool shuffle)
	{
		playlist.shuffled = shuffle;
		shuffleImage.color = (shuffle ? Color.white : Color.gray);
	}

	private void Select(int newIndex, bool rebuild = true)
	{
		if (newIndex < 0 || newIndex >= playlist.Count)
		{
			Debug.LogWarning("Attempted to set current index outside bounds of playlist");
			return;
		}
		bool num = PageOf(newIndex) == currentPage;
		if ((bool)currentButton)
		{
			currentButton.border.color = Color.white;
		}
		int selected = playlist.selected;
		playlist.selected = newIndex;
		if (PageOf(selected) < PageOf(newIndex))
		{
			ChangeAnchorOf(selectedControls, anchors.First(), 0f);
		}
		else if (PageOf(selected) > PageOf(newIndex))
		{
			ChangeAnchorOf(selectedControls, anchors.Last(), 0f);
		}
		if ((bool)currentButton)
		{
			currentButton.border.color = Color.red;
		}
		Transform transform = anchors[playlist.selected % maxPageLength];
		if (num)
		{
			selectedControls.gameObject.SetActive(value: true);
			ChangeAnchorOf(selectedControls, transform);
		}
		else
		{
			selectedControls.gameObject.SetActive(value: false);
			selectedControls.transform.position = transform.position;
		}
		if (rebuild)
		{
			Rebuild(setToPageZero: false);
		}
	}

	protected override Action BuildLeaf(string id, int currentIndex)
	{
		if (!playlist.IsSongLoaded(id))
		{
			Debug.LogWarning("Attempted to load playlist UI while song with ID '" + id + "' has not loaded. Ignoring...");
			return null;
		}
		playlist.GetSongData(id, out var data);
		GameObject go = UnityEngine.Object.Instantiate(itemButtonTemplate, itemButtonTemplate.transform.parent);
		CustomContentButton contentButton = go.GetComponent<CustomContentButton>();
		contentButton.text.text = data.name.ToUpper();
		contentButton.icon.sprite = ((data.icon != null) ? data.icon : defaultIcon);
		go.SetActive(value: true);
		ChangeAnchorOf(go.transform, anchors[currentIndex], 0f);
		buttons.Add(go.transform);
		if (PageOf(playlist.selected) == currentPage && contentButton == currentButton)
		{
			contentButton.border.color = Color.red;
			selectedControls.gameObject.SetActive(value: true);
			ChangeAnchorOf(selectedControls, anchors[currentIndex]);
			return delegate
			{
				selectedControls.gameObject.SetActive(value: false);
				UnityEngine.Object.Destroy(go);
			};
		}
		contentButton.button.onClick.AddListener(delegate
		{
			Select(buttons.IndexOf(contentButton.transform) + currentPage * maxPageLength);
		});
		return delegate
		{
			UnityEngine.Object.Destroy(go);
		};
	}

	public void ChangeAnchorOf(Transform obj, Transform anchor, float time = 0.15f)
	{
		if (changeAnchorRoutines.ContainsKey(obj))
		{
			if (changeAnchorRoutines[obj] != null)
			{
				StopCoroutine(changeAnchorRoutines[obj]);
			}
			changeAnchorRoutines.Remove(obj);
		}
		changeAnchorRoutines.Add(obj, StartCoroutine(ChangeAnchorOverTime()));
		IEnumerator ChangeAnchorOverTime()
		{
			float t = 0f;
			_ = obj.position;
			while (t < time && time > 0f)
			{
				obj.position = Vector3.MoveTowards(obj.position, anchor.position, Time.deltaTime * 2f);
				if (Vector3.Distance(obj.position, anchor.position) <= Mathf.Epsilon)
				{
					break;
				}
				yield return null;
			}
			obj.position = anchor.position;
		}
	}

	public override void Rebuild(bool setToPageZero = true)
	{
		foreach (KeyValuePair<Transform, Coroutine> changeAnchorRoutine in changeAnchorRoutines)
		{
			if (changeAnchorRoutine.Value != null)
			{
				StopCoroutine(changeAnchorRoutine.Value);
			}
		}
		changeAnchorRoutines.Clear();
		buttons.Clear();
		LayoutRebuilder.ForceRebuildLayoutImmediate(itemParent as RectTransform);
		base.Rebuild(setToPageZero);
	}
}
