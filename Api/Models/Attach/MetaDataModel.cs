namespace Api.Models;

public class MetaDataModel
{
    public Guid TempId {get; set;}
    public string Name { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long Size { get; set; }
}


public class MetaWithPath : MetaDataModel
{
    public string FilePath { get; set; } = null!;
    public MetaWithPath(MetaDataModel model, Func<MetaDataModel, string> pathgen)
    {
      TempId = model.TempId;
      Name = model.Name;
      MimeType = model.MimeType;
      Size = model.Size;
      FilePath = pathgen(model);

    }
}