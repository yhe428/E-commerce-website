using onlineshop.Models.Db;
using onlineshop.Models.ViewModels;

namespace onlineshop.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync(int page, int pageSize);
        Task<int> GetProductCountAsync();
        Task<List<Product>> SearchProductsAsync(string searchText, int page, int pageSize);
        Task<int> GetSearchProductCountAsync(string searchText);
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<ProductGalery>> GetProductGalleryAsync(int productId);
        Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 6);
        Task<List<Comment>> GetProductCommentsAsync(int productId);
        Task<bool> AddCommentAsync(Comment comment);
        Task<bool> ProductExistsAsync(int productId);
    }
}