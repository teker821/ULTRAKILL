using UnityEngine;

public class AnimatedTexture : MonoBehaviour
{
	[SerializeField]
	private int materialIndex;

	[SerializeField]
	private float delay;

	[SerializeField]
	private Texture2D[] framePool;

	[SerializeField]
	private TextureType textureType;

	private TimeSince counter;

	private int selector;

	private MaterialPropertyBlock block;

	private Renderer renderer;

	private static readonly int MainTexID = Shader.PropertyToID("_MainTex");

	private static readonly int EmissiveTexID = Shader.PropertyToID("_EmissiveTex");

	private int texID;

	private void Awake()
	{
		switch (textureType)
		{
		case TextureType.Main:
			texID = MainTexID;
			break;
		case TextureType.Emissive:
			texID = EmissiveTexID;
			break;
		}
		block = new MaterialPropertyBlock();
		renderer = GetComponent<Renderer>();
		renderer.GetPropertyBlock(block, materialIndex);
		counter = 0f;
	}

	private void Update()
	{
		if ((float)counter > delay)
		{
			if (selector >= framePool.Length)
			{
				selector = 0;
			}
			block.SetTexture(texID, framePool[selector]);
			renderer.SetPropertyBlock(block, materialIndex);
			selector++;
			counter = 0f;
		}
	}
}
