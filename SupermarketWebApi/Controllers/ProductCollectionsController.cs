using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SupermarketWebApi.DTO;
using SupermarketWebApi.Helpers;
using SupermarketWebApi.Models;
using SupermarketWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.Controllers
{
    [Route("api/productcollections")]
    public class ProductCollectionsController : Controller
    {
        private ISupermarketRepository _supermarketRepository;
        private ILogger<SupermarketController> _logger;

        public ProductCollectionsController(ISupermarketRepository supermarketRepository, ILogger<SupermarketController> logger)
        {
            _supermarketRepository = supermarketRepository;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateProductCollection([FromBody] IEnumerable<ProductForCreationDTO> productCollection)
        {
            if (productCollection == null)
                return BadRequest();

            // Validate data
            foreach (ProductForCreationDTO p in productCollection)
            {
                if (!ModelState.IsValid)
                {
                    // return 422
                    return new UnprocessableEntityObjectResult(ModelState);
                }
            }

            var productEntities = Mapper.Map<IEnumerable<Product>>(productCollection);

            foreach (var product in productEntities)
            {
                _supermarketRepository.AddProduct(product);
            }

            if (!_supermarketRepository.Save())
                throw new Exception("Creating a product collection failed on save.");

            var productCollectionToReturn = Mapper.Map<IEnumerable<ProductDTO>>(productEntities);
            var idsAsString = string.Join(",", productCollectionToReturn.Select(p => p.ProductId));

            return CreatedAtRoute("GetProductCollection",
                new { ids = idsAsString },
                productCollectionToReturn);
        }

        [HttpGet("({ids})", Name = "GetProductCollection")]
        public IActionResult GetProductCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<int> ids)
        {
            if (ids == null)
                return BadRequest();

            var productEntities = _supermarketRepository.GetProductsByIds(ids);

            if (ids.Count() != productEntities.Count())
                return NotFound();

            var productsToReturn = Mapper.Map<IEnumerable<ProductDTO>>(productEntities);
            return Ok(productsToReturn);
        }
    }
}
