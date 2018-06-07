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

namespace SupermarketWebApi.Controllers
{
    [Route("api/stockcollections")]
    public class StockCollectionsController : Controller
    {
        private ISupermarketRepository _supermarketRepository;
        private ILogger<SupermarketController> _logger;

        public StockCollectionsController(ISupermarketRepository supermarketStockRepository, ILogger<SupermarketController> logger)
        {
            _supermarketRepository = supermarketStockRepository;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateSupermarketStockCollection([FromBody] IEnumerable<SupermarketStockForCreationDTO> supermarketStockCollection)
        {
            if (supermarketStockCollection == null)
                return BadRequest();

            // Validate data
            foreach (SupermarketStockForCreationDTO s in supermarketStockCollection)
            {
                if (!ModelState.IsValid || !_supermarketRepository.SupermarketExists(s.SupermarketId) || !_supermarketRepository.ProductExists(s.ProductId))
                    // return 422
                    return new UnprocessableEntityObjectResult(ModelState);
                
                // make sure productId and supermarketId don't match an existing entry
                if (_supermarketRepository.SupermarketStockExists(s.ProductId, s.SupermarketId))
                    // return 409
                    return StatusCode(409);
            }

            var supermarketStockEntities = Mapper.Map<IEnumerable<SupermarketStock>>(supermarketStockCollection);

            foreach (var supermarketStock in supermarketStockEntities)
            {
                _supermarketRepository.AddSupermarketStock(supermarketStock);
            }

            if (!_supermarketRepository.Save())
                throw new Exception("Creating a supermarket stock collection failed on save.");

            var supermarketStockCollectionToReturn = Mapper.Map<IEnumerable<SupermarketStockDTO>>(supermarketStockEntities);
            var idsAsString = string.Join(",", supermarketStockCollectionToReturn.Select(s => s.Id));

            return CreatedAtRoute("GetSupermarketStockCollection",
                new { ids = idsAsString },
                supermarketStockCollectionToReturn);
        }

        [HttpGet("({ids})", Name = "GetSupermarketStockCollection")]
        public IActionResult GetSupermarketStockCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<int> ids)
        {
            if (ids == null)
                return BadRequest();

            var supermarketStockEntities = _supermarketRepository.GetSupermarketStockByIds(ids);

            if (ids.Count() != supermarketStockEntities.Count())
                return NotFound();

            var supermarketStockToReturn = Mapper.Map<IEnumerable<SupermarketStockDTO>>(supermarketStockEntities);
            return Ok(supermarketStockToReturn);
        }
    }
}
