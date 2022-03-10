namespace Domain.Entities;

public class Operation
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public float Amount { get; set; }
    public string Description { get; set; }
    public OperationTag Tag { get; set; }
    public Report Report { get; set; }
}