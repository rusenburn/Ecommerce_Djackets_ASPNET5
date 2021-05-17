using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.APIUI.Dtos;
using Ecommerce.APIUI.Models;
using Ecommerce.Shared.Database;
using Ecommerce.Shared.Entities;
using Ecommerce.Shared.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.APIUI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _repo;

        private readonly ILogger<ProductsController> _logger;
        private readonly IMapper _mapper;

        public ProductsController(
            IUnitOfWork repo,
            ILogger<ProductsController> logger,
            IMapper mapper
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        [HttpGet("latest-products")]
        public async Task<IActionResult> GetLatestProducts(int count = 5)
        {
            try
            {
                if(count <0) return BadRequest($"{nameof(count)} cannot be negative");
                if(count == 0) return Ok(new ProductDto[0]);
                IEnumerable<Product> latestProducts = await _repo.Products.GetLatestProducts(count);
                var latestProductsDtos = _mapper.Map<IEnumerable<Product>,IEnumerable<ProductDto>>(latestProducts);
                return Ok(latestProductsDtos);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{categorySlug}/{productSlug}")]
        public async Task<IActionResult> DetailBySlugs(string categorySlug, string productSlug)
        {
            try
            {
                if(string.IsNullOrEmpty(categorySlug)||string.IsNullOrEmpty(productSlug))
                {
                    return NotFound();
                }
                Product product = await _repo.Products.GetProductBySlugs(categorySlug, productSlug);
                return await ProceedToDetailResult(product);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                if(id <= 0) return NotFound();
                Product product = await _repo.Products.GetOneAsync(id);
                if (product is null)
                {
                    return NotFound();
                }
                int categoryId = product.CategoryId;
                Category category = await _repo.Categories.GetOneAsync(categoryId);
                product.Category = category;
                return await ProceedToDetailResult(product);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }


        private async Task<IActionResult> ProceedToDetailResult(Product product)
        {
            if (product is null) return NotFound();
            ProductDto dto = _mapper.Map<Product,ProductDto>(product);
            return await Task.FromResult(Ok(dto));
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search(SearchParameters query)
        {
            try
            {
                IEnumerable<Product> products = new List<Product>();
                if (!string.IsNullOrEmpty(query.Query))
                {
                    products = await _repo.Products.FindAsync(p => p.Name.Contains(query.Query) || p.Description.Contains(query.Query));
                }
                return Ok(products);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

    }
}