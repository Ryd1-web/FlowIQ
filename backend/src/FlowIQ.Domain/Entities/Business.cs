namespace FlowIQ.Domain.Entities;

public class Business : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Address { get; set; }

    // Foreign key
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Navigation
    public ICollection<Income> Incomes { get; set; } = new List<Income>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
