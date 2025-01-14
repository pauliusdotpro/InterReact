﻿using Stringification;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace Account;

public class Updates : TestCollectionBase
{
    public Updates(ITestOutputHelper output, TestFixture fixture) : base(output, fixture) { }

    [Fact]
    public async Task AccountUpdatesTest()
    {
        Task<IList<object>> task = Client
            .Response
            .Where(m => m is AccountValue or PortfolioValue or AccountUpdateTime or AccountUpdateEnd)
            .TakeWhile(o => o is not AccountUpdateEnd)
            .ToList()
            .ToTask();

        Client.Request.RequestAccountUpdates(true);

        IList<object> list = await task;

        Client.Request.RequestAccountUpdates(false);

        foreach (var o in list)
            Write(o.Stringify());
    }

    [Fact]
    public async Task AccountUpdatesObservableTest()
    {
        IList<object> list = await Client
            .Service
            .AccountUpdatesObservable
            .TakeWhile(o => o is not AccountUpdateEnd)
            .ToList();

        foreach (var o in list)
            Write(o.Stringify());
    }
}
