using AutoMapper;
using Microsoft.AspNetCore.Http;
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
    [Route("api/products")]
    public class ProductsController : Controller
    {
        
        private ISupermarketRepository _supermarketRepository;
        private IProductPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;
        private IUrlHelper _urlHelper;

        public ProductsController(ISupermarketRepository supermarketRepository, 
            IProductPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService, IUrlHelper urlHelper)
        {
            _supermarketRepository = supermarketRepository;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetProducts")]
        public IActionResult GetProducts(ProductResourceParameters productResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            // check mappings are valid
            if (!_propertyMappingService.ValidMappingExistsFor<ProductDTO, Product>
                (productResourceParameters.OrderBy))
            {
                return BadRequest();
            }
 
            // check all fields are valid
            if (!_typeHelperService.TypeHasProperties<ProductDTO>
                (productResourceParameters.Fields))
            {
                return BadRequest();
            }

            var productsFromRepo = _supermarketRepository.GetAllProducts(productResourceParameters);

            var products = Mapper.Map<IEnumerable<ProductDTO>>(productsFromRepo);

            if (mediaType == "application/vnd.idp.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = productsFromRepo.TotalCount,
                    pageSize = productsFromRepo.PageSize,
                    currentPage = productsFromRepo.CurrentPage,
                    totalPages = productsFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateLinksForProducts(productResourceParameters,
                    productsFromRepo.HasNext, productsFromRepo.HasPrevious);

                var shapedProducts = products.ShapeData(productResourceParameters.Fields);

                var shapedProductsWithLinks = shapedProducts.Select(product =>
                {
                    var productAsDictionary = product as IDictionary<string, object>;
                    var productLinks = CreateLinksForProduct(
                        (int)productAsDictionary["ProductId"], productResourceParameters.Fields);

                    productAsDictionary.Add("links", productLinks);

                    return productAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = shapedProductsWithLinks,
                    links = links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = productsFromRepo.HasPrevious ?
                    CreateProductResourceUri(productResourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = productsFromRepo.HasNext ?
                    CreateProductResourceUri(productResourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    previousPageLink,
                    nextPageLink,
                    totalCount = productsFromRepo.TotalCount,
                    pageSize = productsFromRepo.PageSize,
                    currentPage = productsFromRepo.CurrentPage,
                    totalPages = productsFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                return Ok(products.ShapeData(productResourceParameters.Fields));
            }
        }

        private string CreateProductResourceUri(
            ProductResourceParameters productResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetProducts",
                      new
                      {
                          fields = productResourceParameters.Fields,
                          orderBy = productResourceParameters.OrderBy,
                          searchQuery = productResourceParameters.SearchQuery,
                          pageNumber = productResourceParameters.PageNumber - 1,
                          pageSize = productResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetProducts",
                      new
                      {
                          fields = productResourceParameters.Fields,
                          orderBy = productResourceParameters.OrderBy,
                          searchQuery = productResourceParameters.SearchQuery,
                          pageNumber = productResourceParameters.PageNumber + 1,
                          pageSize = productResourceParameters.PageSize
                      });

                default:
                    return _urlHelper.Link("GetProducts",
                    new
                    {
                        fields = productResourceParameters.Fields,
                        orderBy = productResourceParameters.OrderBy,
                        searchQuery = productResourceParameters.SearchQuery,
                        pageNumber = productResourceParameters.PageNumber,
                        pageSize = productResourceParameters.PageSize
                    });
            }
        }

        [HttpGet("{productId}", Name = "GetProduct")]
        public IActionResult GetProduct(int productId, [FromQuery] string fields)
        {
            // check requested fields are valid
            if (!_typeHelperService.TypeHasProperties<ProductDTO>(fields))
                return BadRequest();

            // get product
            var productFromRepo = _supermarketRepository.GetProductById(productId);

            // check product exists
            if (productFromRepo == null)
                return NotFound();

            // map and return data
            var product = Mapper.Map<ProductDTO>(productFromRepo);
            return Ok(product.ShapeData(fields));
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] ProductForCreationDTO product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            // Validate data
            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var productEntity = Mapper.Map<Product>(product);

            _supermarketRepository.AddProduct(productEntity);

            if (!_supermarketRepository.Save())
            {
                throw new Exception("Creating a product failed on Save.");
            }

            var productToReturn = Mapper.Map<ProductDTO>(productEntity);

            return CreatedAtRoute("GetProduct", new { id = productToReturn.ProductId }, productToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockProductCreation(int id)
        {
            if (_supermarketRepository.ProductExists(id))
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            return NotFound();
        }

        [HttpDelete("{productId}", Name = "DeleteProduct")]
        public IActionResult DeleteProduct(int productId)
        {
            var productFromRepo = _supermarketRepository.GetProductById(productId);

            if (productFromRepo == null)
                return NotFound();

            _supermarketRepository.DeleteProduct(productFromRepo);

            if (!_supermarketRepository.Save())
                throw new Exception($"Deleting product {productId} failed on save.");

            return NoContent();
        }

        [HttpPut("{productId}", Name = "UpdateProduct")]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductForUpdateDTO product)
        {
            if (product == null)
                return BadRequest();

            if (!_supermarketRepository.ProductExists(productId))
                return NotFound();

            // Validate data
            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var productFromRepo = _supermarketRepository.GetProductById(productId);
            if (productFromRepo == null)
                return NotFound();

            Mapper.Map(product, productFromRepo);

            _supermarketRepository.UpdateProduct(productId);

            if (!_supermarketRepository.Save())
                throw new Exception($"Updating product {productId} failed on saving.");

            return NoContent();
        }

        private IEnumerable<LinkDTO> CreateLinksForProduct(int productId, string fields)
        {
            var links = new List<LinkDTO>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDTO(_urlHelper.Link("GetProduct", new { productId }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new LinkDTO(_urlHelper.Link("GetProduct", new { productId, fields }),
                    "self",
                    "GET"));
            }
            links.Add(
                    new LinkDTO(_urlHelper.Link("DeleteProduct", new { productId }),
                    "delete_product",
                    "DELETE"));

            links.Add(
                new LinkDTO(_urlHelper.Link("UpdateProduct", new { productId }),
                "update_product",
                "UPDATE"));

            return links;
        }

        private IEnumerable<LinkDTO> CreateLinksForProducts(ProductResourceParameters productResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDTO>();

            // self 
            links.Add(
               new LinkDTO(CreateProductResourceUri(productResourceParameters,
               ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDTO(CreateProductResourceUri(productResourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDTO(CreateProductResourceUri(productResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }


    }
}
