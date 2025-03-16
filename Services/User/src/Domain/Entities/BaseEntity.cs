using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace User.Domain.Entities;

[Index(nameof(Id), IsUnique = true)]
public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}