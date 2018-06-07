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
    [Route("api/staffmembercollections")]
    public class StaffMemberCollectionsController : Controller
    {
        private ISupermarketRepository _supermarketRepository;
        private ILogger<SupermarketController> _logger;

        public StaffMemberCollectionsController(ISupermarketRepository supermarketRepository, ILogger<SupermarketController> logger)
        {
            _supermarketRepository = supermarketRepository;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateStaffMemberCollection([FromBody] IEnumerable<StaffMemberForCreationDTO> staffMemberCollection)
        {
            if (staffMemberCollection == null)
                return BadRequest();

            // Validate data
            foreach (StaffMemberForCreationDTO s in staffMemberCollection)
            {
                if (!ModelState.IsValid || !_supermarketRepository.SupermarketExists(s.SupermarketId))
                {
                    // return 422
                    return new UnprocessableEntityObjectResult(ModelState);
                }
            }

            var staffMemberEntities = Mapper.Map<IEnumerable<StaffMember>>(staffMemberCollection);

            foreach (var staffMember in staffMemberEntities)
            {
                _supermarketRepository.AddStaffMember(staffMember);
            }

            if (!_supermarketRepository.Save())
                throw new Exception("Creating a staff member collection failed on save.");

            var staffMemberCollectionToReturn = Mapper.Map<IEnumerable<StaffMemberDTO>>(staffMemberEntities);
            var idsAsString = string.Join(",", staffMemberCollectionToReturn.Select(s => s.Id));

            return CreatedAtRoute("GetStaffMemberCollection",
                new { ids = idsAsString },
                staffMemberCollectionToReturn);
        }

        [HttpGet("({ids})", Name = "GetStaffMemberCollection")]
        public IActionResult GetStaffMemberCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<int> ids)
        {
            if (ids == null)
                return BadRequest();

            var staffMemberEntities = _supermarketRepository.GetStaffMembersByIds(ids);

            if (ids.Count() != staffMemberEntities.Count())
                return NotFound();

            var staffMembersToReturn = Mapper.Map<IEnumerable<ProductDTO>>(staffMemberEntities);
            return Ok(staffMembersToReturn);
        }
    }
}
