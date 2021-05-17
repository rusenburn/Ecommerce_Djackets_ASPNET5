using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.AdminPanel.Models;
using Ecommerce.AdminPanel.ViewModels;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ecommerce.AdminPanel.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _repo;
        private readonly IImageService _imageService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IUnitOfWork unitOfWork,
            IImageService imageService,
            ILogger<ProductsController> logger)
        {
            _repo = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<IActionResult> Index(FilterParams filterParams = null)
        {
            try
            {
                if (filterParams is null) filterParams = new FilterParams();

                IEnumerable<Product> products = await _repo.Products.GetProductsWithCategoriesAsync(filterParams.Page, filterParams.PageSize, filterParams.Search);
                var viewModel = new ProductsIndexViewModel()
                {
                    FilterParams = filterParams,
                    Products = products
                };
                // ViewData["Search"] = search;
                // ViewData["CurrentPage"] = page;
                // ViewData["pageSize"] = pageSize;
                // ViewData["NextPage"] = page + 1;
                // ViewData["PreviousPage"] = page <= 1 ? 1 : page - 1;
                return View(viewModel);
            }
            catch (System.Exception)
            {
                return DefaultStatus500ServerError();
            }

        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                Product vm = await _repo.Products.GetOneAsync(id);
                if (vm is null) return NotFound();
                return View(vm);
            }
            catch (Exception)
            {
                return DefaultStatus500ServerError();
            }
        }


        [HttpGet]
        public async Task<IActionResult> Upsert(int id)
        {
            try
            {
                if (id < 0) return BadRequest($"{nameof(id)} cannot have a negative value");
                ProductForm vm = new();
                if (id == 0) // create
                {

                    vm.Product = new Product();
                }
                else // update
                {
                    Product product = await _repo.Products.GetOneAsync(id);
                    if (product is null) return NotFound();
                    vm.Product = product;
                }
                vm.Categories = await _repo.Categories.GetAllAsync();
                return View(vm);
            }
            catch (System.Exception)
            {
                return DefaultStatus500ServerError();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductForm productForm)
        {
            try
            {
                if (!ModelState.IsValid) // invalid
                {
                    productForm.Categories = await _repo.Categories.GetAllAsync();
                    return View(productForm);
                }
                Product product = productForm.Product;
                if (product.Id == 0) // create
                {
                    // TODO : put this inside repository logic
                    var productsInDb = await _repo.Products.FindAsync(p => p.Slug == productForm.Product.Slug);
                    if (productsInDb.Count() > 0) // Slug Exists , meaning it is an invalid input
                    {
                        ModelState.AddModelError(nameof(product.Slug), $"{nameof(productForm.Product.Slug)} with name {productForm.Product.Slug} already exists in the database");
                        productForm.Categories = await _repo.Categories.GetAllAsync();
                        return View(productForm);
                    }
                    // Valid
                    product = await _repo.Products.CreateOneAsync(product);
                }
                else // update
                {
                    // TODO fix products time added changes everytime product is being updated
                    product = await _repo.Products.UpdateOneAsync(product.Id, product);
                }
                await _repo.SaveChangesAsync();
                IFormFile imageFile = productForm.imageFile;
                if (_imageService.IsValidImage(imageFile)) // save Image
                {
                    using (var ms = new MemoryStream())
                    {
                        // copy image to stream to make Image File
                        await imageFile.CopyToAsync(ms);
                        using Image im = Image.FromStream(ms);

                        // Send image to image Service and get the new imageName relative to media path
                        string imageName = await _imageService.UpdateImageAsync(im, product.Image);
                        product.Image = imageName;
                        string imageThumbnail = await _imageService.UpdateImageAsync(im, product.Thumbnail, true);
                        product.Thumbnail = imageThumbnail;
                        product = await _repo.Products.UpdateOneAsync(product.Id, product);
                        await _repo.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Details), new { id = product.Id });

            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.InnerException?.Message);
                return DefaultStatus500ServerError();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Product product = await _repo.Products.GetOneAsync(id);
                if (product is null) return NotFound();
                return View(product);
            }
            catch (System.Exception)
            {
                return DefaultStatus500ServerError();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                Product product = await _repo.Products.DeleteOneAsync(id);
                await _repo.SaveChangesAsync();
                if (product is null)
                {
                    return DefaultStatus500ServerError();
                }
                if (!string.IsNullOrEmpty(product.Image))
                {
                    await _imageService.DeleteImageAsync(product.Image);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception)
            {
                return DefaultStatus500ServerError();
            }
        }


        private IActionResult DefaultStatus500ServerError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the server");
        }
    }
}