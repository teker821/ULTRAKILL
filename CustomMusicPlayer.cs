using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomMusicPlayer : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup panelGroup;

	[SerializeField]
	private Text panelText;

	[SerializeField]
	private Image panelIcon;

	[SerializeField]
	private MusicChanger changer;

	[SerializeField]
	private CustomMusicPlaylistEditor playlistEditor;

	[SerializeField]
	private Sprite defaultIcon;

	public float panelApproachTime;

	public float panelStayTime;

	private System.Random random = new System.Random();

	private bool stopped;

	public void OnEnable()
	{
		StartPlaylist();
	}

	public void StartPlaylist()
	{
		if (playlistEditor.playlist.Count < 1)
		{
			Debug.LogError("No songs in playlist, somehow. Not starting playlist routine...");
		}
		else
		{
			StartCoroutine(PlaylistRoutine());
		}
	}

	public void StopPlaylist()
	{
		stopped = true;
	}

	private IEnumerator ShowPanelRoutine(Playlist.SongData song)
	{
		panelText.text = song.name.ToUpper();
		panelIcon.sprite = ((song.icon != null) ? song.icon : defaultIcon);
		float time2 = 0f;
		while (time2 < panelApproachTime)
		{
			time2 += Time.deltaTime;
			panelGroup.alpha = time2 / panelApproachTime;
			yield return null;
		}
		panelGroup.alpha = 1f;
		yield return new WaitForSecondsRealtime(panelStayTime);
		time2 = panelApproachTime;
		while (time2 > 0f)
		{
			time2 -= Time.deltaTime;
			panelGroup.alpha = time2 / panelApproachTime;
			yield return null;
		}
		panelGroup.alpha = 0f;
	}

	private IEnumerator PlaylistRoutine()
	{
		WaitUntil themeNotPlaying = new WaitUntil(() => Application.isFocused && !MonoSingleton<MusicManager>.Instance.targetTheme.isPlaying);
		Playlist.SongData lastSong = null;
		bool first = true;
		Playlist playlist = playlistEditor.playlist;
		IEnumerable<string> enumerable;
		if (!playlist.shuffled)
		{
			IEnumerable<string> ids = playlist.ids;
			enumerable = ids;
		}
		else
		{
			IEnumerable<string> ids = new DeckShuffled<string>(playlist.ids);
			enumerable = ids;
		}
		IEnumerable<string> currentOrder = enumerable;
		if (playlist.loopMode == Playlist.LoopMode.LoopOne)
		{
			currentOrder = currentOrder.Skip(playlist.selected).Take(1);
		}
		while (!stopped)
		{
			if (currentOrder is DeckShuffled<string> deckShuffled)
			{
				deckShuffled.Reshuffle();
			}
			foreach (string item in currentOrder)
			{
				playlistEditor.playlist.GetSongData(item, out var currentSong);
				if (lastSong != currentSong)
				{
					StartCoroutine(ShowPanelRoutine(currentSong));
				}
				if (first)
				{
					if (currentSong.introClip != null)
					{
						changer.ChangeTo(currentSong.introClip);
						yield return themeNotPlaying;
					}
					first = false;
				}
				int i = 0;
				foreach (AudioClip clip in currentSong.clips)
				{
					if (playlistEditor.playlist.loopMode != Playlist.LoopMode.LoopOne && currentSong.maxClips > -1 && i >= currentSong.maxClips)
					{
						break;
					}
					changer.ChangeTo(clip);
					i++;
					yield return themeNotPlaying;
				}
				lastSong = currentSong;
				changer.ChangeTo(null);
				currentSong = null;
			}
			yield return null;
		}
	}
}
