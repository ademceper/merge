using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.LiveCommerce;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Common;

namespace Merge.Application.Services.LiveCommerce;

public class LiveCommerceService : ILiveCommerceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LiveCommerceService> _logger;
    private readonly IMapper _mapper;

    public LiveCommerceService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<LiveCommerceService> logger,
        IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<LiveStreamDto> CreateStreamAsync(CreateLiveStreamDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Canli yayin olusturuluyor. SellerId: {SellerId}, Title: {Title}", dto.SellerId, dto.Title);

        try
        {
            var stream = new LiveStream
            {
                SellerId = dto.SellerId,
                Title = dto.Title,
                Description = dto.Description,
                Status = LiveStreamStatus.Scheduled,
                ScheduledStartTime = dto.ScheduledStartTime,
                StreamUrl = dto.StreamUrl,
                StreamKey = dto.StreamKey,
                ThumbnailUrl = dto.ThumbnailUrl,
                Category = dto.Category,
                Tags = dto.Tags
            };

            await _context.LiveStreams.AddAsync(stream, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ ARCHITECTURE: AutoMapper kullan, manuel mapping YASAK
            // ✅ PERFORMANCE: AsNoTracking + Include ile tek query'de getir
            var createdStream = await _context.LiveStreams
                .AsNoTracking()
                .Include(s => s.Seller)
                .Include(s => s.Products)
                    .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(s => s.Id == stream.Id, cancellationToken);

            _logger.LogInformation("Canli yayin olusturuldu. StreamId: {StreamId}, SellerId: {SellerId}", stream.Id, stream.SellerId);

            return _mapper.Map<LiveStreamDto>(createdStream!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Canli yayin olusturma hatasi. SellerId: {SellerId}, Title: {Title}", dto.SellerId, dto.Title);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveStreamDto?> GetStreamAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var stream = await _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return stream != null ? _mapper.Map<LiveStreamDto>(stream) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    public async Task<IEnumerable<LiveStreamDto>> GetStreamsAsync(Guid? sellerId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var query = _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .AsQueryable();

        if (sellerId.HasValue)
        {
            query = query.Where(s => s.SellerId == sellerId.Value);
        }

        var streams = await query
            .OrderByDescending(s => s.CreatedAt)
            .Take(100) // ✅ Güvenlik: Maksimum 100 stream
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        return _mapper.Map<IEnumerable<LiveStreamDto>>(streams);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveStreamDto> UpdateStreamAsync(Guid streamId, CreateLiveStreamDto dto, CancellationToken cancellationToken = default)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream == null)
        {
            throw new NotFoundException("Yayın", streamId);
        }

        stream.Title = dto.Title;
        stream.Description = dto.Description;
        stream.ScheduledStartTime = dto.ScheduledStartTime;
        stream.StreamUrl = dto.StreamUrl;
        stream.StreamKey = dto.StreamKey;
        stream.ThumbnailUrl = dto.ThumbnailUrl;
        stream.Category = dto.Category;
        stream.Tags = dto.Tags;
        stream.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetStreamAsync(streamId, cancellationToken) ?? throw new NotFoundException("Yayın", streamId);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteStreamAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream == null) return false;

        stream.IsDeleted = true;
        stream.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<LiveStreamDto>> GetActiveStreamsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var query = _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .Where(s => s.Status == LiveStreamStatus.Live && s.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var streams = await query
            .OrderByDescending(s => s.ActualStartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        var items = _mapper.Map<IEnumerable<LiveStreamDto>>(streams);

        return new PagedResult<LiveStreamDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<LiveStreamDto>> GetStreamsBySellerAsync(Guid sellerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (page < 1) page = 1;
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var query = _context.LiveStreams
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .Where(s => s.SellerId == sellerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var streams = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        var items = _mapper.Map<IEnumerable<LiveStreamDto>>(streams);

        return new PagedResult<LiveStreamDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> StartStreamAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream == null) return false;

        stream.Status = LiveStreamStatus.Live;
        stream.ActualStartTime = DateTime.UtcNow;
        stream.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> EndStreamAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream == null) return false;

        stream.Status = LiveStreamStatus.Ended;
        stream.EndTime = DateTime.UtcNow;
        stream.IsActive = false;
        stream.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveStreamDto?> AddProductToStreamAsync(Guid streamId, Guid productId, AddProductToStreamDto? dto = null, CancellationToken cancellationToken = default)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream == null)
        {
            throw new NotFoundException("Yayın", streamId);
        }

        var existing = await _context.LiveStreamProducts
            .FirstOrDefaultAsync(p => p.LiveStreamId == streamId && p.ProductId == productId, cancellationToken);

        if (existing != null)
        {
            throw new BusinessException("Ürün zaten yayına eklenmiş.");
        }

        var streamProduct = new LiveStreamProduct
        {
            LiveStreamId = streamId,
            ProductId = productId,
            DisplayOrder = dto?.DisplayOrder ?? 0,
            SpecialPrice = dto?.SpecialPrice,
            ShowcaseNotes = dto?.ShowcaseNotes
        };

        await _context.LiveStreamProducts.AddAsync(streamProduct, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetStreamAsync(streamId, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ShowcaseProductAsync(Guid streamId, Guid productId, CancellationToken cancellationToken = default)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var streamProduct = await _context.LiveStreamProducts
            .FirstOrDefaultAsync(p => p.LiveStreamId == streamId && p.ProductId == productId, cancellationToken);

        if (streamProduct == null) return false;

        // Unhighlight all products in this stream
        // ⚠️ Batch update için ToListAsync gerekli (tracking ile)
        var allProducts = await _context.LiveStreamProducts
            .Where(p => p.LiveStreamId == streamId)
            .ToListAsync(cancellationToken);

        foreach (var product in allProducts)
        {
            product.IsHighlighted = false;
        }

        // Highlight the selected product
        streamProduct.IsHighlighted = true;
        streamProduct.ShowcasedAt = DateTime.UtcNow;
        streamProduct.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> JoinStreamAsync(Guid streamId, Guid? userId, string? guestId = null, CancellationToken cancellationToken = default)
    {
        var viewer = new LiveStreamViewer
        {
            LiveStreamId = streamId,
            UserId = userId,
            GuestId = guestId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _context.LiveStreamViewers.AddAsync(viewer, cancellationToken);

        // Update viewer count
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream != null)
        {
            // ✅ PERFORMANCE: Database'de count (memory'de YASAK)
            stream.ViewerCount = await _context.LiveStreamViewers
                .CountAsync(v => v.LiveStreamId == streamId && v.IsActive, cancellationToken);
            stream.TotalViewerCount++;
            if (stream.ViewerCount > stream.PeakViewerCount)
            {
                stream.PeakViewerCount = stream.ViewerCount;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> LeaveStreamAsync(Guid streamId, Guid? userId, string? guestId = null, CancellationToken cancellationToken = default)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var viewer = await _context.LiveStreamViewers
            .FirstOrDefaultAsync(v => v.LiveStreamId == streamId &&
                (userId.HasValue ? v.UserId == userId : v.GuestId == guestId) &&
                v.IsActive, cancellationToken);

        if (viewer == null) return false;

        viewer.LeftAt = DateTime.UtcNow;
        viewer.IsActive = false;
        viewer.WatchDuration = (int)(DateTime.UtcNow - viewer.JoinedAt).TotalSeconds;
        viewer.UpdatedAt = DateTime.UtcNow;

        // Update viewer count
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream != null)
        {
            // ✅ PERFORMANCE: Database'de count (memory'de YASAK)
            stream.ViewerCount = await _context.LiveStreamViewers
                .CountAsync(v => v.LiveStreamId == streamId && v.IsActive, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveStreamOrderDto> CreateOrderFromStreamAsync(Guid streamId, Guid orderId, Guid? productId = null, CancellationToken cancellationToken = default)
    {
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Sipariş", orderId);
        }

        var streamOrder = new LiveStreamOrder
        {
            LiveStreamId = streamId,
            OrderId = orderId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow,
            OrderAmount = order.TotalAmount
        };

        await _context.LiveStreamOrders.AddAsync(streamOrder, cancellationToken);

        // Update stream stats
        // ⚠️ Update için tracking gerekli, AsNoTracking KULLANILMAMALI
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream != null)
        {
            stream.OrderCount++;
            stream.Revenue += order.TotalAmount;
        }

        // Update product stats if productId provided
        if (productId.HasValue)
        {
            var streamProduct = await _context.LiveStreamProducts
                .FirstOrDefaultAsync(p => p.LiveStreamId == streamId && p.ProductId == productId.Value, cancellationToken);

            if (streamProduct != null)
            {
                streamProduct.OrderCount++;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<LiveStreamOrderDto>(streamOrder);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<LiveStreamStatsDto> GetStreamStatsAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var stream = await _context.LiveStreams
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == streamId, cancellationToken);

        if (stream == null)
        {
            throw new NotFoundException("Yayın", streamId);
        }

        // ✅ PERFORMANCE: Database'de aggregation (memory'de YASAK)
        var totalViewers = await _context.LiveStreamViewers
            .CountAsync(v => v.LiveStreamId == streamId, cancellationToken);

        var totalOrders = await _context.LiveStreamOrders
            .CountAsync(o => o.LiveStreamId == streamId, cancellationToken);

        var totalRevenue = await _context.LiveStreamOrders
            .Where(o => o.LiveStreamId == streamId)
            .SumAsync(o => (decimal?)o.OrderAmount, cancellationToken) ?? 0;

        return new LiveStreamStatsDto
        {
            StreamId = streamId,
            ViewerCount = stream.ViewerCount,
            PeakViewerCount = stream.PeakViewerCount,
            TotalViewerCount = totalViewers,
            OrderCount = stream.OrderCount,
            Revenue = stream.Revenue,
            TotalRevenue = totalRevenue,
            Status = stream.Status.ToString(), // ✅ BOLUM 1.2: Enum -> string (DTO uyumluluğu)
            Duration = stream.ActualStartTime.HasValue && stream.EndTime.HasValue
                ? (int)(stream.EndTime.Value - stream.ActualStartTime.Value).TotalSeconds
                : stream.ActualStartTime.HasValue
                    ? (int)(DateTime.UtcNow - stream.ActualStartTime.Value).TotalSeconds
                    : 0
        };
    }
}

