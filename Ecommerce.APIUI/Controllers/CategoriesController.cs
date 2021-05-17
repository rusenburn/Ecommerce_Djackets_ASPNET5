using System.Threading.Tasks;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Ecommerce.APIUI.Dtos;

namespace Ecommerce.APIUI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly ILogger<CategoriesController> _logger;
        private readonly IMapper _mapper;

        public CategoriesController(
            IUnitOfWork unitOfWork,
            ILogger<CategoriesController> logger,
            IMapper mapper)
        {
            _repo = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0) return BadRequest("Wrong input were provided.");
            try
            {
                Category category = await _repo.Categories.GetOneAsync(id);
                return await ProceedToDetailResult(category);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("slug/{categorySlug}")]
        public async Task<IActionResult> GetCategoryBySlug(string categorySlug)
        {
            if (string.IsNullOrEmpty(categorySlug)) return BadRequest("Wrong input were provided.");
            try
            {
                Category category = await _repo.Categories.GetCategoryBySlug(categorySlug);
                return await ProceedToDetailResult(category);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<IActionResult> ProceedToDetailResult(Category category)
        {
            if (category is null) return NotFound();
            category.Products = (ICollection<Product>)await _repo.Products.FindAsync(p => p.CategoryId == category.Id);
            var dto = _mapper.Map<Category,CategoryDTO>(category);
            return Ok(dto);
        }
    }
}