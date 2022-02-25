using IdentitySample.Shared.Models;

namespace IdentitySample.BusinessLayer.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAsync();

    Task SaveAsync(Product product);
}