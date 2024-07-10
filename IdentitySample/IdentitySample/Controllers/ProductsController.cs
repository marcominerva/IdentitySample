using IdentitySample.BusinessLayer.Services;
using IdentitySample.Shared.Models;
using IdentitySample.StorageProviders;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var products = await productService.GetAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Save(Product product)
    {
        await productService.SaveAsync(product);
        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromServices] IStorageProvider storageProvider)
    {
        await storageProvider.SaveAsync($"products/images/{file.FileName}", file.OpenReadStream(), true);
        return NoContent();
    }
}
