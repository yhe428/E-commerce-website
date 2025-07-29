using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using onlineshop.Models.Db;
using onlineshop.Models.ViewModels;

namespace onlineshop.Services
{
    public class ProductService : IProductService
    {
        private readonly OnlineShopContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductService> _logger;
        private const int CacheExpirationMinutes = 10;

        public ProductService(OnlineShopContext context, IMemoryCache cache, ILogger<ProductService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<Product>> GetProductsAsync(int page, int pageSize)
        {
            try
            {
                return await _context.Products
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products for page {Page}", page);
                return new List<Product>();
            }
        }

        public async Task<int> GetProductCountAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync("product_count", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes);
                    return await _context.Products.CountAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product count");
                return 0;
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string searchText, int page, int pageSize)
        {
            try
            {
                var query = _context.Products.Where(x =>
                    EF.Functions.Like(x.Title, "%" + searchText + "%") ||
                    EF.Functions.Like(x.Tags, "%" + searchText + "%"));

                return await query
                    .OrderBy(x => x.Title)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products with text {SearchText}", searchText);
                return new List<Product>();
            }
        }

        public async Task<int> GetSearchProductCountAsync(string searchText)
        {
            try
            {
                var query = _context.Products.Where(x =>
                    EF.Functions.Like(x.Title, "%" + searchText + "%") ||
                    EF.Functions.Like(x.Tags, "%" + searchText + "%"));

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search product count for {SearchText}", searchText);
                return 0;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                return await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by id {ProductId}", id);
                return null;
            }
        }

        public async Task<List<ProductGalery>> GetProductGalleryAsync(int productId)
        {
            try
            {
                return await _context.ProductGaleries
                    .Where(x => x.ProductId == productId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product gallery for product {ProductId}", productId);
                return new List<ProductGalery>();
            }
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 6)
        {
            try
            {
                return await _context.Products
                    .Where(x => x.Id != productId)
                    .OrderByDescending(x => x.Id)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related products for product {ProductId}", productId);
                return new List<Product>();
            }
        }

        public async Task<List<Comment>> GetProductCommentsAsync(int productId)
        {
            try
            {
                return await _context.Comments
                    .Where(x => x.ProductId == productId)
                    .OrderByDescending(x => x.CreateDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for product {ProductId}", productId);
                return new List<Comment>();
            }
        }

        public async Task<bool> AddCommentAsync(Comment comment)
        {
            try
            {
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment for product {ProductId}", comment.ProductId);
                return false;
            }
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            try
            {
                return await _context.Products.AnyAsync(p => p.Id == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if product exists {ProductId}", productId);
                return false;
            }
        }
    }
}