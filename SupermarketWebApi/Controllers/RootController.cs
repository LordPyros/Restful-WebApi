using Microsoft.AspNetCore.Mvc;
using SupermarketWebApi.DTO;
using System.Collections.Generic;

namespace Library.API.Controllers
{

    [Route("api")]
    public class RootController : Controller
    {
        private IUrlHelper _urlHelper;

        public RootController(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType == "application/vnd.idp.hateoas+json")
            {
                var links = new List<LinkDTO>();

                links.Add(
                  new LinkDTO(_urlHelper.Link("GetRoot", new { }),
                  "self",
                  "GET"));

                links.Add(
                 new LinkDTO(_urlHelper.Link("GetSupermarkets", new { }),
                 "supermarkets",
                 "GET"));

                links.Add(
                  new LinkDTO(_urlHelper.Link("CreateSupermarket", new { }),
                  "create_supermarket",
                  "POST"));

                return Ok(links);
            }

            return NoContent();
        }
    }
}
