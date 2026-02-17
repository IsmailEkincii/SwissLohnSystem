using Microsoft.AspNetCore.Mvc;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Controllers
{
    [ApiController]
    [Route("ui-api/lookups")]
    public class UiLookupsController : ControllerBase
    {
        private readonly ApiClient _api;
        public UiLookupsController(ApiClient api) => _api = api;

        [HttpGet("qst-tariffs")]
        public async Task<IActionResult> GetQstTariffs([FromQuery] string canton)
        {
            var (ok, data, msg) =
                await _api.GetAsync<object>(
                    $"/api/Lookups/qst-tariffs?canton={canton}");

            if (!ok)
                return BadRequest(msg);

            return Ok(data);
        }
    }
}
