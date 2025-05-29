using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GragonBallController(IHttpClientFactory httpClientFactory, IConnectionMultiplexer redis) : ControllerBase
    {
        private readonly HttpClient httpClient = httpClientFactory.CreateClient("DragonBallAPI");
        private readonly IConnectionMultiplexer redis =redis;
        /// <summary>
        /// Se conecta al API de Dragon Ball y devuelve la data, ademas la inserta en REDIS.
        /// </summary>
        /// <returns></returns>

        [HttpGet("GetCharacters")]
        public async Task<IActionResult> GetCharacters()
        {
            try
            {
                var result = await httpClient.GetAsync("characters");
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    JsonSerializerOptions options = new()
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    CharacterResponse list = JsonSerializer.Deserialize<CharacterResponse>(json, options);
                    await InsertDataInRedis(list.Items);

                    return Ok(list);
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

        /// <summary>
        /// Busca la data en REDIS.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        [HttpGet("GetCharacterInRedis/{name}")]
        public async Task<IActionResult> GetCharacterInRedis([FromRoute] string name)
        {
            try
            {
                var result = await GetDataToREDIS(name);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }


        private async Task InsertDataInRedis(List<Character> characters)
        {
            try
            {
                var db = redis.GetDatabase();
                var server = redis.GetServer(redis.GetEndPoints().First());
                var keysToDelete = server.Keys(pattern: "Character:*").ToArray();
                await Task.WhenAll(keysToDelete.Select(k => db.KeyDeleteAsync(k)));

                // Inserción masiva eficiente
                var entries = characters.Select(v =>
                    new KeyValuePair<RedisKey, RedisValue>($"Character:{v.Name}", JsonSerializer.Serialize(v))
                ).ToArray();

                await db.StringSetAsync(entries);

            }
            catch (Exception e)
            {
                 StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        private async Task<Character> GetDataToREDIS(string name)
        {
            try
            {
                var db = redis.GetDatabase();
                string? json = await db.StringGetAsync($"Character:{name}");

                if (!string.IsNullOrEmpty(json))
                {
                    var character = JsonSerializer.Deserialize<Character>(json);
                    return character ?? new Character();
                }
                return new();

            }
            catch (Exception ex)
            {
                return new();
            }
        }
    }
}
