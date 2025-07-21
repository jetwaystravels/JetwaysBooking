using DomainLayer.Model;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Service.Interface;

namespace OnionArchitectureAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CP_GSTController : Controller
    {
        private readonly ICP_GstDetail<CP_GSTModel> _gstDetail;
        public CP_GSTController(ICP_GstDetail<CP_GSTModel> gstDetail)
        {
            this._gstDetail = gstDetail;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}


        [HttpGet("GetGstDetail")]
        public async Task<IActionResult> GetGstDetail()
        {
            var data = await _gstDetail.GetAllAsync();
            return Ok(data);

            //return View();

        }
    }
}
