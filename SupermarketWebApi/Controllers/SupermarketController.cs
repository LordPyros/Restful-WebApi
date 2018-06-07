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
    [Route("api/supermarkets")]
    public class SupermarketController : Controller
    {

        private ISupermarketRepository _supermarketRepository;
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;

        public SupermarketController(ISupermarketRepository supermarketRepository,
            IUrlHelper urlHelper, IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService)
        {
            _supermarketRepository = supermarketRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
        }

        [HttpGet(Name = "GetSupermarkets")]
        public IActionResult GetSupermarkets(SupermarketResourceParameters supermarketResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            // check mappings are valid
            if (!_propertyMappingService.ValidMappingExistsFor<SupermarketDTO, Supermarket>
                (supermarketResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            // check all fields are valid
            if (!_typeHelperService.TypeHasProperties<SupermarketDTO>
                (supermarketResourceParameters.Fields))
            {
                return BadRequest();
            }

            var supermarketsFromRepo = _supermarketRepository.GetAllSupermarkets(supermarketResourceParameters);

            var supermarkets = Mapper.Map<IEnumerable<SupermarketDTO>>(supermarketsFromRepo);

            

            if (mediaType == "application/vnd.idp.hateoas+json")
            {
                // Pagination
                var paginationMetadata = new
                {
                    totalCount = supermarketsFromRepo.TotalCount,
                    pageSize = supermarketsFromRepo.PageSize,
                    currentPage = supermarketsFromRepo.CurrentPage,
                    totalPages = supermarketsFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateLinksForSupermarkets(supermarketResourceParameters,
                    supermarketsFromRepo.HasNext, supermarketsFromRepo.HasPrevious);

                var shapedSupermarkets = supermarkets.ShapeData(supermarketResourceParameters.Fields);

                var shapedSupermarketsWithLinks = shapedSupermarkets.Select(supermarket =>
                {
                    var supermarketAsDictionary = supermarket as IDictionary<string, object>;
                    var supermarketLinks = CreateLinksForSupermarket(
                        (int)supermarketAsDictionary["SupermarketId"], supermarketResourceParameters.Fields);

                    supermarketAsDictionary.Add("links", supermarketLinks);

                    return supermarketAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = shapedSupermarketsWithLinks,
                    links = links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = supermarketsFromRepo.HasPrevious ?
                    CreateSupermarketResourceUri(supermarketResourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = supermarketsFromRepo.HasNext ?
                    CreateSupermarketResourceUri(supermarketResourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    previousPageLink,
                    nextPageLink,
                    totalCount = supermarketsFromRepo.TotalCount,
                    pageSize = supermarketsFromRepo.PageSize,
                    currentPage = supermarketsFromRepo.CurrentPage,
                    totalPages = supermarketsFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                return Ok(supermarkets.ShapeData(supermarketResourceParameters.Fields));
            }
        }

        [HttpGet("{supermarketId}", Name ="GetSupermarket")]
        public IActionResult GetSupermarket(int supermarketId, [FromQuery] string fields)
        {
            // check requested fields are valid
            if (!_typeHelperService.TypeHasProperties<SupermarketDTO>(fields))
                return BadRequest();

            // get supermarket
            var supermarketFromRepo = _supermarketRepository.GetSupermarketById(supermarketId);

            if (supermarketFromRepo == null)
                return NotFound();

            //map data
            var supermarket = Mapper.Map<SupermarketDTO>(supermarketFromRepo);

            // create links
            var links = CreateLinksForSupermarket(supermarketId, fields);

            var linkedResourceToReturn = supermarket.ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);
            
            // return data
            return Ok(linkedResourceToReturn);
        }

        [HttpGet("{supermarketId}/products", Name = "GetProductsFromSupermarket")]
        public IActionResult GetProductsFromSupermarket(int supermarketId)
        {
            // check requested supermarket exists
            if (!_supermarketRepository.SupermarketExists(supermarketId))
                return NotFound();

            // Get all products stocked by supermarket
            var productsFromRepo = _supermarketRepository.GetAllProductsFromSupermarket(supermarketId);

            // map and return data
            var products = Mapper.Map<IEnumerable<ProductDTO>>(productsFromRepo);
            return Ok(products);
        }

        [HttpGet("{supermarketId}/staffmembers", Name = "GetStaffMembersFromSupermarket")]
        public IActionResult GetStaffMembersFromSupermarket(int supermarketId)
        {
            // get all staff members working at requested supermarket
            var staffMembersFromRepo = _supermarketRepository.GetAllStaffMembersWithSupermarketId(supermarketId);

            // return not found if no staff or supermarket doesn't exist
            if (staffMembersFromRepo.Count() == 0)
                return NotFound();

            // map and return data
            var staffMembers = Mapper.Map<IEnumerable<StaffMemberDTO>>(staffMembersFromRepo);
            return Ok(staffMembers);
        }

        [HttpPost(Name = "CreateSupermarket")]
        public IActionResult CreateSupermarket([FromBody] SupermarketForCreationDTO supermarket)
        {
            // check supermarket exists
            if (supermarket == null)
            {
                return BadRequest();
            }

            // Validate data
            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // map data
            var supermarketEntity = Mapper.Map<Supermarket>(supermarket);

            // add supermarket and save
            _supermarketRepository.AddSupermarket(supermarketEntity);

            if (!_supermarketRepository.Save())
            {
                throw new Exception("Creating a supermarket failed on Save.");
            }

            // map and return new supermarket
            var supermarketToReturn = Mapper.Map<SupermarketDTO>(supermarketEntity);

            var links = CreateLinksForSupermarket(supermarketToReturn.SupermarketId, null);

            var linkedResourceToReturn = supermarketToReturn.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetSupermarket", new { SupermarketId = linkedResourceToReturn["SupermarketId"] }, linkedResourceToReturn);
        }

        [HttpPost("{supermarketId}")]
        public IActionResult BlockSupermarketCreation(int supermarketId)
        {
            // return not found if user tries to create a supermarket at an ID uri
            if (_supermarketRepository.SupermarketExists(supermarketId))
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            return NotFound();
        }

        [HttpDelete("{supermarketId}/products/{productId}")]
        public IActionResult DeleteProductFromSupermarket(int supermarketId, int productId)
        {
            // check supermarket exists
            if (!_supermarketRepository.SupermarketExists(supermarketId))
                return NotFound();

            // get targeted stock
            var productFromSupermarketFromRepo = _supermarketRepository.GetStockByProductAndSupermarket(supermarketId, productId);

            // check stock not null
            if (productFromSupermarketFromRepo == null)
                return NotFound();

            // delete and save
            _supermarketRepository.DeleteSupermarketStock(productFromSupermarketFromRepo);

            if (!_supermarketRepository.Save())
                throw new Exception($"Deleting product {productId} from supermarket {supermarketId} failed on save.");

            return NoContent();
        }

        [HttpDelete("{supermarketId}", Name = "DeleteSupermarket")]
        public IActionResult DeleteSupermarket(int supermarketId)
        {
            // get supermarket for deleting
            var supermarketFromRepo = _supermarketRepository.GetSupermarketById(supermarketId);

            // check supermarket not null
            if (supermarketFromRepo == null)
                return NotFound();

            // delete and save
            _supermarketRepository.DeleteSupermarket(supermarketFromRepo);

            if (!_supermarketRepository.Save())
                throw new Exception($"Deleting supermarket {supermarketId} and associated stock failed on save.");

            return NoContent();
        }

        [HttpPut("{supermarketId}")]
        public IActionResult UpdateSupermarket(int supermarketId, [FromBody] SupermarketForUpdateDTO supermarket)
        {
            // check supermarket was supplied
            if (supermarket == null)
                return BadRequest();

            // check supermarket for updating exists
            if (!_supermarketRepository.SupermarketExists(supermarketId))
                return NotFound();

            // Validate data
            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            // get supermarket for updating
            var supermarketFromRepo = _supermarketRepository.GetSupermarketById(supermarketId);
            if (supermarketFromRepo == null)
                return NotFound();

            // map data
            Mapper.Map(supermarket, supermarketFromRepo);

            // Update and save
            _supermarketRepository.UpdateSupermarket(supermarketId);

            if (!_supermarketRepository.Save())
                throw new Exception($"Updating supermarket {supermarketId} failed on saving.");

            return NoContent();
        }

        private IEnumerable<LinkDTO> CreateLinksForSupermarket(int supermarketId, string fields)
        {
            var links = new List<LinkDTO>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDTO(_urlHelper.Link("GetSupermarket", new { supermarketId }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new LinkDTO(_urlHelper.Link("GetSupermarket", new { supermarketId, fields }),
                    "self",
                    "GET"));
            }
            links.Add(
                    new LinkDTO(_urlHelper.Link("DeleteSupermarket", new { supermarketId }),
                    "delete_supermarket",
                    "DELETE"));

            links.Add(
                new LinkDTO(_urlHelper.Link("GetProductsFromSupermarket", new { supermarketId }),
                "products",
                "GET"));

            links.Add(
                new LinkDTO(_urlHelper.Link("GetStaffMembersFromSupermarket", new { supermarketId }),
                "staff",
                "GET"));

            return links;
        }

        private IEnumerable<LinkDTO> CreateLinksForSupermarkets(SupermarketResourceParameters supermarketResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDTO>();

            // self 
            links.Add(
               new LinkDTO(CreateSupermarketResourceUri(supermarketResourceParameters,
               ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDTO(CreateSupermarketResourceUri(supermarketResourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDTO(CreateSupermarketResourceUri(supermarketResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        private string CreateSupermarketResourceUri(
            SupermarketResourceParameters supermarketResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetSupermarkets",
                      new
                      {
                          fields = supermarketResourceParameters.Fields,
                          orderBy = supermarketResourceParameters.OrderBy,
                          searchQuery = supermarketResourceParameters.SearchQuery,
                          pageNumber = supermarketResourceParameters.PageNumber - 1,
                          pageSize = supermarketResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetSupermarkets",
                      new
                      {
                          fields = supermarketResourceParameters.Fields,
                          orderBy = supermarketResourceParameters.OrderBy,
                          searchQuery = supermarketResourceParameters.SearchQuery,
                          pageNumber = supermarketResourceParameters.PageNumber + 1,
                          pageSize = supermarketResourceParameters.PageSize
                      });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link("GetSupermarkets",
                    new
                    {
                        fields = supermarketResourceParameters.Fields,
                        orderBy = supermarketResourceParameters.OrderBy,
                        searchQuery = supermarketResourceParameters.SearchQuery,
                        pageNumber = supermarketResourceParameters.PageNumber,
                        pageSize = supermarketResourceParameters.PageSize
                    });
            }
        }
    }
}
