namespace IniaApi.Model
{
    public class DataModel
    {
        public required string Country { get; set; }
        public required int Year { get; set; }
        public required string Subject { get; set; }
        public required double Value { get; set; }
        public required string SubjectCode { get;  set; }
        public required string CountryCode { get; set; }
    }
}