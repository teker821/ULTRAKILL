namespace Discord;

public struct ImageHandle
{
	public ImageType Type;

	public long Id;

	public uint Size;

	public static ImageHandle User(long id)
	{
		return User(id, 128u);
	}

	public static ImageHandle User(long id, uint size)
	{
		ImageHandle result = default(ImageHandle);
		result.Type = ImageType.User;
		result.Id = id;
		result.Size = size;
		return result;
	}
}
