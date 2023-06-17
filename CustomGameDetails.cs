public class CustomGameDetails
{
	public string uniqueIdentifier;

	public int levelNumber;

	public ulong? workshopId;

	public CampaignJson campaign;

	public BloodMapInstance mapInstance;

	public string campaignId => campaign?.uuid;
}
