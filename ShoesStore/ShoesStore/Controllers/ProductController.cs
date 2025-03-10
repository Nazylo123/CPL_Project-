﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesStore.IRepository;
using ShoesStore.Model;
using ShoesStore.ViewModel.RequestModel;

using ShoesStore.ViewModel.ResponseModel;

using WebApi.Data;

namespace ShoesStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProdudctRepository _productRepository;
        private AppDbContext _context;

		public ProductController(IProdudctRepository productRepository, AppDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

       

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Product>>> GetProductsByDatTest()
		{
			var products = await _context.Products
	   .Include(p => p.Category) // Liên kết với bảng Category
	   .Include(p => p.ProductSizeStocks) // Liên kết đến ProductSizeStock
            .ThenInclude(ps => ps.Size)
	   .Include(p => p.ProductImages) // Liên kết với bảng ProductImages
	   .Select(p => new ProductViewModel
	   {
		   Id = p.Id,
		   Name = p.Name,
		   Description = p.Description,
		   Price = p.Price,
		   CategoryId = p.CategoryId,
		   CategoryName = p.Category.Name,
		   CreatedAt = p.CreatedAt,
		   UpdatedAt = p.UpdatedAt,

		   // Lấy danh sách SizeId, SizeName, và Quantity từ ProductSizeStocks
		   SizeId = p.ProductSizeStocks.Select(pss => pss.SizeId).ToList(),
		   SizeName = p.ProductSizeStocks.Select(pss => pss.Size.SizeName).ToList(),
		   Quantity = p.ProductSizeStocks.Select(pss => pss.Quantity).ToList(),

		   // Dữ liệu hình ảnh
		   
		   Url = p.ProductImages.FirstOrDefault().ImageUrl
	   })
	   .ToListAsync();

			return Ok(products);
		}




		//	[HttpGet("get-by-mun")]
		//	public async Task<ActionResult<IEnumerable<ProductViewModel>>> GetProductsByMun(
		//[FromQuery] int pageNumber = 1,
		//[FromQuery] int pageSize = 6)
		//	{
		//		// Validate pageNumber and pageSize
		//		if (pageNumber <= 0 || pageSize <= 0)
		//		{
		//			return BadRequest("PageNumber and PageSize must be greater than 0.");
		//		}

		//		// Query with pagination
		//		var query = _context.Products
		//			.Include(p => p.Category) // Liên kết với bảng Category
		//			.Include(p => p.ProductSizeStocks) // Liên kết đến ProductSizeStock
		//				.ThenInclude(ps => ps.Size)
		//			.Include(p => p.ProductImages) // Liên kết với bảng ProductImages
		//			.Select(p => new ProductViewModel
		//			{
		//				Id = p.Id,
		//				Name = p.Name,
		//				Description = p.Description,
		//				Price = p.Price,
		//				CategoryId = p.CategoryId,
		//				CategoryName = p.Category.Name,
		//				CreatedAt = p.CreatedAt,
		//				UpdatedAt = p.UpdatedAt,

		//				// Lấy danh sách SizeId, SizeName, và Quantity từ ProductSizeStocks
		//				SizeId = p.ProductSizeStocks.Select(pss => pss.SizeId).ToList(),
		//				SizeName = p.ProductSizeStocks.Select(pss => pss.Size.SizeName).ToList(),
		//				Quantity = p.ProductSizeStocks.Select(pss => pss.Quantity).ToList(),

		//				// Dữ liệu hình ảnh
		//				Url = p.ProductImages.FirstOrDefault().ImageUrl
		//			});

		//		// Get total count
		//		var totalCount = await query.CountAsync();

		//		// Apply pagination
		//		var products = await query
		//			.Skip((pageNumber - 1) * pageSize)
		//			.Take(pageSize)
		//			.ToListAsync();

		//		// Return paginated response
		//		return Ok(new
		//		{
		//			TotalCount = totalCount,
		//			PageNumber = pageNumber,
		//			PageSize = pageSize,
		//			TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
		//			Data = products
		//		});
		//	}
		[HttpGet("get-by-mun")]
		public async Task<ActionResult<IEnumerable<ProductViewModel>>> GetProductsByMun(
	[FromQuery] int pageNumber = 1,
	[FromQuery] int pageSize = 6,
	[FromQuery] decimal? minPrice = null,  // Lọc theo giá tối thiểu
	[FromQuery] decimal? maxPrice = null,  // Lọc theo giá tối đa
	[FromQuery] string searchTerm = null,  // Lọc theo tên sản phẩm
	[FromQuery] List<int> sizeIds = null,  // Lọc theo danh sách kích thước
	[FromQuery] string categoryName = null // Lọc theo tên thể loại
)
		{
			// Validate pageNumber and pageSize
			if (pageNumber <= 0 || pageSize <= 0)
			{
				return BadRequest("PageNumber and PageSize must be greater than 0.");
			}

			// Query with pagination and filters
			var query = _context.Products
				.Include(p => p.Category) // Liên kết với bảng Category
				.Include(p => p.ProductSizeStocks) // Liên kết đến ProductSizeStock
					.ThenInclude(ps => ps.Size)
				.Include(p => p.ProductImages) // Liên kết với bảng ProductImages
				.AsQueryable(); // Sử dụng AsQueryable để có thể linh động thêm điều kiện lọc

			// Lọc theo giá
			if (minPrice.HasValue)
			{
				query = query.Where(p => p.Price >= minPrice.Value);
			}

			if (maxPrice.HasValue)
			{
				query = query.Where(p => p.Price <= maxPrice.Value);
			}

			// Lọc theo tên sản phẩm
			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(p => p.Name.Contains(searchTerm));
			}

			// Lọc theo tên thể loại
			if (!string.IsNullOrEmpty(categoryName))
			{
				query = query.Where(p => p.Category.Name.Contains(categoryName));
			}

			// Lọc theo danh sách kích thước
			if (sizeIds != null && sizeIds.Any())
			{
				query = query.Where(p => p.ProductSizeStocks.Any(ps => sizeIds.Contains(ps.SizeId)));
			}

			// Chọn dữ liệu để trả về
			var productsQuery = query.Select(p => new ProductViewModel
			{
				Id = p.Id,
				Name = p.Name,
				Description = p.Description,
				Price = p.Price,
				CategoryId = p.CategoryId,
				CategoryName = p.Category.Name,
				CreatedAt = p.CreatedAt,
				UpdatedAt = p.UpdatedAt,

				// Lấy danh sách SizeId, SizeName, và Quantity từ ProductSizeStocks
				SizeId = p.ProductSizeStocks.Select(pss => pss.SizeId).ToList(),
				SizeName = p.ProductSizeStocks.Select(pss => pss.Size.SizeName).ToList(),
				Quantity = p.ProductSizeStocks.Select(pss => pss.Quantity).ToList(),

				// Dữ liệu hình ảnh
				Url = p.ProductImages.FirstOrDefault().ImageUrl
			});

			// Get total count
			var totalCount = await productsQuery.CountAsync();

			// Apply pagination
			var products = await productsQuery
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Return paginated response
			return Ok(new
			{
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
				Data = products
			});
		}


		[HttpGet("get-categories")]
		public async Task<ActionResult<IEnumerable<string>>> GetCategories()
		{
			var categories = await _context.Categories
											.Take(5)
										   .Select(c => c.Name)
										   .ToListAsync();

			return Ok(categories);
		}


		[HttpGet("search-suggestions")]
		public async Task<ActionResult<IEnumerable<string>>> GetSearchSuggestions([FromQuery] string searchTerm)
		{
			// Kiểm tra input
			if (string.IsNullOrEmpty(searchTerm))
			{
				return Ok(new List<string>());
			}

			// Lấy danh sách sản phẩm phù hợp với từ khóa
			var suggestions = await _context.Products
				.Where(p => p.Name.Contains(searchTerm)) // Tìm kiếm tên sản phẩm
				.OrderBy(p => p.Name) // Sắp xếp theo tên
				.Select(p => p.Name) // Chỉ lấy tên sản phẩm
				.Take(10) // Giới hạn gợi ý tối đa
				.ToListAsync();

			return Ok(suggestions);
		}



        // POST: api/Product
        [HttpPost("{categoryID}")]
        public async Task<ActionResult> CreateProduct([FromBody] ProductRequestModel productRequest, int categoryID)
        {
            if (productRequest == null)
                return BadRequest(new { message = "Invalid product request." });

            if (string.IsNullOrWhiteSpace(productRequest.Name) ||
                productRequest.Price <= 0 ||
                productRequest.SizeQuantities == null || !productRequest.SizeQuantities.Any() ||
                productRequest.ImageUrls == null || !productRequest.ImageUrls.Any())
            {
                return BadRequest(new { message = "Product request contains invalid or missing data." });
            }

            var categoryExists = await _context.Categories.AsNoTracking()
                .AnyAsync(c => c.Id == categoryID);
            if (!categoryExists)
                return NotFound(new { message = "Category not found." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = new Product
                {
                    Name = productRequest.Name,
                    Description = productRequest.Description,
                    Price = productRequest.Price,
                    CategoryId = categoryID,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ProductSizeStocks = productRequest.SizeQuantities.Select(sq => new ProductSizeStock
                    {
                        SizeId = sq.SizeId,
                        Quantity = sq.Quantity
                    }).ToList(),
                    ProductImages = productRequest.ImageUrls.Select(url => new ProductImage
                    {
                        ImageUrl = url
                    }).ToList()
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Product created successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Internal server error.", details = ex.Message });
            }
        }


        [HttpDelete("{productId}")]
        public async Task<ActionResult> DeleteProduct(int productId)
        {
            try
            {
                bool result = await _productRepository.DeleteProductAsync(productId);
                if (result)
                {
                    return Ok("Deleted successfully.");
                }
                return NotFound($"Product with ID {productId} not found.");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Error: {e.Message}");
            }
        }
        [HttpGet("{productId}")]
        public async Task<ActionResult<ProductReponseViewModel>> GetProduct(int productId)
        {
            try
            {
                var product = await _productRepository.GetProductAsync(productId);

                if (product == null)
                {
                    return NotFound($"Product with ID {productId} not found.");
                }

                return Ok(product);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal Server Error: {e.Message}");
            }
        }

        [HttpPut("{productId}")]
        public async Task<ActionResult> UpdateProduct(int productId, [FromBody] ProductRequestModel productRequest)
        {
            if (productRequest == null)
                return BadRequest(new { message = "Invalid product request." });

            var existingProduct = await _context.Products
                .Include(p => p.ProductSizeStocks)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (existingProduct == null)
                return NotFound(new { message = "Product not found." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Cập nhật thông tin cơ bản
                existingProduct.Name = productRequest.Name;
                existingProduct.Description = productRequest.Description;
                existingProduct.Price = productRequest.Price;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                // Xóa các liên kết cũ
                _context.ProductSizeStocks.RemoveRange(existingProduct.ProductSizeStocks);
                _context.ProductImages.RemoveRange(existingProduct.ProductImages);

                // Thêm các liên kết mới
                existingProduct.ProductSizeStocks = productRequest.SizeQuantities.Select(sq => new ProductSizeStock
                {
                    SizeId = sq.SizeId,
                    Quantity = sq.Quantity
                }).ToList();

                existingProduct.ProductImages = productRequest.ImageUrls.Select(url => new ProductImage
                {
                    ImageUrl = url
                }).ToList();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Product updated successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Internal server error.", details = ex.Message });
            }
        }
    }
}
