using System.ComponentModel.DataAnnotations.Schema;

namespace IdentitySample.DataAccessLayer.Entities.Common;

public abstract class BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
}
