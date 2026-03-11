using BusinessLayer.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace ApiLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransController : BaseController
    {
        private readonly IStockTransService _stockTransService;

        public StockTransController(IStockTransService _stockTransService)
        {
            this._stockTransService = _stockTransService;
        }

        [HttpGet("{stockId}")]
        public async Task<IActionResult> GetTransactionsByStockId(int stockId, [FromQuery] StockTransFilterDto filter)
        {
            var result = await _stockTransService.GetTransactionsByStockIdAsync(stockId, filter);
            return Ok(result);
        }
    }
}