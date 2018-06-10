using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SupermarketWebApi.DTO;
using SupermarketWebApi.Helpers;
using SupermarketWebApi.Models;
using SupermarketWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SupermarketWebApi.Controllers
{
    [Route("api/stock")]
    public class StockController : Controller
    {
        private ISupermarketRepository _supermarketRepository;
        private IStockPropertyMappingService _stockMappingService;
        private ITypeHelperService _typeHelperService;
        private IUrlHelper _urlHelper;

        public StockController(ISupermarketRepository supermarketRepository,
            IStockPropertyMappingService stockMappingService, ITypeHelperService typeHelperService, IUrlHelper urlHelper)
        {
            _supermarketRepository = supermarketRepository;
            _stockMappingService = stockMappingService;
            _typeHelperService = typeHelperService;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetAllStocK")]
        public IActionResult GetAllStock(StockResourceParameters stockResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            // check mappings are valid
            if (!_stockMappingService.ValidMappingExistsFor<SupermarketStockDTO, SupermarketStock>
                (stockResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            // check all fields are valid
            if (!_typeHelperService.TypeHasProperties<SupermarketStockDTO>
                (stockResourceParameters.Fields))
            {
                return BadRequest();
            }

            // get all stock from repo
            var stockFromRepo = _supermarketRepository.GetAllStock(stockResourceParameters);

            var stock = Mapper.Map<IEnumerable<SupermarketStockDTO>>(stockFromRepo);

            if (mediaType == "application/vnd.idp.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = stockFromRepo.TotalCount,
                    pageSize = stockFromRepo.PageSize,
                    currentPage = stockFromRepo.CurrentPage,
                    totalPages = stockFromRepo.TotalPages,
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateLinksForSupermarketStocks(stockResourceParameters,
                                                    stockFromRepo.HasNext, stockFromRepo.HasPrevious);

                var shapedStocks = stock.ShapeData(stockResourceParameters.Fields);

                var shapedStocksWithLinks = shapedStocks.Select(stocks =>
                {
                    var stockAsDictionary = stocks as IDictionary<string, object>;
                    var stockLinks = CreateLinksForSupermarketStock(
                        (int)stockAsDictionary["Id"], stockResourceParameters.Fields);

                    stockAsDictionary.Add("links", stockLinks);

                    return stockAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = shapedStocksWithLinks,
                    links = links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = stockFromRepo.HasPrevious ?
                CreateProductResourceUri(stockResourceParameters,
                ResourceUriType.PreviousPage) : null;

                var nextPageLink = stockFromRepo.HasNext ?
                    CreateProductResourceUri(stockResourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    totalCount = stockFromRepo.TotalCount,
                    pageSize = stockFromRepo.PageSize,
                    currentPage = stockFromRepo.CurrentPage,
                    totalPages = stockFromRepo.TotalPages,
                    previousPageLink,
                    nextPageLink
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                return Ok(stock.ShapeData(stockResourceParameters.Fields));
            }
        }

        private string CreateProductResourceUri(
            StockResourceParameters stockResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAllStock",
                      new
                      {
                          fields = stockResourceParameters.Fields,
                          orderBy = stockResourceParameters.OrderBy,
                          pageNumber = stockResourceParameters.PageNumber - 1,
                          pageSize = stockResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAllStock",
                      new
                      {
                          fields = stockResourceParameters.Fields,
                          orderBy = stockResourceParameters.OrderBy,
                          pageNumber = stockResourceParameters.PageNumber + 1,
                          pageSize = stockResourceParameters.PageSize
                      });

                default:
                    return _urlHelper.Link("GetAllStock",
                    new
                    {
                        fields = stockResourceParameters.Fields,
                        orderBy = stockResourceParameters.OrderBy,
                        pageNumber = stockResourceParameters.PageNumber,
                        pageSize = stockResourceParameters.PageSize
                    });
            }
        }

        [HttpGet("{id}", Name = "GetStock")]
        public IActionResult GetStock(int id, [FromQuery] string fields)
        {
            // check requested fields are valid
            if (!_typeHelperService.TypeHasProperties<SupermarketStockDTO>(fields))
                return BadRequest();

            // get stock by id
            var stockFromRepo = _supermarketRepository.GetStockById(id);

            // check stock exists
            if (stockFromRepo == null)
                return NotFound();

            // map and return stock
            var stock = Mapper.Map<SupermarketStockDTO>(stockFromRepo);
            
            var links = CreateLinksForSupermarketStock(id, fields);

            var linkedResourceToReturn = stock.ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateStock")]
        public IActionResult CreateSupermarketStock([FromBody] SupermarketStockForCreationDTO stock)
        {
            // check a new stock was passed in
            if (stock == null)
                return BadRequest();

            // Validate data
            if (!ModelState.IsValid || !_supermarketRepository.SupermarketExists(stock.SupermarketId) || !_supermarketRepository.ProductExists(stock.ProductId))
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);

            // make sure productId and supermarketId don't match an existing entry
            if (_supermarketRepository.SupermarketStockExists(stock.ProductId, stock.SupermarketId))
                // return 409
                return StatusCode(409);

            // map stock
            var stockEntity = Mapper.Map<SupermarketStock>(stock);

            // add stock and save
            _supermarketRepository.AddSupermarketStock(stockEntity);

            if (!_supermarketRepository.Save())
                throw new Exception("Creating stock failed on Save.");

            var stockToReturn = Mapper.Map<SupermarketStockDTO>(stockEntity);

            var links = CreateLinksForSupermarketStock(stockToReturn.Id, null);

            var linkedResourceToReturn = stockToReturn.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetStock", new { id = linkedResourceToReturn["Id"] }, linkedResourceToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockSupermarketStockCreation(int id)
        {
            // return 404 if user tries to create stock with specific id
            if (_supermarketRepository.SupermarketStockExists(id))
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteStock")]
        public IActionResult DeleteSupermarketStock(int id)
        {
            // get stock for deletion
            var supermarketStockFromRepo = _supermarketRepository.GetStockById(id);

            // check stock exists
            if (supermarketStockFromRepo == null)
                return NotFound();

            // delete and save
            _supermarketRepository.DeleteSupermarketStock(supermarketStockFromRepo);

            if (!_supermarketRepository.Save())
                throw new Exception($"Deleting stock {id} failed on save.");

            // return 204 on success
            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateStock")]
        public IActionResult UpdateSupermarketStock(int id, [FromBody] SupermarketStockForUpdateDTO supermarketStock)
        {
            // check a stock was passed in
            if (supermarketStock == null)
                return BadRequest();

            // check the target for updating exists
            if (!_supermarketRepository.SupermarketStockExists(id))
                return NotFound();

            // Validate data
            if (!ModelState.IsValid || !_supermarketRepository.SupermarketExists(supermarketStock.SupermarketId) || !_supermarketRepository.ProductExists(supermarketStock.ProductId))
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);

            // make sure productId and supermarketId don't match an existing entry (unless its the row being updated)
            if (_supermarketRepository.SupermarketStockExists(supermarketStock.ProductId, supermarketStock.SupermarketId, id))
                // return 409
                return StatusCode(409);

            // get stock to be updated
            var supermarketStockFromRepo = _supermarketRepository.GetStockById(id);

            // check stock was obtained
            if (supermarketStockFromRepo == null)
                return NotFound();

            // map data
            Mapper.Map(supermarketStock, supermarketStockFromRepo);

            // update and save
            _supermarketRepository.UpdateSupermarketStock(id);

            if (!_supermarketRepository.Save())
                throw new Exception($"Updating supermarket stock {id} failed on saving.");

            // return 204 on success
            return NoContent();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateStock")]
        public IActionResult PartiallyUpdateStock(int id,
            [FromBody] JsonPatchDocument<SupermarketStockForUpdateDTO> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var stockFromRepo = _supermarketRepository.GetStockById(id);
            if (stockFromRepo == null)
                return NotFound();

            var stockToPatch = Mapper.Map<SupermarketStockForUpdateDTO>(stockFromRepo);

            patchDoc.ApplyTo(stockToPatch, ModelState);

            TryValidateModel(stockToPatch);

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            Mapper.Map(stockToPatch, stockFromRepo);

            _supermarketRepository.UpdateSupermarketStock(id);

            if (!_supermarketRepository.Save())
                throw new Exception($"Patching stock {id} failed on save.");

            return NoContent();
        }

        private IEnumerable<LinkDTO> CreateLinksForSupermarketStock(int id, string fields)
        {
            var links = new List<LinkDTO>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDTO(_urlHelper.Link("GetStock", new { id }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new LinkDTO(_urlHelper.Link("GetStock", new { id, fields }),
                    "self",
                    "GET"));
            }
            links.Add(
                    new LinkDTO(_urlHelper.Link("DeleteStock", new { id }),
                    "delete_stock",
                    "DELETE"));

            links.Add(
                new LinkDTO(_urlHelper.Link("UpdateStock", new { id }),
                "update_stock",
                "PUT"));

            links.Add(
                new LinkDTO(_urlHelper.Link("PartiallyUpdateStock", new { id }),
                "patch_stock",
                "PATCH"));

            return links;
        }

        private IEnumerable<LinkDTO> CreateLinksForSupermarketStocks(StockResourceParameters stockResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDTO>();

            // self 
            links.Add(
               new LinkDTO(CreateStockResourceUri(stockResourceParameters,
               ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDTO(CreateStockResourceUri(stockResourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDTO(CreateStockResourceUri(stockResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        private string CreateStockResourceUri(
            StockResourceParameters stockResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAllStock",
                      new
                      {
                          fields = stockResourceParameters.Fields,
                          orderBy = stockResourceParameters.OrderBy,
                          pageNumber = stockResourceParameters.PageNumber - 1,
                          pageSize = stockResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAllStock",
                      new
                      {
                          fields = stockResourceParameters.Fields,
                          orderBy = stockResourceParameters.OrderBy,
                          pageNumber = stockResourceParameters.PageNumber + 1,
                          pageSize = stockResourceParameters.PageSize
                      });

                default:
                    return _urlHelper.Link("GetAllStock",
                    new
                    {
                        fields = stockResourceParameters.Fields,
                        orderBy = stockResourceParameters.OrderBy,
                        pageNumber = stockResourceParameters.PageNumber,
                        pageSize = stockResourceParameters.PageSize
                    });
            }
        }

        [HttpOptions]
        public IActionResult GetSupermarketStockOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }
    }
}
