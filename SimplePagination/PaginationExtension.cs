using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimplePagination
{
    /// <summary>
    /// Provides extension methods for pagination functionality on IQueryable and IEnumerable collections.
    /// </summary>
    public static class PaginationExtension
    {
        /// <summary>
        /// Converts an IQueryable to a paginated list asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source IQueryable to paginate.</param>
        /// <param name="pageNumber">The requested page number (one-based).</param>
        /// <param name="pageSize">The requested number of items per page.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task representing the asynchronous operation that returns a paginated list.</returns>
        public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> source,
            int? pageNumber,
            int? pageSize,
            CancellationToken cancellationToken = default)
        {
            var count = await source.CountAsync(cancellationToken);

            var (normalizedPageNumber, normalizedPageSize) = NormalizePaginationParameters(pageNumber, pageSize, count);

            var items = await source
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<T>(items, count, normalizedPageNumber, normalizedPageSize);
        }

        /// <summary>
        /// Converts an IEnumerable to a paginated list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source collection.</typeparam>
        /// <param name="source">The source IEnumerable to paginate.</param>
        /// <param name="pageNumber">The requested page number (one-based).</param>
        /// <param name="pageSize">The requested number of items per page.</param>
        /// <returns>A paginated list containing the requested page of items.</returns>
        public static PaginatedList<T> ToPaginatedList<T>(
            this IEnumerable<T> source,
            int? pageNumber,
            int? pageSize)
        {
            var count = source.Count();

            var (normalizedPageNumber, normalizedPageSize) = NormalizePaginationParameters(pageNumber, pageSize, count);

            var items = source
                .Skip((normalizedPageNumber - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();

            return new PaginatedList<T>(items, count, normalizedPageNumber, normalizedPageSize);
        }

        /// <summary>
        /// Normalizes pagination parameters to ensure they are valid
        /// </summary>
        /// <param name="pageNumber">The requested page number</param>
        /// <param name="pageSize">The requested page size</param>
        /// <param name="totalCount">The total number of items</param>
        /// <returns>A tuple containing the normalized page number and page size</returns>
        private static (int pageNumber, int pageSize) NormalizePaginationParameters(int? pageNumber, int? pageSize,
            int totalCount)
        {
            if (pageNumber < 0 || pageSize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    $"Page number and page size must be greater than or equal to 0. Page number: {pageNumber}, Page size: {pageSize}.");
            }

            // Default to all items if pagination is not specified
            if (pageNumber is null || pageSize is null || pageSize == 0)
            {
                return (1, totalCount);
            }

            var normalizedPageNumber = Math.Max(1, pageNumber.Value);
            var normalizedPageSize = Math.Max(1, pageSize.Value);

            // Ensure page number doesn't exceed total pages
            var totalPages = (int)Math.Ceiling((double)totalCount / normalizedPageSize);
            if (totalPages > 0 && normalizedPageNumber > totalPages)
            {
                normalizedPageNumber = totalPages;
            }

            //Ensure PageSize doesn't exceed total count
            if (normalizedPageSize > totalCount)
            {
                normalizedPageSize = totalCount;
            }

            return (normalizedPageNumber, normalizedPageSize);
        }
    }
}