namespace PopeAI.Models.RoleIncome;

public class RoleIncomes
{
    [Key]
    public long RoleId {get; set;}
    public int Income {get; set;}
    public string RoleName {get; set;}
    public long PlanetId {get; set;}
    public DateTime LastPaidOut {get; set;}
}