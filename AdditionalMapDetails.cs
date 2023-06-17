[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class AdditionalMapDetails : MonoSingleton<AdditionalMapDetails>
{
	public bool hasAuthorLinks;

	public AuthorLink[] authorLinks;
}
