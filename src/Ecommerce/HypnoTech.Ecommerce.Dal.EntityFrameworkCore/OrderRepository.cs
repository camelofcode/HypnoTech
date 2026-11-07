using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Ecommerce.Dal.Repositories;
using HypnoTech.Ecommerce.Models;
using HypnoTech.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HypnoTech.Ecommerce.Dal.EntityFrameworkCore
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly IHypnoTechDbContextFactory _factory;
        private readonly ICountryRepository _countries;

        public OrderRepository(
            IHypnoTechDbContextFactory factory,
            ICountryRepository countries)
        {
            _factory = factory;
            _countries = countries;
        }

        public async Task SaveOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            order.Address.Country = (await _countries.GetCountryByCodeAsync(
                order.Address.CountryCode,
                cancellationToken)).DisplayText;
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            context.Add(order);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Order> CompleteOrderAsync(
            PaymentCompletion completion,
            CancellationToken cancellationToken = default)
        {
            return await UpdateStatusAsync(completion.PaymentId, "COMPLETE", order =>
            {
                order.PayerId = completion.PayerId;
                order.SaleId = completion.SaleId;
                order.SaleUrl = completion.SaleUrl;
                order.EmailAddress = completion.EmailAddress;
            }, cancellationToken);
        }

        public async Task CancelOrderAsync(
            string paymentId,
            CancellationToken cancellationToken = default)
        {
            await UpdateStatusAsync(paymentId, "CANCELLED", null, cancellationToken);
        }

        public async Task<PagedListResponse<Order>> GetOrdersAsync(
            PagedRequest request,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            var query = context.Set<Order>().AsNoTracking()
                .Where(order => order.Status == "COMPLETE")
                .OrderByDescending(order => order.DateTime);
            var count = await query.CountAsync(cancellationToken);
            return new PagedListResponse<Order>
            {
                CurrentPage = request.Page,
                Items = await query.Skip(request.Skip).Take(request.PageSize).ToListAsync(cancellationToken),
                TotalPages = (int)Math.Ceiling(count / (decimal)request.PageSize)
            };
        }

        public async Task<Order> GetOrderAsync(
            string paymentId,
            CancellationToken cancellationToken = default)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            return await context.Set<Order>().AsNoTracking()
                .Include(order => order.Items)
                .SingleOrDefaultAsync(order => order.PaymentId == paymentId, cancellationToken);
        }

        public Task<Order> MarkAsShippedAsync(
            string paymentId,
            CancellationToken cancellationToken = default)
        {
            return UpdateStatusAsync(paymentId, "SHIPPED", null, cancellationToken);
        }

        private async Task<Order> UpdateStatusAsync(
            string paymentId,
            string status,
            Action<Order> update,
            CancellationToken cancellationToken)
        {
            await using var context = await _factory.CreateDbContextAsync(cancellationToken);
            var order = await context.Set<Order>().SingleAsync(
                item => item.PaymentId == paymentId,
                cancellationToken);
            update?.Invoke(order);
            order.Status = status;
            await context.SaveChangesAsync(cancellationToken);
            return order;
        }
    }
}
