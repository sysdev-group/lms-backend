namespace LMS.Application.Common;

public static class PaginationConstants
{
    /// <summary>
    /// Hard ceiling on PageSize accepted by any list endpoint.
    /// Callers requesting more than this receive exactly this many rows.
    /// </summary>
    public const int MaxPageSize = 100;
}
