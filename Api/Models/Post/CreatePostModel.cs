namespace Api.Models.Post;

public class CreatePostModel
{

    public Guid Id { get; set; }
    public string? Description { get; set; }
    public Guid AuthorId { get; set; } 
    public  List<MetaWithPath>? Contents { get; set; } = new List<MetaWithPath>();
}
public class CreatePostRequest
{
    public string? Description { get; set; } 
    public List<MetaDataModel> Contents { get; set; } = new List<MetaDataModel>();

}