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
    [Route("api/staffmembers")]
    public class StaffMemberController : Controller
    {
        private ISupermarketRepository _supermarketRepository;
        private IStaffMemberPropertyMappingService _staffMemberMappingService;
        private ITypeHelperService _typeHelperService;
        private IUrlHelper _urlHelper;

        public StaffMemberController(ISupermarketRepository supermarketRepository,
            IStaffMemberPropertyMappingService staffMemberMappingService, ITypeHelperService typeHelperService, IUrlHelper urlHelper)
        {
            _supermarketRepository = supermarketRepository;
            _staffMemberMappingService = staffMemberMappingService;
            _typeHelperService = typeHelperService;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetStaffMembers")]
        public IActionResult GetStaffMembers(StaffMemberResourceParameters staffMemberResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            // check mappings are valid
            if (!_staffMemberMappingService.ValidMappingExistsFor<StaffMemberDTO, StaffMember>
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

            var staffMembersFromRepo = _supermarketRepository.GetAllStaffMembers(staffMemberResourceParameters);

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
                    CreateProductResourceUri(staffMemberResourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = staffMembersFromRepo.HasNext ?
                    CreateProductResourceUri(staffMemberResourceParameters,
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

        private string CreateProductResourceUri(
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

        [HttpGet("{id}", Name = "GetStaffMember")]
        public IActionResult GetStaffMember(int id, [FromQuery] string fields)
        {
            // check requested fields are valid
            if (!_typeHelperService.TypeHasProperties<StaffMemberDTO>(fields))
                return BadRequest();

            // get staff member
            var staffMemberFromRepo = _supermarketRepository.GetStaffMemberById(id);

            // check staff member exists
            if (staffMemberFromRepo == null)
                return NotFound();

            // map and return data
            var staffMember = Mapper.Map<StaffMemberDTO>(staffMemberFromRepo);
            return Ok(staffMember.ShapeData(fields));
        }

        [HttpPost]
        public IActionResult CreateStaffMember([FromBody] StaffMemberForCreationDTO staffMember)
        {
            if (staffMember == null)
            {
                return BadRequest();
            }

            // Validate data
            if (!ModelState.IsValid || !_supermarketRepository.SupermarketExists(staffMember.SupermarketId))
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var staffMemberEntity = Mapper.Map<StaffMember>(staffMember);

            _supermarketRepository.AddStaffMember(staffMemberEntity);

            if (!_supermarketRepository.Save())
            {
                throw new Exception("Creating a staff member failed on Save.");
            }

            var staffMemberToReturn = Mapper.Map<StaffMemberDTO>(staffMemberEntity);

            return CreatedAtRoute("GetStaffMember", new { id = staffMemberToReturn.Id }, staffMemberToReturn);
        }

        [HttpPost("{id}")]
        public IActionResult BlockStaffmemberCreation(int id)
        {
            if (_supermarketRepository.StaffMemberExists(id))
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteStaffMember")]
        public IActionResult DeleteStaffMember(int id)
        {
            var staffMemberFromRepo = _supermarketRepository.GetStaffMemberById(id);

            if (staffMemberFromRepo == null)
                return NotFound();

            _supermarketRepository.DeleteStaffMember(staffMemberFromRepo);

            if (!_supermarketRepository.Save())
                throw new Exception($"Deleting staff member {id} failed on save.");

            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateStaffMember")]
        public IActionResult UpdateStaffMember(int id, [FromBody] StaffMemberForUpdateDTO staffMember)
        {
            // check a staff member was included in the body
            if (staffMember == null)
                return BadRequest();

            // check the staff member to be replaced exists
            if (!_supermarketRepository.StaffMemberExists(id))
                return NotFound();

            // Validate data
            if (!ModelState.IsValid || !_supermarketRepository.SupermarketExists(staffMember.SupermarketId))
            {
                // return 422
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var staffMemberFromRepo = _supermarketRepository.GetStaffMemberById(id);
            if (staffMemberFromRepo == null)
                return NotFound();

            Mapper.Map(staffMember, staffMemberFromRepo);

            _supermarketRepository.UpdateStaffMember(id);

            if (!_supermarketRepository.Save())
                throw new Exception($"Updating staff member {id} failed on saving.");

            return NoContent();
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
                "UPDATE"));

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
