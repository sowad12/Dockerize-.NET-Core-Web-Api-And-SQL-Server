using main.Context;
using main.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace main.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            return await _context.Products.ToListAsync();
        }
        [HttpPost("seed-data")]
        public async Task<bool> SeedData()
        {
            var list = new List<Product>()
            {
                new Product() { Name = "Apple iPad", Price = 1000 },
                new Product() {  Name = "Samsung Smart TV", Price = 1500 },
                new Product() { Name = "Nokia 130", Price = 1200 }
        };
            await _context.AddRangeAsync(list);
            var res = await _context.SaveChangesAsync();
            return res > 0;
        }
    }
}
