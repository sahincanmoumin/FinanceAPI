using BusinessLayer.Abstract;
using EntityLayer.DTOs.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiLayer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransController : BaseController
    {
        private readonly IStockTransService _stockTransService;

        public StockTransController(IStockTransService stockTransService)
        {
            _stockTransService = stockTransService;
        }

        [HttpGet("{stockId}")]
        public async Task<IActionResult> GetTransactionsByStockId(int stockId, [FromQuery] StockTransFilterDto filter)
        {
            var result = await _stockTransService.GetTransactionsByStockIdAsync(stockId, filter);
            return Ok(result);
        }
    }
}