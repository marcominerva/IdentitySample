using IdentitySample.BusinessLayer.Services;
using IdentitySample.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentitySample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService productService;

    public ProductsController(IProductService productService)
    {
        this.productService = productService;
    }

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
}
