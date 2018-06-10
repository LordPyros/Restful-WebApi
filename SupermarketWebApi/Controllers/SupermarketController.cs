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
    [Route("api/supermarkets")]
    public class SupermarketController : Controller
    {
        private ISupermarketRepository _supermarketRepository;
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;
        private IProductPropertyMappingService _productPropertyMappingService;
        private IStaffMemberPropertyMappingService _staffMemberPropertyMappingService;

        public SupermarketController(ISupermarketRepository supermarketRepository,
            IUrlHelper urlHelper, IPropertyMappingService propertyMappingService, ITypeHelperService typeHelperService,
            IProductPropertyMappingService productPropertyMappingService, IStaffMemberPropertyMappingService staffMemberPropertyMappingService)
        {
            _supermarketRepository = supermarketRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
            _productPropertyMappingService = productPropertyMappingService;
            _staffMemberPropertyMappingService = staffMemberPropertyMappingService;
        }

        [HttpGet(Name = "GetSupermarkets")]
        [HttpHead]
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
        public IActionResult GetProductsFromSupermarket(int supermarketId, ProductResourceParameters productResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_supermarketRepository.SupermarketExists(supermarketId))
                return NotFound();

            // check mappings are valid
            if (!_productPropertyMappingService.ValidMappingExistsFor<ProductDTO, Product>
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

            var productsFromRepo = _supermarketRepository.GetAllProductsFromSupermarket(supermarketId, productResourceParameters);

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

        [HttpGet("{supermarketId}/staffmembers", Name = "GetStaffMembersFromSupermarket")]
        public IActionResult GetStaffMembersFromSupermarket(int supermarketId, StaffMemberResourceParameters staffMemberResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            // check mappings are valid
            if (!_staffMemberPropertyMappingService.ValidMappingExistsFor<StaffMemberDTO, StaffMember>
                (staffMemberResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            // check all fields are valid
            if (!_typeHelperService.TypeHasProperties<StaffMemberDTO>
                (staffMemberResourceParameters.Fields))
            {
                return BadRequest();
            }

            var staffMembersFromRepo = _supermarketRepository.GetAllStaffMembersFromSupermarket(supermarketId, staffMemberResourceParameters);

            var staffMembers = Mapper.Map<IEnumerable<StaffMemberDTO>>(staffMembersFromRepo);

            if (mediaType == "application/vnd.idp.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = staffMembersFromRepo.TotalCount,
                    pageSize = staffMembersFromRepo.PageSize,
                    currentPage = staffMembersFromRepo.CurrentPage,
                    totalPages = staffMembersFromRepo.TotalPages,
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateLinksForStaffMembers(staffMemberResourceParameters,
                                    staffMembersFromRepo.HasNext, staffMembersFromRepo.HasPrevious);

                var shapedStaffMembers = staffMembers.ShapeData(staffMemberResourceParameters.Fields);

                var shapedStaffMembersWithLinks = shapedStaffMembers.Select(staffMember =>
                {
                    var staffMemberAsDictionary = staffMember as IDictionary<string, object>;
                    var staffMemberLinks = CreateLinksForStaffMember(
                        (int)staffMemberAsDictionary["Id"], staffMemberResourceParameters.Fields);

                    staffMemberAsDictionary.Add("links", staffMemberLinks);

                    return staffMemberAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = shapedStaffMembersWithLinks,
                    links = links
                };

                return Ok(linkedCollectionResource);

            }
            else
            {
                var previousPageLink = staffMembersFromRepo.HasPrevious ?
                    CreateStaffMemberResourceUri(staffMemberResourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = staffMembersFromRepo.HasNext ?
                    CreateStaffMemberResourceUri(staffMemberResourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    previousPageLink,
                    nextPageLink,
                    totalCount = staffMembersFromRepo.TotalCount,
                    pageSize = staffMembersFromRepo.PageSize,
                    currentPage = staffMembersFromRepo.CurrentPage,
                    totalPages = staffMembersFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                return Ok(staffMembers.ShapeData(staffMemberResourceParameters.Fields));
            }
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

        [HttpPut("{supermarketId}", Name = "UpdateSupermarket")]
        public IActionResult UpdateSupermarket(int supermarketId, [FromBody] SupermarketForUpdateDTO supermarket)
        {
            // check supermarket was supplied
            if (supermarket == null)
                return BadRequest();

            // check supermarket for updating exists - commented out for upsert example below
            //if (!_supermarketRepository.SupermarketExists(supermarketId))
            //    return NotFound();

            // Validate data
            if (!ModelState.IsValid)
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }
            
            // get supermarket for updating
            var supermarketFromRepo = _supermarketRepository.GetSupermarketById(supermarketId);

            // Upsert Example
            if (supermarketFromRepo == null)
            {
                var supermarketToAdd = Mapper.Map<Supermarket>(supermarket);

                _supermarketRepository.AddSupermarket(supermarketToAdd);

                if (!_supermarketRepository.Save())
                    throw new Exception($"Updating supermarket {supermarketId} failed on saving.");

                var supermarketToReturn = Mapper.Map<SupermarketDTO>(supermarketToAdd);

                return CreatedAtRoute("GetSupermarket",
                    new { supermarketId },
                    supermarketToReturn);
            }


            // map data
            Mapper.Map(supermarket, supermarketFromRepo);

            // Update and save
            _supermarketRepository.UpdateSupermarket(supermarketId);

            if (!_supermarketRepository.Save())
                throw new Exception($"Updating supermarket {supermarketId} failed on saving.");

            return NoContent();
        }

        [HttpPatch("{supermarketId}", Name = "PartiallyUpdateSupermarket")]
        public IActionResult PartiallyUpdateSupermarket(int supermarketId,
            [FromBody] JsonPatchDocument<SupermarketForUpdateDTO> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var supermarketFromRepo = _supermarketRepository.GetSupermarketById(supermarketId);
            if (supermarketFromRepo == null)
                return NotFound();

            var supermarketToPatch = Mapper.Map<SupermarketForUpdateDTO>(supermarketFromRepo);

            patchDoc.ApplyTo(supermarketToPatch, ModelState);

            TryValidateModel(supermarketToPatch);

            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);

            Mapper.Map(supermarketToPatch, supermarketFromRepo);

            _supermarketRepository.UpdateSupermarket(supermarketId);

            if (!_supermarketRepository.Save())
                throw new Exception($"Patching supermarket {supermarketId} failed on save.");

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetSupermarketsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST,HEAD");
            return Ok();
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

            links.Add(
                new LinkDTO(_urlHelper.Link("UpdateSupermarket", new { supermarketId }),
                "update_supermarket",
                "PUT"));

            links.Add(
                new LinkDTO(_urlHelper.Link("PartiallyUpdateSupermarket", new { supermarketId }),
                "patch_supermarket",
                "PATCH"));

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

        
        
        public IEnumerable<LinkDTO> CreateLinksForProduct(int productId, string fields)
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
                "PUT"));

            links.Add(
                new LinkDTO(_urlHelper.Link("PartiallyUpdateProduct", new { productId }),
                "patch_product",
                "PATCH"));

            return links;
        }

        public IEnumerable<LinkDTO> CreateLinksForProducts(ProductResourceParameters productResourceParameters,
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

        private IEnumerable<LinkDTO> CreateLinksForStaffMember(int id, string fields)
        {
            var links = new List<LinkDTO>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDTO(_urlHelper.Link("GetStaffMember", new { id }),
                    "self",
                    "GET"));
            }
            else
            {
                links.Add(
                    new LinkDTO(_urlHelper.Link("GetStaffMember", new { id, fields }),
                    "self",
                    "GET"));
            }
            links.Add(
                    new LinkDTO(_urlHelper.Link("DeleteStaffMember", new { id }),
                    "delete_staff_member",
                    "DELETE"));

            links.Add(
                new LinkDTO(_urlHelper.Link("UpdateStaffMember", new { id }),
                "update_staff_member",
                "PUT"));

            links.Add(
                new LinkDTO(_urlHelper.Link("PartiallyUpdateStaffMember", new { id }),
                "patch_staff_member",
                "PATCH"));

            return links;
        }

        private IEnumerable<LinkDTO> CreateLinksForStaffMembers(StaffMemberResourceParameters staffMemberResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDTO>();

            // self 
            links.Add(
               new LinkDTO(CreateStaffMemberResourceUri(staffMemberResourceParameters,
               ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDTO(CreateStaffMemberResourceUri(staffMemberResourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDTO(CreateStaffMemberResourceUri(staffMemberResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        private string CreateStaffMemberResourceUri(
            StaffMemberResourceParameters staffMemberResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetStaffMembers",
                      new
                      {
                          fields = staffMemberResourceParameters.Fields,
                          orderBy = staffMemberResourceParameters.OrderBy,
                          searchQuery = staffMemberResourceParameters.SearchQuery,
                          pageNumber = staffMemberResourceParameters.PageNumber - 1,
                          pageSize = staffMemberResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetStaffMembers",
                      new
                      {
                          fields = staffMemberResourceParameters.Fields,
                          orderBy = staffMemberResourceParameters.OrderBy,
                          searchQuery = staffMemberResourceParameters.SearchQuery,
                          pageNumber = staffMemberResourceParameters.PageNumber + 1,
                          pageSize = staffMemberResourceParameters.PageSize
                      });

                default:
                    return _urlHelper.Link("GetStaffMembers",
                    new
                    {
                        fields = staffMemberResourceParameters.Fields,
                        orderBy = staffMemberResourceParameters.OrderBy,
                        searchQuery = staffMemberResourceParameters.SearchQuery,
                        pageNumber = staffMemberResourceParameters.PageNumber,
                        pageSize = staffMemberResourceParameters.PageSize
                    });
            }
        }
    }
}
