using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GragonBallController(IHttpClientFactory httpClientFactory) : ControllerBase
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("DragonBallAPI");


        [HttpGet("GetCharacters")]
        public async Task<IActionResult> GetCharacters()
        {
            try
            {
                var result = await httpClient.GetAsync("characters");
                if (result.IsSuccessStatusCode)
                {
                    return Ok(result.Content.ReadAsStringAsync());
                }
                return StatusCode((int)result.StatusCode, await result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("GetCharacter/{id:int}")]
        public async Task<IActionResult> GetCharacters([FromRoute]int id )
        {
            try
            {
                var result = await httpClient.GetAsync($"characters/{id}");
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    return Ok(content);
                }
                return StatusCode((int)result.StatusCode, await result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
