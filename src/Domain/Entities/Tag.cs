namespace Domain.Entities;

public class Tag
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public string OwnerId { get; set; }

    public User Owner { get; set; }
}