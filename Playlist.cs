using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class Playlist
{
	public enum LoopMode
	{
		Loop,
		LoopOne
	}

	public class SongData
	{
		public string name;

		public Sprite icon;

		public List<AudioClip> clips;

		public AudioClip introClip;

		public int maxClips;

		public SongData(string name, Sprite icon, AudioClip introClip, List<AudioClip> clips, int maxClips)
		{
			this.name = name;
			this.icon = icon;
			this.introClip = introClip;
			this.clips = clips.ToList();
			this.maxClips = maxClips;
		}
	}

	[JsonProperty]
	private List<string> _ids = new List<string>();

	[JsonProperty]
	private LoopMode _loopMode = LoopMode.LoopOne;

	[JsonProperty]
	private int _selected;

	[JsonProperty]
	private bool _shuffled = true;

	private Dictionary<string, SongData> loadedSongDict = new Dictionary<string, SongData>();

	public ReadOnlyCollection<string> ids => _ids.AsReadOnly();

	public LoopMode loopMode
	{
		get
		{
			return _loopMode;
		}
		set
		{
			_loopMode = value;
			this.OnChanged?.Invoke();
		}
	}

	public int selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
			this.OnChanged?.Invoke();
		}
	}

	public bool shuffled
	{
		get
		{
			return _shuffled;
		}
		set
		{
			_shuffled = value;
			this.OnChanged?.Invoke();
		}
	}

	public bool AllSongsLoaded => _ids.Where(IsSongLoaded).Count() > 0;

	public int Count => _ids.Count;

	public event Action OnChanged;

	public bool IsSongLoaded(string id)
	{
		return loadedSongDict.ContainsKey(id);
	}

	public bool GetSongData(string id, out SongData data)
	{
		return loadedSongDict.TryGetValue(id, out data);
	}

	public void Remove(int index)
	{
		if (index >= 0 && index < _ids.Count)
		{
			if (_ids.Count > 1)
			{
				_ids.RemoveAt(index);
				this.OnChanged?.Invoke();
			}
			else
			{
				Debug.LogWarning("Attempted to remove last song from playlist!");
			}
		}
		else
		{
			Debug.LogWarning($"Attempted to remove index '{index}' from playlist, which is out of bounds.");
		}
	}

	public void Swap(int index1, int index2)
	{
		string value = _ids[index1];
		_ids[index1] = _ids[index2];
		_ids[index2] = value;
		this.OnChanged?.Invoke();
	}

	public void AddCustomSong(string filePath)
	{
	}

	public void AddSoundtrackSong(string id, SoundtrackSong song)
	{
		if (song == null)
		{
			Debug.LogWarning("Attempted to add null song to playlist, ID '" + id + "'. Ignoring...");
			return;
		}
		_ids.Add(id);
		if (!loadedSongDict.ContainsKey(id))
		{
			loadedSongDict.Add(id, new SongData(song.songName + " <color=grey>" + song.extraLevelBit + "</color>", song.icon, song.introClip, song.clips, song.maxClipsIfNotRepeating));
		}
		this.OnChanged?.Invoke();
	}
}
