using bicycleBackend.Data;
using bicycleBackend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace bicycleBackend.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly string _imagePath;

        public ProductsController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _imagePath = Path.Combine(env.ContentRootPath, "wwwroot/images");

            // ✅ Proveravamo da li folder postoji, ako ne - kreiramo ga
            if (!Directory.Exists(_imagePath))
            {
                Directory.CreateDirectory(_imagePath);
            }
        }

        // ✅ Metoda za dodavanje proizvoda
        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromForm] IFormCollection formData)
        {
            try
            {
                var name = formData["name"].ToString();
                var category = formData["category"].ToString();
                var description = formData["description"].ToString();

                // ✅ Validacija cene - koristimo TryParse da izbegnemo crash
                if (!decimal.TryParse(formData["price"], out var price) || price <= 0)
                    return BadRequest("Invalid price value.");

                var file = formData.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                    return BadRequest("No image uploaded.");

                // ✅ Generisanje jedinstvenog imena slike
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(_imagePath, fileName);

                // ✅ Čuvanje fajla na server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // ✅ Vraćamo URL slike sa potpunim URL-om
                var imageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";

                // ✅ Čuvanje proizvoda u bazi
                var product = new Product
                {
                    Name = name,
                    Price = price,
                    Category = category,
                    Description = description,
                    ImageUrl = imageUrl
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product added successfully!", product });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // ✅ Metoda za dohvaćanje svih proizvoda
        [HttpGet("all")]
        public IActionResult GetAllProducts()
        {
            var products = _context.Products.ToList();
            return Ok(products);
        }

        // ✅ Metoda za brisanje proizvoda
        // ✅ Metoda za brisanje proizvoda
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                // ✅ Proveravamo da li proizvod postoji u bazi
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found." });
                }

                // ✅ Brisanje slike sa servera
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    try
                    {
                        // ✅ Uzmi samo ime fajla iz URL-a
                        var fileName = Path.GetFileName(new Uri(product.ImageUrl).AbsolutePath);
                        var filePath = Path.Combine(_imagePath, fileName);

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Warning: Error deleting image file: {ex.Message}");
                    }
                }

                // ✅ Brisanje proizvoda iz baze
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
