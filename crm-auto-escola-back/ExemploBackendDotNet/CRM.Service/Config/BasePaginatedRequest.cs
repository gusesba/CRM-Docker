using MediatR;

namespace Exemplo.Service.Config
{
    public abstract class BasePaginatedRequest<TResponse> : IRequest<TResponse>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? OrderBy { get; set; }
        public string? OrderDirection { get; set; } = "asc";
    }
}
