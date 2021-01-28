using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Moq;

namespace ConsoleApp2
{
    class Program
    {
        private static int FilterCondition = 0;

        static async Task Main(string[] args)
        {
            Console.WriteLine("IAsyncEnumerable");
            await foreach (var d in ListAsync())
            {
                Console.WriteLine(d);
            }

            Console.WriteLine("IEnumerable");
            foreach (var d in List())
            {
                Console.WriteLine(d);
            }

            Console.WriteLine("IEnumerable from Async");
            foreach (var d in ListSyncFromAsync())
            {
                Console.WriteLine(d);
            }
        }

        static async IAsyncEnumerable<int> ListAsync()
        {
            var asyncPageableResult = GetPageDataAsync();

            await foreach (var i in asyncPageableResult)
            {
                if (i > FilterCondition)
                    yield return i;
            }
        }

        static IEnumerable<int> List()
        {
            var pageableResult = GetPageData();

            foreach (var i in pageableResult)
            {
                if (i > FilterCondition)
                    yield return i;
            }
        }

        static IEnumerable<int> ListSyncFromAsync()
        {
            IAsyncEnumerator<int> e = GetPageDataAsync().GetAsyncEnumerator();
            try
            {
                while (e.MoveNextAsync()
                    .AsTask()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult())
                    {
                        if (e.Current > FilterCondition)
                            yield return e.Current;
                    }
            }
            finally
            {
                if (e != null)
                    e.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        #region TestDataProvider

        static AsyncPageable<int> GetPageDataAsync()
        {
            var pg = new Mock<AsyncPageable<int>>();
            pg.Setup(p => p.GetAsyncEnumerator(CancellationToken.None))
                .Returns(
                    () =>
                    {
                        return GetDataAsync().GetAsyncEnumerator();
                    });

            return pg.Object;
        }

        static Pageable<int> GetPageData()
        {
            var pg = new Mock<Pageable<int>>();
            pg.Setup(p => p.GetEnumerator())
                .Returns(
                    () =>
                    {
                        return GetData().GetEnumerator();
                    });

            return pg.Object;
        }

        static IEnumerable<int> GetData()
        {
            return ListAsync()
                .ToListAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        static async IAsyncEnumerable<int> GetDataAsync()
        {
            for (int i = 1; i <= 10; i++)
            {
                await Task.Delay(1000);//Simulate waiting for data to come through. 
                yield return i;
            }
        }

        #endregion
    }
}
