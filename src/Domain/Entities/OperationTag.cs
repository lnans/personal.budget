namespace Domain.Entities;

public class OperationTag
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public string OwnerId { get; set; }

    public virtual User Owner { get; set; }
}