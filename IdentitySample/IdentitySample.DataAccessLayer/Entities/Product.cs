using System.ComponentModel.DataAnnotations;
using IdentitySample.DataAccessLayer.Entities.Common;

namespace IdentitySample.DataAccessLayer.Entities;

public class Product : TenantEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public double Price { get; set; }
}
