namespace Domain.Entities;

public class Report
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public float Budget { get; set; }
    public int Days { get; set; }
    public ICollection<ReportConstant> Constants { get; set; }
    public ICollection<Operation> Operations { get; set; }
}