using System;
using System.Collections.Generic;

[Serializable]
public class SoundtrackFolder
{
	public string name;

	public List<AssetReferenceSoundtrackSong> songs;

	public SoundtrackFolder(string name, List<AssetReferenceSoundtrackSong> songs)
	{
		this.name = name;
		this.songs = songs;
	}
}
