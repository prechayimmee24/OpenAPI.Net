﻿using ASP.NET.Demo.Models;
using ASP.NET.Demo.Services;
using Microsoft.AspNetCore.SignalR;
using OpenAPI.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ASP.NET.Demo.Hubs
{
    public class TradingAccountHub : Hub
    {
        private readonly ITradingAccountsService _tradingAccountsService;

        public TradingAccountHub(ITradingAccountsService tradingAccountsService)
        {
            _tradingAccountsService = tradingAccountsService;
        }

        public async Task LoadAccount(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            _ = await _tradingAccountsService.GetAccountModelByLogin(Convert.ToInt64(accountLogin));

            await Clients.Caller.SendAsync("AccountLoaded", accountLogin);
        }

        public async Task GetSymbols(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountModel = await _tradingAccountsService.GetAccountModelByLogin(Convert.ToInt64(accountLogin));

            await Clients.Caller.SendAsync("Symbols", new
            {
                accountLogin,
                Symbols = accountModel.Symbols.Where(symbol => symbol.Data.TradingMode == ProtoOATradingMode.Enabled).Select(symbol => new
                {
                    symbol.Name,
                    symbol.Bid,
                    symbol.Ask,
                    symbol.Id,
                    symbol.TickSize,
                    MinVolume = MonetaryConverter.FromMonetary(symbol.Data.MinVolume),
                    MaxVolume = MonetaryConverter.FromMonetary(symbol.Data.MaxVolume),
                    StepVolume = MonetaryConverter.FromMonetary(symbol.Data.StepVolume),
                })
            });
        }

        public async IAsyncEnumerable<SymbolQuote> GetSymbolQuotes(string accountLogin, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) yield return null;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            var channel = _tradingAccountsService.GetSymbolsQuoteChannel(accountId);

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var quote))
                {
                    yield return quote;
                }
            }
        }

        public void StopSymbolQuotes(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            _tradingAccountsService.StopSymbolQuotes(accountId);
        }

        public async Task GetPositions(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountModel = await _tradingAccountsService.GetAccountModelByLogin(Convert.ToInt64(accountLogin));

            await Clients.Caller.SendAsync("Positions", new
            {
                accountLogin,
                Positions = accountModel.Positions.Select(marketOrder => Position.FromModel(marketOrder))
            });
        }

        public async IAsyncEnumerable<Position> GetPositionUpdates(string accountLogin, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) yield return null;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            var channel = _tradingAccountsService.GetPositionUpdatesChannel(accountId);

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var position))
                {
                    yield return position;
                }
            }
        }

        public void StopPositionUpdates(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            _tradingAccountsService.StopPositionUpdates(accountId);
        }

        public async Task ClosePosition(string accountLogin, string positionId)
        {
            if (string.IsNullOrWhiteSpace(accountLogin) || string.IsNullOrWhiteSpace(positionId)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            await _tradingAccountsService.ClosePosition(accountId, Convert.ToInt64(positionId));
        }

        public async Task CloseAllPositions(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            await _tradingAccountsService.CloseAllPosition(accountId);
        }

        public async Task GetOrders(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountModel = await _tradingAccountsService.GetAccountModelByLogin(Convert.ToInt64(accountLogin));

            await Clients.Caller.SendAsync("Orders", new
            {
                accountLogin,
                Orders = accountModel.PendingOrders.Select(order => PendingOrder.FromModel(order))
            });
        }

        public async IAsyncEnumerable<PendingOrder> GetOrderUpdates(string accountLogin, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) yield return null;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            var channel = _tradingAccountsService.GetOrderUpdatesChannel(accountId);

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var order))
                {
                    yield return order;
                }
            }
        }

        public void StopOrderUpdates(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            _tradingAccountsService.StopOrderUpdates(accountId);
        }

        public async Task CancelOrder(string accountLogin, string orderId)
        {
            if (string.IsNullOrWhiteSpace(accountLogin) || string.IsNullOrWhiteSpace(orderId)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            await _tradingAccountsService.CancelOrder(accountId, Convert.ToInt64(orderId));
        }

        public async Task CancelAllOrders(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            await _tradingAccountsService.CancelAllOrders(accountId);
        }

        public async IAsyncEnumerable<AccountInfo> GetAccountInfoUpdates(string accountLogin, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) yield return null;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            var channel = _tradingAccountsService.GetAccountInfoUpdatesChannel(accountId);

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var accountInfo))
                {
                    yield return accountInfo;
                }
            }
        }

        public void StopAccountInfoUpdates(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            _tradingAccountsService.StopAccountInfoUpdates(accountId);
        }

        public async Task GetAccountInfo(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            await Clients.Caller.SendAsync("AccountInfo", new
            {
                accountLogin,
                Info = _tradingAccountsService.GetAccountInfo(accountId)
            });
        }

        public async IAsyncEnumerable<Error> GetErrors(string accountLogin, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) yield return null;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            var channel = _tradingAccountsService.GetErrorsChannel(accountId);

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (channel.Reader.TryRead(out var error))
                {
                    yield return error;
                }
            }
        }

        public void StopErrors(string accountLogin)
        {
            if (string.IsNullOrWhiteSpace(accountLogin)) return;

            var accountId = _tradingAccountsService.GetAccountId(Convert.ToInt64(accountLogin));

            _tradingAccountsService.StopErrors(accountId);
        }

        public Task CreateNewMarketOrder(NewMarketOrderRequest orderRequest) => _tradingAccountsService.CreateNewMarketOrder(orderRequest);
    }
}