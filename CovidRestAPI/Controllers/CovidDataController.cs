using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CovidRestAPI.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace CovidRestAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CovidDataController : ControllerBase
    {
        private readonly CovidDBContext _context;
        static readonly HttpClient client = new HttpClient();

        public CovidDataController(CovidDBContext context)
        {
            _context = context;
        }

        // GET: CovidData
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CovidData>>> GetCovidData()
        {
            var result = await _context.CovidData.ToListAsync();

            if (result != null && result.Count == 0)
                return NotFound("No Covid data in database.");
            return result;
        }

        // GET: CovidData/import
        [HttpGet("import")]
        public async Task<ActionResult<string>> GetCovidDataFromPublicAPI()
        {
            // Returns all cases by case type for a country from the first recorded case.
            HttpResponseMessage response = await client.GetAsync("https://api.covid19api.com/dayone/country/croatia");
            if (response.IsSuccessStatusCode)
            {
                List<CovidData> covidList;
                var responseJson= await response.Content.ReadAsStringAsync();

                try
                {
                    covidList = JsonConvert.DeserializeObject<List<CovidData>>(responseJson);
                }
                catch (Exception)
                {
                    return BadRequest("Error while data serialization.");
                }

                foreach (var covidData in covidList)
                {
                    if (!CovidDataExists(covidData.CountryCode, covidData.Date))
                    {
                        _context.CovidData.Add(covidData);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return BadRequest("Error while saving in DB.");
                }

                return Ok(responseJson);
            }
            return "Can not get response from api.covid19api.com. Status: " + response.StatusCode.ToString();
        }

        // GET: CovidData/byDate?from=a&to=b&minConfirmed=c&maxConfirmed=d
        [HttpGet("byDate")]
        public async Task<ActionResult<List<CovidData>>> GetCovidData(DateTime from, DateTime to, int? minConfirmed, int? maxConfirmed)
        {
            if (from == new DateTime()) return BadRequest("Parameter 'from' is obligatory.");
            if (to == new DateTime()) return BadRequest("Parameter 'to' is obligatory.");
            if (minConfirmed == null) return BadRequest("Parameter 'minConfirmed' is obligatory.");
            else if (minConfirmed < 0) return BadRequest("Parameter 'minConfirmed' must be positive number.");
            if (maxConfirmed == null) return BadRequest("Parameter 'maxConfirmed' is obligatory.");
            else if (maxConfirmed < 0) return BadRequest("Parameter 'maxConfirmed' must be positive number.");

            var covidData = await _context.CovidData.Where(x => x.Date >= from && x.Date <= to && x.Confirmed >= minConfirmed && x.Confirmed <= maxConfirmed).ToListAsync();

            if (covidData.Count == 0)
            {
                return NotFound("No Covid data in database by given parameters.");
            }

            return covidData;
        }

        // GET: CovidData/single?countryCode=a&caseDate=b
        [HttpGet("single")]
        public async Task<ActionResult<CovidData>> GetSingleCovidData(string countryCode, DateTime caseDate)
        {
            if(string.IsNullOrEmpty(countryCode)) return BadRequest("Parameter 'countryCode' is obligatory.");
            if (caseDate == new DateTime()) return BadRequest("Parameter 'caseDate' is obligatory.");

            var covidData = await _context.CovidData.FirstOrDefaultAsync(x => x.CountryCode == countryCode && x.Date == caseDate);

            if (covidData == null)
            {
                return NotFound("No Covid data in database by given parameters.");
            }

            return covidData;
        }

        // POST: CovidData
        [HttpPost]
        public async Task<ActionResult<CovidData>> PostCovidData(CovidData covidData)
        {
            _context.CovidData.Add(covidData);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CovidDataExists(covidData.CountryCode, covidData.Date))
                {
                    return Conflict("This Covid data is already contained in database.");
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCovidData", new { id = covidData.CountryCode }, covidData);
        }

        // DELETE: CovidData/countryCode=a&caseDate=b
        [HttpDelete]
        public async Task<ActionResult<CovidData>> DeleteCovidData(string countryCode, DateTime caseDate)
        {
            if (string.IsNullOrEmpty(countryCode)) return BadRequest("Parameter 'countryCode' is obligatory.");
            if (caseDate == new DateTime()) return BadRequest("Parameter 'caseDate' is obligatory.");

            var covidData = await _context.CovidData.FirstOrDefaultAsync(x => x.CountryCode == countryCode && x.Date == caseDate);
            
            if (covidData == null)
            {
                return NotFound("No Covid data in database by given parameters.");
            }

            _context.CovidData.Remove(covidData);
            await _context.SaveChangesAsync();

            return covidData;
        }

        private bool CovidDataExists(string countryCode, DateTime caseDate)
        {
            return _context.CovidData.Any(e => e.CountryCode == countryCode && e.Date == caseDate);
        }
    }
}
