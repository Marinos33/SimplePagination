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
            if (pageNumber < 0 || pageSize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    $"Page number and page size must be greater than or equal to 0. Page number: {pageNumber}, Page size: {pageSize}.");
            }

            int effectivePageNumber = pageNumber ?? 1;
            int effectivePageSize = pageSize ?? int.MaxValue;

            if (effectivePageSize == 0)
            {
                effectivePageSize = int.MaxValue; // This will return all items
            }

            int count = await source.CountAsync(cancellationToken);

            (effectivePageNumber, effectivePageSize) = NormalizePaginationParameters(effectivePageNumber, effectivePageSize, count);

            if (count == 0)
            {
                return new PaginatedList<T>(EmptyList<T>.Instance, 0, effectivePageNumber, effectivePageSize);
            }

            List<T> items;

            if (effectivePageSize < count)
            {
                int skip = (effectivePageNumber - 1) * effectivePageSize;
                items = await source.Skip(skip).Take(effectivePageSize).ToListAsync(cancellationToken);
            }
            else
            {
                items = await source.ToListAsync(cancellationToken);
            }

            return new PaginatedList<T>(items, count, effectivePageNumber, effectivePageSize);
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
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (pageNumber < 0 || pageSize < 0)
            {
                throw new ArgumentOutOfRangeException(
                    $"Page number and page size must be greater than or equal to 0. Page number: {pageNumber}, Page size: {pageSize}.");
            }

            int effectivePageNumber = pageNumber ?? 1;
            int effectivePageSize = pageSize ?? int.MaxValue;

            if (effectivePageSize == 0)
            {
                effectivePageSize = int.MaxValue; // This will return all items
            }

            // Special case for arrays and List<T> for performance
            if (source is T[] sourceArray)
            {
                return PaginateArray(sourceArray, effectivePageNumber, effectivePageSize);
            }

            if (source is List<T> sourceList)
            {
                return PaginateList(sourceList, effectivePageNumber, effectivePageSize);
            }

            // Handle other collection types with known Count
            if (source is ICollection<T> collection)
            {
                int count = collection.Count;

                (effectivePageNumber, effectivePageSize) = NormalizePaginationParameters(effectivePageNumber, effectivePageSize, count);

                if (count == 0)
                {
                    return new PaginatedList<T>(EmptyList<T>.Instance, 0, effectivePageNumber, effectivePageSize);
                }

                int itemsToTake = Math.Min(effectivePageSize, count - ((effectivePageNumber - 1) * effectivePageSize));
                if (itemsToTake <= 0)
                {
                    return new PaginatedList<T>(EmptyList<T>.Instance, count, effectivePageNumber, effectivePageSize);
                }

                List<T> items = new List<T>(itemsToTake);

                int startIndex = (effectivePageNumber - 1) * effectivePageSize;
                int endIndex = startIndex + itemsToTake;
                int currentIndex = 0;

                foreach (T item in source)
                {
                    if (currentIndex >= endIndex)
                        break;

                    if (currentIndex >= startIndex)
                        items.Add(item);

                    currentIndex++;
                }

                return new PaginatedList<T>(items, count, effectivePageNumber, effectivePageSize);
            }
            else
            {
                // For unknown-size collections, we need to materialize
                List<T> sourceAsList = source.ToList();
                int count = sourceAsList.Count;

                (effectivePageNumber, effectivePageSize) = NormalizePaginationParameters(effectivePageNumber, effectivePageSize, count);

                if (count == 0)
                {
                    return new PaginatedList<T>(EmptyList<T>.Instance, 0, effectivePageNumber, effectivePageSize);
                }

                int skip = (effectivePageNumber - 1) * effectivePageSize;
                int take = Math.Min(effectivePageSize, count - skip);

                if (take <= 0)
                {
                    return new PaginatedList<T>(EmptyList<T>.Instance, count, effectivePageNumber, effectivePageSize);
                }

                List<T> pagedItems = new List<T>(take);
                for (int i = 0; i < take && skip + i < sourceAsList.Count; i++)
                {
                    pagedItems.Add(sourceAsList[skip + i]);
                }

                return new PaginatedList<T>(pagedItems, count, effectivePageNumber, effectivePageSize);
            }
        }

        /// <summary>
        /// Paginate an array efficiently.
        /// </summary>
        private static PaginatedList<T> PaginateArray<T>(T[] source, int pageNumber, int pageSize)
        {
            int count = source.Length;
            (pageNumber, pageSize) = NormalizePaginationParameters(pageNumber, pageSize, count);

            if (count == 0)
            {
                return new PaginatedList<T>(EmptyList<T>.Instance, 0, pageNumber, pageSize);
            }

            int skip = (pageNumber - 1) * pageSize;
            int take = Math.Min(pageSize, count - skip);

            if (take <= 0)
            {
                return new PaginatedList<T>(EmptyList<T>.Instance, count, pageNumber, pageSize);
            }

            List<T> pagedItems = new List<T>(take);
            for (int i = 0; i < take; i++)
            {
                pagedItems.Add(source[skip + i]);
            }

            return new PaginatedList<T>(pagedItems, count, pageNumber, pageSize);
        }

        /// <summary>
        /// Paginate a List efficiently.
        /// </summary>
        private static PaginatedList<T> PaginateList<T>(List<T> source, int pageNumber, int pageSize)
        {
            int count = source.Count;
            (pageNumber, pageSize) = NormalizePaginationParameters(pageNumber, pageSize, count);

            if (count == 0)
            {
                return new PaginatedList<T>(EmptyList<T>.Instance, 0, pageNumber, pageSize);
            }

            int skip = (pageNumber - 1) * pageSize;
            int take = Math.Min(pageSize, count - skip);

            if (take <= 0)
            {
                return new PaginatedList<T>(EmptyList<T>.Instance, count, pageNumber, pageSize);
            }

            List<T> pagedItems = new List<T>(take);
            int end = Math.Min(skip + take, source.Count);
            for (int i = skip; i < end; i++)
            {
                pagedItems.Add(source[i]);
            }

            return new PaginatedList<T>(pagedItems, count, pageNumber, pageSize);
        }

        /// <summary>
        /// Normalizes pagination parameters to ensure they are valid
        /// </summary>
        private static (int pageNumber, int pageSize) NormalizePaginationParameters(int pageNumber, int pageSize,
            int totalCount)
        {
            if (totalCount == 0)
            {
                return (1, Math.Max(1, pageSize));
            }

            // Ensure page number is at least 1
            pageNumber = Math.Max(1, pageNumber);

            // Calculate total pages only once
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Adjust page number if it exceeds total pages
            if (pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            // Ensure page size doesn't exceed total count
            pageSize = Math.Min(pageSize, totalCount);

            return (pageNumber, pageSize);
        }
    }

    /// <summary>
    /// Provides a singleton empty list to avoid allocations when returning empty collections.
    /// </summary>
    internal static class EmptyList<T>
    {
        public static readonly List<T> Instance = new List<T>(0);
    }
}