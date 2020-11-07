﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PartsUnlimited.Models;
using PartsUnlimited.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsUnlimited.Queries
{
    public class OrdersQuery : IOrdersQuery
    {
        private readonly IPartsUnlimitedContext _db;

        public OrdersQuery(IPartsUnlimitedContext context)
        {
            _db = context;
        }

        public async Task<OrdersModel> IndexHelperAsync(string username, DateTime? start, DateTime? end, int count, string invalidOrderSearch, bool isAdminSearch)
        {
            // The datetime submitted is only expected to have a resolution of a day, so we remove
            // the time of day from start and end.  We add a day for queryEnd to ensure the date
            // includes the whole day requested
            var queryStart = (start ?? DateTime.Now).Date;
            var queryEnd = (end ?? DateTime.Now).Date.AddDays(1).AddSeconds(-1);

            var results = await GetOrderQuery(username, queryStart, queryEnd, count).ToListAsync();

            return new OrdersModel(results, username, queryStart, queryEnd, invalidOrderSearch, isAdminSearch);
        }

        private IQueryable<Order> GetOrderQuery(string username, DateTime start, DateTime end, int count)
        {
            if (String.IsNullOrEmpty(username))
            {
                return _db
                    .Orders
                    .Where(o => o.OrderDate < end && o.OrderDate >= start)
                    .OrderBy(o => o.OrderDate)
                    .Take(count);
            }
            else
            {
                return _db
                    .Orders
                    .Where(o => o.OrderDate < end && o.OrderDate >= start && o.Username == username)
                    .OrderBy(o => o.OrderDate)
                    .Take(count);
            }
        }

        public async Task<Order> FindOrderAsync(int id)
        {
            var orders = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

            return orders;
        }
    }
}
