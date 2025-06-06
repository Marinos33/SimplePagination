using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Shouldly;
using System.Linq.Expressions;

namespace SimplePagination.UnitTests
{
    public class PaginationExtensionTests
    {
        #region ToPaginatedListAsync Tests

        [Fact]
        public async Task ToPaginatedListAsync_WithValidParameters_ShouldReturnCorrectPage()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };
            var mockDbSet = GetQueryableMockDbSet(testData);

            // Act
            var result = await mockDbSet.Object.ToPaginatedListAsync(2, 2);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.Items[0].ShouldBe("Item3");
            result.Items[1].ShouldBe("Item4");
            result.PageNumber.ShouldBe(2);
            result.PageSize.ShouldBe(2);
            result.TotalCount.ShouldBe(5);
            result.TotalPages.ShouldBe(3);
            result.HasPreviousPage.ShouldBeTrue();
            result.HasNextPage.ShouldBeTrue();
        }

        [Fact]
        public async Task ToPaginatedListAsync_WithNullParameters_ShouldReturnAllItems()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };
            var mockDbSet = GetQueryableMockDbSet(testData);

            // Act
            var result = await mockDbSet.Object.ToPaginatedListAsync(null, null);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(5);
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(5);
            result.TotalCount.ShouldBe(5);
            result.HasNextPage.ShouldBeFalse();
        }

        [Fact]
        public async Task ToPaginatedListAsync_WithPageNumberBeyondTotalPages_ShouldReturnLastPage()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };
            var mockDbSet = GetQueryableMockDbSet(testData);

            // Act
            var result = await mockDbSet.Object.ToPaginatedListAsync(10, 2);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(1);
            result.Items[0].ShouldBe("Item5");
            result.PageNumber.ShouldBe(3);
            result.PageSize.ShouldBe(2);
            result.TotalCount.ShouldBe(5);
            result.HasPreviousPage.ShouldBeTrue();
            result.HasNextPage.ShouldBeFalse();
        }

        [Fact]
        public async Task ToPaginatedListAsync_WithZeroPageSize_ShouldReturnAllItems()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };
            var mockDbSet = GetQueryableMockDbSet(testData);

            // Act
            var result = await mockDbSet.Object.ToPaginatedListAsync(1, 0);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(5);
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(5);
            result.TotalCount.ShouldBe(5);
        }

        [Fact]
        public async Task ToPaginatedListAsync_WithNegativeParameters_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };
            var mockDbSet = GetQueryableMockDbSet(testData);

            // Act & Assert
            await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
                await mockDbSet.Object.ToPaginatedListAsync(-1, 2));

            await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
                await mockDbSet.Object.ToPaginatedListAsync(1, -2));
        }

        [Fact]
        public async Task ToPaginatedListAsync_WithCancellationToken_ShouldRespectToken()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };
            var mockDbSet = GetQueryableMockDbSet(testData);

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            // Act
            var result = await mockDbSet.Object.ToPaginatedListAsync(1, 2, token);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
        }

        #endregion ToPaginatedListAsync Tests

        #region ToPaginatedList Tests

        [Fact]
        public void ToPaginatedList_WithValidParameters_ShouldReturnCorrectPage()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };

            // Act
            var result = testData.AsEnumerable().ToPaginatedList(2, 2);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(2);
            result.Items[0].ShouldBe("Item3");
            result.Items[1].ShouldBe("Item4");
            result.PageNumber.ShouldBe(2);
            result.PageSize.ShouldBe(2);
            result.TotalCount.ShouldBe(5);
            result.TotalPages.ShouldBe(3);
            result.HasPreviousPage.ShouldBeTrue();
            result.HasNextPage.ShouldBeTrue();
        }

        [Fact]
        public void ToPaginatedList_WithNullParameters_ShouldReturnAllItems()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };

            // Act
            var result = testData.AsEnumerable().ToPaginatedList(null, null);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(5);
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(5);
            result.TotalCount.ShouldBe(5);
            result.HasNextPage.ShouldBeFalse();
        }

        [Fact]
        public void ToPaginatedList_WithPageNumberBeyondTotalPages_ShouldReturnLastPage()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };

            // Act
            var result = testData.AsEnumerable().ToPaginatedList(10, 2);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(1);
            result.Items[0].ShouldBe("Item5");
            result.PageNumber.ShouldBe(3);
            result.PageSize.ShouldBe(2);
            result.TotalCount.ShouldBe(5);
            result.HasPreviousPage.ShouldBeTrue();
            result.HasNextPage.ShouldBeFalse();
        }

        [Fact]
        public void ToPaginatedList_WithZeroPageSize_ShouldReturnAllItems()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };

            // Act
            var result = testData.AsEnumerable().ToPaginatedList(1, 0);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(5);
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(5);
            result.TotalCount.ShouldBe(5);
        }

        [Fact]
        public void ToPaginatedList_WithNegativeParameters_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3", "Item4", "Item5" };

            // Act & Assert
            Should.Throw<ArgumentOutOfRangeException>(() =>
                testData.AsEnumerable().ToPaginatedList(-1, 2));

            Should.Throw<ArgumentOutOfRangeException>(() =>
                testData.AsEnumerable().ToPaginatedList(1, -2));
        }

        #endregion ToPaginatedList Tests

        #region NormalizePaginationParameters Tests

        [Fact]
        public void NormalizePaginationParameters_WithNullParameters_ShouldReturnDefaultValues()
        {
            // Arrange - NormalizePaginationParameters is private, we'll test through the public methods
            var testData = new List<string> { "Item1", "Item2", "Item3" };

            // Act
            var result = testData.AsEnumerable().ToPaginatedList(null, null);

            // Assert
            result.ShouldNotBeNull();
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(3);
            result.TotalCount.ShouldBe(3);
        }

        [Fact]
        public void NormalizePaginationParameters_WithPageSizeLargerThanTotalCount_ShouldLimitPageSize()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3" };

            // Act
            var result = testData.AsEnumerable().ToPaginatedList(1, 10);

            // Assert
            result.ShouldNotBeNull();
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(3); // Limited to total count
            result.TotalCount.ShouldBe(3);
        }

        #endregion NormalizePaginationParameters Tests

        #region Helper Methods

        public static Mock<DbSet<T>> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return mockSet;
        }

        public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }

            public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
        }

        public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            public TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
                => new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new TestAsyncEnumerable<TElement>(expression);

            public object Execute(Expression expression)
                => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression)
                => _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var expectedResultType = typeof(TResult).GetGenericArguments()[0];

                // Get the right Execute<T>(Expression) method
                var method = typeof(IQueryProvider)
                    .GetMethods()
                    .First(m => m.Name == "Execute"
                                && m.IsGenericMethodDefinition
                                && m.GetParameters().Length == 1
                                && m.GetParameters()[0].ParameterType == typeof(Expression));

                var genericMethod = method.MakeGenericMethod(expectedResultType);

                var executionResult = genericMethod.Invoke(_inner, [expression]);

                var taskResult = typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(null, [executionResult]);

                return (TResult)taskResult!;
            }
        }

        public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
            {
            }

            public TestAsyncEnumerable(Expression expression) : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        #endregion Helper Methods
    }
}