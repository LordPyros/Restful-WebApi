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
    [Route("api/supermarketcollections")]
    public class SupermarketCollectionsController : Controller
    {
        private ISupermarketRepository _supermarketRepository;
        private ILogger<SupermarketController> _logger;

        public SupermarketCollectionsController(ISupermarketRepository supermarketRepository, ILogger<SupermarketController> logger)
        {
            _supermarketRepository = supermarketRepository;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateSupermarketCollection([FromBody] IEnumerable<SupermarketForCreationDTO> supermarketCollection)
        {
            // check collection was supplied by user
            if (supermarketCollection == null)
                return BadRequest();

            // Validate data
            foreach (SupermarketForCreationDTO s in supermarketCollection)
            {
                if (!ModelState.IsValid)
                {
                    // return 422
                    return new UnprocessableEntityObjectResult(ModelState);
                }
            }

            // map data
            var supermarketEntities = Mapper.Map<IEnumerable<Supermarket>>(supermarketCollection);

            // create/add supermarkets and save
            foreach (var supermarket in supermarketEntities)
            {
                _supermarketRepository.AddSupermarket(supermarket);
            }

            if (!_supermarketRepository.Save())
                throw new Exception("Creating a supermarket collection failed on save.");

            // map new supermarkets
            var supermarketCollectionToReturn = Mapper.Map<IEnumerable<SupermarketDTO>>(supermarketEntities);
            // get ids of new supermarkets to return
            var idsAsString = string.Join(",", supermarketCollectionToReturn.Select(s => s.SupermarketId));

            // return all created supermarkets
            return CreatedAtRoute("GetSupermarketCollection",
                new { ids = idsAsString },
                supermarketCollectionToReturn);
        }

        [HttpGet("({ids})", Name = "GetSupermarketCollection")]
        public IActionResult GetSupermarketCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<int> ids)
        {
            // check ids were passed in
            if (ids == null)
                return BadRequest();

            // get all supermarkets with matching ID's
            var supermarketEntities = _supermarketRepository.GetSupermarketsByIds(ids);

            // check there is a supermarket for each id passed in
            if (ids.Count() != supermarketEntities.Count())
                return NotFound();

            // map and return data
            var supermarketsToReturn = Mapper.Map<IEnumerable<SupermarketDTO>>(supermarketEntities);
            return Ok(supermarketsToReturn);
        }
    }
}
