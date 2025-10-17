using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UUIDNext;

namespace EMRS.Domain.Entities;

    public abstract class BaseEntity
{

    public Guid Id { get; set; } =
      Uuid.NewDatabaseFriendly(Database.PostgreSql);
    /*Guid.NewGuid();*/
    public DateTimeOffset? UpdatedAt { get; set; } = null!;
    public DateTimeOffset? DeletedAt { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
    public void Delete()
    {
        UpdatedAt = DateTime.UtcNow;
        DeletedAt = DateTime.UtcNow;
        IsDeleted = true;
    }

    public void Restore()
    {
        UpdatedAt = DateTime.UtcNow;
        DeletedAt = null;
        IsDeleted = false;
    }
}

