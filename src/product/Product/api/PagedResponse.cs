namespace ProductApi.Api
{
    /// <summary>
    /// Generic paged response used by list endpoints.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public readonly record struct PagedResponse<T>(
        IEnumerable<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
