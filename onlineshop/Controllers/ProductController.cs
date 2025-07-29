using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using onlineshop.Models.Db;
using onlineshop.Services;
using System.Text.RegularExpressions;

namespace onlineshop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private const int PageSize = 12; // Products per page

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var totalProducts = await _productService.GetProductCountAsync();
                var totalPages = (int)Math.Ceiling((double)totalProducts / PageSize);
                
                var products = await _productService.GetProductsAsync(page, PageSize);

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["HasPreviousPage"] = page > 1;
                ViewData["HasNextPage"] = page < totalPages;

                return View(products);
            }
            catch (Exception ex)
            {
                // Log error and return empty list
                return View(new List<Product>());
            }
        }

        public async Task<IActionResult> SearchProducts(string SearchText, int page = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    return RedirectToAction(nameof(Index));
                }

                var totalProducts = await _productService.GetSearchProductCountAsync(SearchText);
                var totalPages = (int)Math.Ceiling((double)totalProducts / PageSize);

                var products = await _productService.SearchProductsAsync(SearchText, page, PageSize);

                ViewData["SearchText"] = SearchText;
                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["HasPreviousPage"] = page > 1;
                ViewData["HasNextPage"] = page < totalPages;

                return View("Index", products);
            }
            catch (Exception ex)
            {
                return View("Index", new List<Product>());
            }
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                
                if (product == null)
                {
                    return NotFound();
                }

                // Load related data efficiently in parallel
                var galleryTask = _productService.GetProductGalleryAsync(id);
                var newProductsTask = _productService.GetRelatedProductsAsync(id, 6);
                var commentsTask = _productService.GetProductCommentsAsync(id);

                // Wait for all tasks to complete
                await Task.WhenAll(galleryTask, newProductsTask, commentsTask);

                ViewData["gallery"] = await galleryTask;
                ViewData["NewProducts"] = await newProductsTask;
                ViewData["comments"] = await commentsTask;

                return View(product);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComment(string name, string email, string comment, int productId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(comment) || productId <= 0)
                {
                    TempData["ErrorMessage"] = "Please complete your information";
                    return Redirect("/Product/ProductDetails/" + productId);
                }

                // Use more robust email validation
                if (!IsValidEmail(email))
                {
                    TempData["ErrorMessage"] = "Email is not valid";
                    return Redirect("/Product/ProductDetails/" + productId);
                }

                // Verify product exists
                var productExists = await _productService.ProductExistsAsync(productId);
                if (!productExists)
                {
                    TempData["ErrorMessage"] = "Product not found";
                    return Redirect("/Product/");
                }

                var newComment = new Comment
                {
                    Name = name.Trim(),
                    Email = email.Trim().ToLowerInvariant(),
                    CommentText = comment.Trim(),
                    ProductId = productId,
                    CreateDate = DateTime.UtcNow
                };

                var success = await _productService.AddCommentAsync(newComment);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Your comment submitted successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "An error occurred while submitting your comment";
                }

                return Redirect("/Product/ProductDetails/" + productId);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your comment";
                return Redirect("/Product/ProductDetails/" + productId);
            }
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
