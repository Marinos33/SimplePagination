# SimplePagination

[![NuGet](https://img.shields.io/nuget/v/Marinos33.SimplePagination.svg)](https://www.nuget.org/packages/Marinos33.SimplePagination)
[![License](https://img.shields.io/github/license/Marinos33/SimplePagination)](https://github.com/Marinos33/SimplePagination/blob/main/LICENSE)

A lightweight, efficient .NET pagination library that simplifies handling paginated data collections. 
SimplePagination provides a memory efficient extension methods for IQueryable and IEnumerable collections with robust handling of edge cases.

## Features

- Pagination support for both `IQueryable<T>` and `IEnumerable<T>` collections
- Asynchronous pagination support for Entity Framework Core queries
- Optimized performance for different collection types (arrays, lists, etc.)
- Robust parameter validation and normalization
- Clean, intuitive API with nullable parameters
- Target framework: .NET Standard 2.0 (compatible with .NET Core, .NET 5+, and .NET Framework)

## Installation

Install the package via NuGet Package Manager or via .NET CLI: 
```bash
Install-Package Marinos33.SimplePagination
```

## Usage

### Basic Usage
```csharp
using SimplePagination; 
using System.Collections.Generic;

// For IEnumerable 
collections IEnumerable<Product> products = GetProducts(); 
var pagedProducts = products.ToPaginatedList(pageNumber: 1, pageSize: 10);

// For IQueryable (Entity Framework) with async support 
var pagedCustomers = await dbContext.Customers
.Where(c => c.IsActive)
.OrderBy(c => c.LastName)
.ToPaginatedListAsync(pageNumber: 2, pageSize: 25);
```

### PaginatedList Class

```csharp
public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
```


## Special Cases

- If `pageNumber` is null, it defaults to 1
- If `pageSize` is null or 0, all items will be returned in a single page
- If the requested page number exceeds the total pages, the last page is returned
- Empty source collections are handled gracefully

## Dependencies

- Microsoft.EntityFrameworkCore (for IQueryable extensions)
- .NET Standard 2.0+

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.



