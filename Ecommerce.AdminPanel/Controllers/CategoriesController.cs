using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ecommerce.AdminPanel.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _repo;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            IUnitOfWork unitOfWork,
            ILogger<CategoriesController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repo = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                IEnumerable<Category> categories = await _repo.Categories.GetAllAsync();
                return View(categories);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return Default500InternalServerError();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if(id<=0)return NotFound(); 
                Category category = await _repo.Categories.GetOneAsync(id);
                if (category is null) return NotFound();
                category.Products = (ICollection<Product>)(await _repo.Products.FindAsync(p => p.CategoryId == category.Id));
                return View(category);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return Default500InternalServerError();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int id)
        {
            try
            {
                if (id < 0) return NotFound();
                Category category = new Category();
                if (id == 0) // create
                {
                    category = new();
                }
                else //update
                {
                    category = await _repo.Categories.GetOneAsync(id);
                    if (category is null) return NotFound();
                }
                return View(category);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return Default500InternalServerError();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Category category)
        {
            try
            {
                if (ModelState.IsValid is not true) // invalid
                {
                    return View(category);
                }

                if (category.Id == 0) // create
                {
                    var categoryInDb = await _repo.Categories.GetCategoryBySlug(category.Slug);
                    if (categoryInDb is not null) // Slug Exists , meaning it is an invalid input
                    {
                        ModelState.AddModelError(nameof(category.Slug), $"{nameof(category.Slug)} with name {category.Slug} already exists in the database");
                        return View(category);
                    }
                    category = await _repo.Categories.CreateOneAsync(category);
                }
                else // update
                {
                    category = await _repo.Categories.UpdateOneAsync(category.Id, category);
                }
                await _repo.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = category.Id });
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return Default500InternalServerError();
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0) return NotFound();
                Category category = await _repo.Categories.GetOneAsync(id);
                if (category is null) return NotFound();
                return View(category);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return Default500InternalServerError();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                Category category = await _repo.Categories.DeleteOneAsync(id);
                await _repo.SaveChangesAsync();
                if (category is null) return BadRequest();
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
                return Default500InternalServerError();
            }
        }

        private IActionResult Default500InternalServerError()
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the server");
        }

    }
}