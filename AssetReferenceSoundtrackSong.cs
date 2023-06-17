using System;
using UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceSoundtrackSong : AssetReferenceT<SoundtrackSong>
{
	public AssetReferenceSoundtrackSong(string guid)
		: base(guid)
	{
	}
}
