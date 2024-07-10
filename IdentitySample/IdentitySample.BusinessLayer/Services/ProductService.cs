using IdentitySample.Contracts;
using IdentitySample.DataAccessLayer;
using IdentitySample.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Entities = IdentitySample.DataAccessLayer.Entities;

namespace IdentitySample.BusinessLayer.Services;

public class ProductService(DataContext dataContext, IUserService userService) : IProductService
{
    public async Task<IEnumerable<Product>> GetAsync()
    {
        var products = await dataContext.Products.OrderBy(p => p.Name)
            .Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            }).ToListAsync();

        return products;
    }

    public async Task SaveAsync(Product product)
    {
        var dbProduct = new Entities.Product
        {
            Name = product.Name,
            Price = product.Price
        };

        dataContext.Products.Add(dbProduct);
        await dataContext.SaveChangesAsync();
    }
}
