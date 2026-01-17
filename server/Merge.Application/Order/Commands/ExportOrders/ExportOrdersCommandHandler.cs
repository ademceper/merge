using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Commands.ExportOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ExportOrdersCommandHandler(IDbContext context, IMapper mapper, ILogger<ExportOrdersCommandHandler> logger) : IRequestHandler<ExportOrdersCommand, byte[]>
{

    public async Task<byte[]> Handle(ExportOrdersCommand request, CancellationToken cancellationToken)
    {
        var orders = await GetOrdersForExportAsync(request, cancellationToken);

        return request.Format switch
        {
            ExportFormat.Csv => ExportToCsv(orders),
            ExportFormat.Json => ExportToJson(orders, request.IncludeOrderItems, request.IncludeAddress),
            ExportFormat.Excel => ExportToCsv(orders), // Simplified: using CSV for Excel
            _ => throw new ArgumentOutOfRangeException(nameof(request.Format))
        };
    }

    private async Task<List<OrderDto>> GetOrdersForExportAsync(ExportOrdersCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = context.Set<OrderEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            var statusEnum = Enum.Parse<OrderStatus>(request.Status);
            query = query.Where(o => o.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(request.PaymentStatus))
        {
            var paymentStatusEnum = Enum.Parse<PaymentStatus>(request.PaymentStatus);
            query = query.Where(o => o.PaymentStatus == paymentStatusEnum);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == request.UserId.Value);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Orders exported. Count: {Count}, StartDate: {StartDate}, EndDate: {EndDate}",
            orders.Count, request.StartDate, request.EndDate);

        return mapper.Map<List<OrderDto>>(orders);
    }

    private byte[] ExportToCsv(List<OrderDto> orders)
    {
        var csv = new StringBuilder();
        csv.AppendLine("OrderNumber,UserId,SubTotal,ShippingCost,Tax,TotalAmount,Status,PaymentStatus,CreatedAt");

        foreach (var order in orders)
        {
            csv.AppendLine($"\"{order.OrderNumber}\"," +
                          $"\"{order.UserId}\"," +
                          $"{order.SubTotal}," +
                          $"{order.ShippingCost}," +
                          $"{order.Tax}," +
                          $"{order.TotalAmount}," +
                          $"\"{order.Status}\"," +
                          $"\"{order.PaymentStatus}\"," +
                          $"\"{order.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private byte[] ExportToJson(List<OrderDto> orders, bool includeOrderItems, bool includeAddress)
    {
        var exportData = orders.Select(o => new
        {
            o.OrderNumber,
            o.UserId,
            o.SubTotal,
            o.ShippingCost,
            o.Tax,
            o.TotalAmount,
            o.Status,
            o.PaymentStatus,
            o.CreatedAt,
            OrderItems = includeOrderItems ? o.OrderItems.Select(oi => new
            {
                oi.ProductName,
                oi.Quantity,
                oi.Price,
                oi.TotalPrice
            }) : null,
            Address = includeAddress ? new
            {
                o.Address.AddressLine1,
                o.Address.AddressLine2,
                o.Address.City,
                o.Address.Country,
                o.Address.PostalCode
            } : null
        }).ToList();

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Encoding.UTF8.GetBytes(json);
    }
}
