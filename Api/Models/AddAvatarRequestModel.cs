namespace Api.Models;

public class AddAvatarRequestModel
{
    public MetaDataModel Avatar { get; set; } = null!;
    public Guid UserId { get; set; }
}
