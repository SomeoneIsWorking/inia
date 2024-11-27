using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using IniaApi.Model;

namespace IniaApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private const string CacheKey = "ExcelData";
        private const string CountriesCacheKey = "Countries";
        private const string SubjectsCacheKey = "Subjects";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public DataController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public IActionResult GetFilteredData([FromQuery] string country, [FromQuery] string subject)
        {
            if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(subject))
            {
                return BadRequest("Country and subject parameters are required");
            }

            var data = GetCachedData();
            var filteredData = data
                .Where(d => d.CountryCode == country && d.Subject == subject)
                .Select(d => new { d.Year, d.Value })
                .SkipWhile(d => d.Value == 0)
                .ToList();
            return Ok(filteredData);
        }

        [HttpGet("countries")]
        public IActionResult GetCountries()
        {
            var countries = GetCachedCountries();
            return Ok(countries);
        }

        [HttpGet("subjects")]
        public IActionResult GetSubjects()
        {
            var subjects = GetCachedSubjects();
            return Ok(subjects);
        }

        private List<DataModel> GetCachedData()
        {
            return _memoryCache.GetOrCreate(CacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return GetDataFromExcel();
            })!;
        }

        private List<CountryModel> GetCachedCountries()
        {
            return _memoryCache.GetOrCreate(CountriesCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return GetCachedData()
                    .Select(d => new CountryModel
                    {
                        Name = d.Country,
                        Code = d.CountryCode
                    })
                    .DistinctBy(x => x.Code)
                    .OrderBy(s => s.Name)
                    .ToList();
            })!;
        }

        private List<SubjectModel> GetCachedSubjects()
        {
            return _memoryCache.GetOrCreate(SubjectsCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return GetCachedData()
                    .Select(d => new SubjectModel
                    {
                        Name = d.Subject,
                        Code = d.Subject
                    })
                    .DistinctBy(x => x.Code)
                    .OrderBy(s => s.Name)
                    .ToList();
            })!;
        }
        private List<DataModel> GetDataFromExcel()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t",
                HasHeaderRecord = true
            };
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var reader = new StreamReader("WEOOct2023all.xls", Encoding.GetEncoding("ISO-8859-9"));
            using var csv = new CsvReader(reader, config);
            csv.Read();
            csv.ReadHeader();
            var yearColumns = csv.HeaderRecord!.Skip(9)
                .Where(h => int.TryParse(h, out _))
                .Select(h => int.Parse(h))
                .ToList();


            return csv.ReadManually(row => yearColumns.Select(year => new DataModel
            {
                Country = row.GetField("Country")!.Trim(),
                Subject = row.GetField("Subject Descriptor")!.Trim(),
                Year = year,
                Value = double.TryParse(csv.GetField(year.ToString()), out var v) ? v : 0,
                CountryCode = csv.GetField("ISO")!,
                SubjectCode = csv.GetField("WEO Subject Code")!
            })).SelectMany(x => x).ToList();
        }
    }
}