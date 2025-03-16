namespace User.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = null!;
    public DateTime Expires { get; set; } = DateTime.MinValue;
    public Guid UserId { get; set; } = Guid.Empty;
    public User User { get; set; } = null!;
}