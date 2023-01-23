namespace DAL.Entities;

public class Avatar : Attach
{
    public virtual User User { get; set; }
 
}
