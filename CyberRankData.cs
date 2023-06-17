using System;

[Serializable]
public class CyberRankData
{
	public int wave;

	public float[] preciseWavesByDifficulty;

	public int[] kills;

	public int[] style;

	public float[] time;

	public CyberRankData()
	{
		preciseWavesByDifficulty = new float[6];
		kills = new int[6];
		style = new int[6];
		time = new float[6];
	}
}
