namespace ArtemisBanking.Application.DTOs.Api
{
    public class PaginatedResponseDTO<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}