﻿using Stringification;
using System.Reactive.Threading.Tasks;

namespace InterReact;

// add CancellationToken ct = default)

public partial class Service
{
    private readonly Dictionary<string, Task<IList<IHasRequestId>>> ContractDetailsCache = new();

    /// <summary>
    /// Returns a list of one or more contract details objects using the supplied 
    /// contract as selector. Results are cached. 
    /// For options or futures:
    /// If expiry is not specified, ContractDetails objects are retrieved for all expiries.
    /// If strike is not specified, ContractDetails objects are retrieved for all strikes.
    /// And so on. So beware that calling this method may result in attempting to retrieve a large number of contracts.
    /// This operation may take a long time and is subject to usage limiting, 
    /// so the Timeout() operator may be useful.
    /// </summary>
    public async Task<List<ContractDetails>> GetContractDetailsAsync(Contract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);

        if (contract.ComboLegs.Any())
            throw new ArgumentException("Contract must not include ComboLegs.");
        if (!string.IsNullOrEmpty(contract.ComboLegsDescription))
            throw new ArgumentException("Contract must not include ComboLegsDescription.");
        if (contract.DeltaNeutralContract is not null)
            throw new ArgumentException("Contract must not include DeltaNeutralValidation.");
        if (contract.SecurityIdType == SecurityIdType.Undefined ^ string.IsNullOrEmpty(contract.SecurityId))
            throw new ArgumentException("Invalid SecurityId/SecurityIdType combination.");

        string key = contract.Stringify();
        
        Task<IList<IHasRequestId>>? task;
        lock (ContractDetailsCache)
        {
            if (!ContractDetailsCache.TryGetValue(key, out task))
            {
                task = GetContractDetailsTask(contract); // start task
                ContractDetailsCache[key] = task;
            }
        }

        // await task outside lock
        IList<IHasRequestId> result = await task.ConfigureAwait(false);

        lock (ContractDetailsCache)
        {
            AlertMessage? alert = result.OfType<AlertMessage>().FirstOrDefault();
            if (alert == null)
                return result.Cast<ContractDetails>().ToList();
            ContractDetailsCache.Remove(key); // try

            throw alert.ToException();       
        }
    }

    private Task<IList<IHasRequestId>> GetContractDetailsTask(Contract contract)
    {
        int id = Request.GetNextId();

        Task<IList<IHasRequestId>> task = Response
            .WithRequestId(id)
            .TakeUntil(m => m is ContractDetailsEnd or AlertMessage)
            .Where(m => m is ContractDetails or AlertMessage)
            .ToList()
            .ToTask();

        Request.RequestContractDetails(id, contract);

        return task;
    }
}
