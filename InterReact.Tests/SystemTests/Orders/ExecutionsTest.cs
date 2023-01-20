﻿using Stringification;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace Orders;

public class Executions : TestCollectionBase
{
    public Executions(ITestOutputHelper output, TestFixture fixture) : base(output, fixture) { }

    [Fact]
    public async Task RequestExecutionsAsyncTest()
    {
        IList<IHasRequestId> list = await Client
            .Service
            .GetExecutionsAsync();
           
        Write($"Executions found: {list.Count}.");

        foreach (var item in list)
            Write(item.Stringify());
    }
}
