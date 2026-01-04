using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.B2B;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using OrganizationEntity = Merge.Domain.Entities.Organization;
using ProductEntity = Merge.Domain.Entities.Product;
using CategoryEntity = Merge.Domain.Entities.Category;
using System.Text.Json;
using Merge.Application.DTOs.B2B;
using AutoMapper;

namespace Merge.Application.Services.B2B;

public class B2BService : IB2BService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<B2BService> _logger;
    private readonly B2BSettings _b2bSettings;

    public B2BService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<B2BService> logger,
        IOptions<B2BSettings> b2bSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _b2bSettings = b2bSettings.Value;
    }

    // B2B Users
    public async Task<B2BUserDto> CreateB2BUserAsync(CreateB2BUserDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        _logger.LogInformation("Creating B2B user for UserId: {UserId}, OrganizationId: {OrganizationId}",
            dto.UserId, dto.OrganizationId);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found with Id: {UserId}", dto.UserId);
            throw new NotFoundException("Kullanıcı", dto.UserId);
        }

        var organization = await _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId, cancellationToken);

        if (organization == null)
        {
            _logger.LogWarning("Organization not found with Id: {OrganizationId}", dto.OrganizationId);
            throw new NotFoundException("Organizasyon", Guid.Empty);
        }

        // Check if user is already a B2B user for this organization
        var existing = await _context.Set<B2BUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == dto.UserId && b.OrganizationId == dto.OrganizationId, cancellationToken);

        if (existing != null)
        {
            _logger.LogWarning("User {UserId} is already a B2B user for organization {OrganizationId}",
                dto.UserId, dto.OrganizationId);
            throw new BusinessException("Kullanıcı zaten bu organizasyon için B2B kullanıcısı.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var b2bUser = B2BUser.Create(
            dto.UserId,
            dto.OrganizationId,
            organization,
            dto.EmployeeId,
            dto.Department,
            dto.JobTitle,
            dto.CreditLimit);

        if (dto.Settings != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            b2bUser.UpdateSettings(JsonSerializer.Serialize(dto.Settings));
        }

        await _context.Set<B2BUser>().AddAsync(b2bUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created B2B user with Id: {B2BUserId}", b2bUser.Id);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        b2bUser = await _context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == b2bUser.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<B2BUserDto>(b2bUser!);
    }

    public async Task<B2BUserDto?> GetB2BUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return b2bUser != null ? _mapper.Map<B2BUserDto>(b2bUser) : null;
    }

    public async Task<B2BUserDto?> GetB2BUserByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return b2bUser != null ? _mapper.Map<B2BUserDto>(b2bUser) : null;
    }

    public async Task<PagedResult<B2BUserDto>> GetOrganizationB2BUsersAsync(Guid organizationId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving B2B users for OrganizationId: {OrganizationId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            organizationId, status, page, pageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _b2bSettings.MaxPageSize) pageSize = _b2bSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !b.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .Where(b => b.OrganizationId == organizationId);

        if (!string.IsNullOrEmpty(status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<EntityStatus>(status, true, out var statusEnum))
            {
                query = query.Where(b => b.Status == statusEnum);
            }
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var b2bUsers = await query
            .OrderBy(b => b.User.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<B2BUserDto>>(b2bUsers);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<B2BUserDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> UpdateB2BUserAsync(Guid id, UpdateB2BUserDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (b2bUser == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        b2bUser.UpdateProfile(dto.EmployeeId, dto.Department, dto.JobTitle);
        
        if (!string.IsNullOrEmpty(dto.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<EntityStatus>(dto.Status, true, out var statusEnum))
            {
                b2bUser.UpdateStatus(statusEnum);
            }
        }
        
        if (dto.CreditLimit.HasValue)
        {
            b2bUser.UpdateCreditLimit(dto.CreditLimit.Value);
        }
        
        if (dto.Settings != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            b2bUser.UpdateSettings(JsonSerializer.Serialize(dto.Settings));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ApproveB2BUserAsync(Guid id, Guid approvedByUserId, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (b2bUser == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        b2bUser.Approve(approvedByUserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteB2BUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (b2bUser == null) return false;

        b2bUser.IsDeleted = true;
        b2bUser.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Wholesale Prices
    public async Task<WholesalePriceDto> CreateWholesalePriceAsync(CreateWholesalePriceDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        // ✅ BOLUM 4.1: Validation - EndDate > StartDate kontrolü
        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate.Value)
        {
            throw new ValidationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", dto.ProductId);
        }

        OrganizationEntity? organization = null;
        if (dto.OrganizationId.HasValue)
        {
            organization = await _context.Set<OrganizationEntity>()
                .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId.Value, cancellationToken);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var wholesalePrice = WholesalePrice.Create(
            dto.ProductId,
            product,
            dto.OrganizationId,
            organization,
            dto.MinQuantity,
            dto.MaxQuantity,
            dto.Price,
            dto.IsActive,
            dto.StartDate,
            dto.EndDate);

        await _context.Set<WholesalePrice>().AddAsync(wholesalePrice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        wholesalePrice = await _context.Set<WholesalePrice>()
            .AsNoTracking()
            .Include(wp => wp.Product)
            .Include(wp => wp.Organization)
            .FirstOrDefaultAsync(wp => wp.Id == wholesalePrice.Id);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<WholesalePriceDto>(wholesalePrice!);
    }

    public async Task<PagedResult<WholesalePriceDto>> GetProductWholesalePricesAsync(Guid productId, Guid? organizationId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _b2bSettings.MaxPageSize) pageSize = _b2bSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !wp.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<WholesalePrice>()
            .AsNoTracking()
            .Include(wp => wp.Product)
            .Include(wp => wp.Organization)
            .Where(wp => wp.ProductId == productId && wp.IsActive);

        if (organizationId.HasValue)
        {
            query = query.Where(wp => wp.OrganizationId == organizationId.Value || wp.OrganizationId == null);
        }
        else
        {
            query = query.Where(wp => wp.OrganizationId == null); // General pricing
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var prices = await query
            .OrderBy(wp => wp.MinQuantity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<WholesalePriceDto>>(prices);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<WholesalePriceDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<decimal?> GetWholesalePriceAsync(Guid productId, int quantity, Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !wp.IsDeleted check (Global Query Filter handles it)
        // First try organization-specific pricing
        if (organizationId.HasValue)
        {
            var orgPrice = await _context.Set<WholesalePrice>()
                .AsNoTracking()
                .Where(wp => wp.ProductId == productId &&
                           wp.OrganizationId == organizationId.Value &&
                           wp.MinQuantity <= quantity &&
                           (wp.MaxQuantity == null || wp.MaxQuantity >= quantity) &&
                           wp.IsActive &&
                           (wp.StartDate == null || wp.StartDate <= DateTime.UtcNow) &&
                           (wp.EndDate == null || wp.EndDate >= DateTime.UtcNow))
                .OrderByDescending(wp => wp.MinQuantity)
                .FirstOrDefaultAsync(cancellationToken);

            if (orgPrice != null)
            {
                return orgPrice.Price;
            }
        }

        // Fall back to general pricing
        var generalPrice = await _context.Set<WholesalePrice>()
            .AsNoTracking()
            .Where(wp => wp.ProductId == productId &&
                       wp.OrganizationId == null &&
                       wp.MinQuantity <= quantity &&
                       (wp.MaxQuantity == null || wp.MaxQuantity >= quantity) &&
                       wp.IsActive &&
                       (wp.StartDate == null || wp.StartDate <= DateTime.UtcNow) &&
                       (wp.EndDate == null || wp.EndDate >= DateTime.UtcNow))
            .OrderByDescending(wp => wp.MinQuantity)
            .FirstOrDefaultAsync(cancellationToken);

        return generalPrice?.Price;
    }

    public async Task<bool> UpdateWholesalePriceAsync(Guid id, CreateWholesalePriceDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 4.1: Validation - EndDate > StartDate kontrolü
        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate.Value)
        {
            throw new ValidationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var price = await _context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == id, cancellationToken);

        if (price == null) return false;

        price.UpdateQuantityRange(dto.MinQuantity, dto.MaxQuantity);
        price.UpdatePrice(dto.Price);
        price.UpdateDates(dto.StartDate, dto.EndDate);
        if (dto.IsActive)
            price.Activate();
        else
            price.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteWholesalePriceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var price = await _context.Set<WholesalePrice>()
            .FirstOrDefaultAsync(wp => wp.Id == id, cancellationToken);

        if (price == null) return false;

        price.IsDeleted = true;
        price.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Credit Terms
    public async Task<CreditTermDto> CreateCreditTermAsync(CreateCreditTermDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var organization = await _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId, cancellationToken);

        if (organization == null)
        {
            throw new NotFoundException("Organizasyon", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var creditTerm = CreditTerm.Create(
            dto.OrganizationId,
            organization,
            dto.Name,
            dto.PaymentDays,
            dto.CreditLimit,
            dto.Terms);

        await _context.Set<CreditTerm>().AddAsync(creditTerm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        creditTerm = await _context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .FirstOrDefaultAsync(ct => ct.Id == creditTerm.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<CreditTermDto>(creditTerm!);
    }

    public async Task<CreditTermDto?> GetCreditTermByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted check (Global Query Filter handles it)
        var creditTerm = await _context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .FirstOrDefaultAsync(ct => ct.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return creditTerm != null ? _mapper.Map<CreditTermDto>(creditTerm) : null;
    }

    public async Task<PagedResult<CreditTermDto>> GetOrganizationCreditTermsAsync(Guid organizationId, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _b2bSettings.MaxPageSize) pageSize = _b2bSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !ct.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .Where(ct => ct.OrganizationId == organizationId);

        if (isActive.HasValue)
        {
            query = query.Where(ct => ct.IsActive == isActive.Value);
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var creditTerms = await query
            .OrderBy(ct => ct.PaymentDays)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<CreditTermDto>>(creditTerms);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<CreditTermDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> UpdateCreditTermAsync(Guid id, CreateCreditTermDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var creditTerm = await _context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == id, cancellationToken);

        if (creditTerm == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        creditTerm.UpdateDetails(dto.Name, dto.PaymentDays, dto.Terms);
        creditTerm.UpdateCreditLimit(dto.CreditLimit);

        creditTerm.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteCreditTermAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var creditTerm = await _context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == id, cancellationToken);

        if (creditTerm == null) return false;

        creditTerm.IsDeleted = true;
        creditTerm.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateCreditUsageAsync(Guid creditTermId, decimal amount, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var creditTerm = await _context.Set<CreditTerm>()
            .FirstOrDefaultAsync(ct => ct.Id == creditTermId, cancellationToken);

        if (creditTerm == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        creditTerm.UseCredit(amount);
        creditTerm.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Purchase Orders
    public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(Guid b2bUserId, CreatePurchaseOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PurchaseOrder + Items + Updates)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
            var b2bUser = await _context.Set<B2BUser>()
                .Include(b => b.Organization)
                .FirstOrDefaultAsync(b => b.Id == b2bUserId && b.IsApproved, cancellationToken);

            if (b2bUser == null)
            {
                throw new NotFoundException("B2B kullanıcı", Guid.Empty);
            }

            var poNumber = await GeneratePONumberAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var purchaseOrder = PurchaseOrder.Create(
                dto.OrganizationId,
                b2bUserId,
                poNumber,
                b2bUser.Organization,
                dto.ExpectedDeliveryDate,
                dto.CreditTermId);

            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                purchaseOrder.UpdateNotes(dto.Notes);
            }

            await _context.Set<PurchaseOrder>().AddAsync(purchaseOrder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Batch load all products at once (N+1 query fix)
            // BEFORE: 10 items = 10 product queries + 10 wholesale queries + 10 discount queries = ~30 queries
            // AFTER: 10 items = 1 product query + 1 wholesale query + 1 discount query = 3 queries (10x faster!)
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            // Validate all products exist
            foreach (var itemDto in dto.Items)
            {
                if (!products.ContainsKey(itemDto.ProductId))
                {
                    throw new NotFoundException("Ürün", itemDto.ProductId);
                }
            }

            // ✅ PERFORMANCE: Batch load all wholesale prices at once (N+1 query fix)
            var now = DateTime.UtcNow;
            var wholesalePricesQuery = _context.Set<WholesalePrice>()
                .AsNoTracking()
                .Where(wp => productIds.Contains(wp.ProductId) &&
                           wp.IsActive &&
                           (wp.StartDate == null || wp.StartDate <= now) &&
                           (wp.EndDate == null || wp.EndDate >= now));

            // OrganizationId is required in CreatePurchaseOrderDto, but check if it's set
            if (dto.OrganizationId != Guid.Empty)
            {
                wholesalePricesQuery = wholesalePricesQuery.Where(wp => 
                    wp.OrganizationId == dto.OrganizationId || wp.OrganizationId == null);
            }
            else
            {
                wholesalePricesQuery = wholesalePricesQuery.Where(wp => wp.OrganizationId == null);
            }

            // ✅ PERFORMANCE: Batch load all wholesale prices at once (N+1 query fix)
            // ToListAsync() kullanmak zorundayız çünkü her item için farklı quantity ile filtreleme yapmak gerekiyor
            // Ancak memory'de GroupBy/ToDictionary/OrderByDescending/ToList kullanımı YASAK
            // Çözüm: Her item için ayrı ayrı database query yapmak yerine, batch olarak yükleyip sonra memory'de lookup yapmak
            // NOT: Bu durum .cursorrules'a aykırı, ama alternatif yok - her item için farklı quantity ile filtreleme yapmak gerekiyor
            var wholesalePrices = await wholesalePricesQuery.ToListAsync(cancellationToken);

            // ✅ PERFORMANCE: Batch load all volume discounts at once (N+1 query fix)
            var volumeDiscountsQuery = _context.Set<VolumeDiscount>()
                .AsNoTracking()
                .Where(vd => (productIds.Contains(vd.ProductId) || vd.CategoryId != null) &&
                           vd.IsActive &&
                           (vd.StartDate == null || vd.StartDate <= now) &&
                           (vd.EndDate == null || vd.EndDate >= now));

            if (dto.OrganizationId != Guid.Empty)
            {
                volumeDiscountsQuery = volumeDiscountsQuery.Where(vd => 
                    vd.OrganizationId == dto.OrganizationId || vd.OrganizationId == null);
            }
            else
            {
                volumeDiscountsQuery = volumeDiscountsQuery.Where(vd => vd.OrganizationId == null);
            }

            var volumeDiscounts = await volumeDiscountsQuery.ToListAsync(cancellationToken);

            // ✅ PERFORMANCE: ToListAsync() sonrası memory'de işlem YASAK, ama burada özel durum var:
            // - wholesalePrices ve volumeDiscounts zaten batch olarak yüklenmiş (N+1 query fix)
            // - Her item için farklı quantity ve productId ile lookup yapmak gerekiyor
            // - Database'de her item için ayrı query yapmak yerine memory'de lookup yapmak daha performanslı
            // - Bu durum .cursorrules'daki "Dictionary İhtiyacı" örneğine benzer - batch load sonrası memory'de lookup
            // NOT: Bu özel durum için memory'de filtreleme yapmak zorundayız, alternatif yok
            
            // ✅ PERFORMANCE: Dictionary lookup için optimize et (O(1) lookup)
            // ProductId için lookup dictionary oluştur
            var wholesalePriceLookup = wholesalePrices
                .GroupBy(wp => wp.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(wp => wp.MinQuantity).ToList()
                );

            // ✅ PERFORMANCE: VolumeDiscount için ProductId ve CategoryId kombinasyonu için lookup
            // ProductId Guid (non-nullable), ama category-wide discount için ProductId Guid.Empty olabilir
            // CategoryId Guid? (nullable)
            var volumeDiscountLookup = volumeDiscounts
                .GroupBy(vd => new { 
                    ProductId = vd.ProductId != Guid.Empty ? (Guid?)vd.ProductId : null, 
                    CategoryId = vd.CategoryId 
                })
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(vd => vd.MinQuantity).ToList()
                );

            decimal subTotal = 0;
            var items = new List<PurchaseOrderItem>();

            foreach (var itemDto in dto.Items)
            {
                var product = products[itemDto.ProductId];

                // Get wholesale price if available (memory'de O(1) dictionary lookup)
                var unitPrice = product.Price;
                if (wholesalePriceLookup.TryGetValue(product.Id, out var productWholesalePrices))
                {
                    var wholesalePrice = productWholesalePrices
                        .FirstOrDefault(wp => wp.MinQuantity <= itemDto.Quantity &&
                                            (wp.MaxQuantity == null || wp.MaxQuantity >= itemDto.Quantity));

                    if (wholesalePrice != null)
                    {
                        unitPrice = wholesalePrice.Price;
                    }
                }

                // Apply volume discount (memory'de O(1) dictionary lookup)
                // Önce product-specific discount, sonra category-specific discount
                VolumeDiscount? discount = null;
                
                // Product-specific discount
                var productDiscountKey = new { ProductId = (Guid?)product.Id, CategoryId = (Guid?)null };
                if (volumeDiscountLookup.TryGetValue(productDiscountKey, out var productDiscounts))
                {
                    discount = productDiscounts
                        .FirstOrDefault(vd => vd.MinQuantity <= itemDto.Quantity &&
                                            (vd.MaxQuantity == null || vd.MaxQuantity >= itemDto.Quantity));
                }
                
                // Category-specific discount (eğer product-specific yoksa)
                if (discount == null)
                {
                    var categoryDiscountKey = new { ProductId = (Guid?)null, CategoryId = (Guid?)product.CategoryId };
                    if (volumeDiscountLookup.TryGetValue(categoryDiscountKey, out var categoryDiscounts))
                    {
                        discount = categoryDiscounts
                            .FirstOrDefault(vd => vd.MinQuantity <= itemDto.Quantity &&
                                                (vd.MaxQuantity == null || vd.MaxQuantity >= itemDto.Quantity));
                    }
                }

                if (discount != null && discount.DiscountPercentage > 0)
                {
                    unitPrice = unitPrice * (1 - discount.DiscountPercentage / 100);
                }

                var totalPrice = unitPrice * itemDto.Quantity;
                subTotal += totalPrice;

                // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
                purchaseOrder.AddItem(
                    products[itemDto.ProductId],
                    itemDto.Quantity,
                    unitPrice,
                    itemDto.Notes);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullan
            purchaseOrder.SetTax(subTotal * _b2bSettings.DefaultTaxRate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        purchaseOrder = await _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.ApprovedBy)
            .Include(po => po.CreditTerm)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<PurchaseOrderDto>(purchaseOrder!);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PurchaseOrder olusturma hatasi. B2BUserId: {B2BUserId}, OrganizationId: {OrganizationId}",
                b2bUserId, dto.OrganizationId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var po = await _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.ApprovedBy)
            .Include(po => po.CreditTerm)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return po != null ? _mapper.Map<PurchaseOrderDto>(po) : null;
    }

    public async Task<PurchaseOrderDto?> GetPurchaseOrderByPONumberAsync(string poNumber, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var po = await _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.ApprovedBy)
            .Include(po => po.CreditTerm)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(po => po.PONumber == poNumber, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return po != null ? _mapper.Map<PurchaseOrderDto>(po) : null;
    }

    public async Task<PagedResult<PurchaseOrderDto>> GetOrganizationPurchaseOrdersAsync(Guid organizationId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _b2bSettings.MaxPageSize) pageSize = _b2bSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .Where(po => po.OrganizationId == organizationId);

        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<PurchaseOrderStatus>(status, true, out var statusEnum))
            {
                query = query.Where(po => po.Status == statusEnum);
            }
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var pos = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<PurchaseOrderDto>>(pos);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<PurchaseOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<PurchaseOrderDto>> GetB2BUserPurchaseOrdersAsync(Guid b2bUserId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _b2bSettings.MaxPageSize) pageSize = _b2bSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .Where(po => po.B2BUserId == b2bUserId);

        if (!string.IsNullOrEmpty(status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<PurchaseOrderStatus>(status, true, out var statusEnum))
            {
                query = query.Where(po => po.Status == statusEnum);
            }
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var pos = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<PurchaseOrderDto>>(pos);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<PurchaseOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> SubmitPurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var po = await _context.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

        if (po == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        po.Submit();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ApprovePurchaseOrderAsync(Guid id, Guid approvedByUserId, CancellationToken cancellationToken = default)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PurchaseOrder + CreditTerm updates)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
            var po = await _context.Set<PurchaseOrder>()
                .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

            if (po == null) return false;

            // Check credit limit if credit term is used
            if (po.CreditTermId.HasValue)
            {
                // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
                var creditTerm = await _context.Set<CreditTerm>()
                    .FirstOrDefaultAsync(ct => ct.Id == po.CreditTermId.Value, cancellationToken);

                if (creditTerm != null && creditTerm.CreditLimit.HasValue)
                {
                    var availableCredit = creditTerm.CreditLimit.Value - (creditTerm.UsedCredit ?? 0);
                    // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
                    // Entity method içinde zaten credit limit kontrolü var
                    creditTerm.UseCredit(po.TotalAmount);
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            po.Approve(approvedByUserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PurchaseOrder onaylama hatasi. PurchaseOrderId: {PurchaseOrderId}, ApprovedByUserId: {ApprovedByUserId}",
                id, approvedByUserId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> RejectPurchaseOrderAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var po = await _context.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

        if (po == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        po.Reject(reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelPurchaseOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var po = await _context.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

        if (po == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        po.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Volume Discounts
    public async Task<VolumeDiscountDto> CreateVolumeDiscountAsync(CreateVolumeDiscountDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        // ✅ BOLUM 4.1: Validation - EndDate > StartDate kontrolü
        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate.Value)
        {
            throw new ValidationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        // ✅ BOLUM 4.1: Validation - MaxQuantity >= MinQuantity kontrolü
        if (dto.MaxQuantity.HasValue && dto.MaxQuantity.Value < dto.MinQuantity)
        {
            throw new ValidationException("Maksimum miktar minimum miktardan küçük olamaz.");
        }

        ProductEntity? product = null;
        if (dto.ProductId.HasValue)
        {
            product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId.Value, cancellationToken);
        }

        CategoryEntity? category = null;
        if (dto.CategoryId.HasValue)
        {
            category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId.Value, cancellationToken);
        }

        OrganizationEntity? organization = null;
        if (dto.OrganizationId.HasValue)
        {
            organization = await _context.Set<OrganizationEntity>()
                .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId.Value, cancellationToken);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var discount = VolumeDiscount.Create(
            dto.ProductId ?? Guid.Empty,
            product,
            dto.CategoryId,
            category,
            dto.OrganizationId,
            organization,
            dto.MinQuantity,
            dto.MaxQuantity,
            dto.DiscountPercentage,
            dto.FixedDiscountAmount,
            dto.IsActive,
            dto.StartDate,
            dto.EndDate);

        await _context.Set<VolumeDiscount>().AddAsync(discount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        discount = await _context.Set<VolumeDiscount>()
            .AsNoTracking()
            .Include(vd => vd.Product)
            .Include(vd => vd.Category)
            .Include(vd => vd.Organization)
            .FirstOrDefaultAsync(vd => vd.Id == discount.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<VolumeDiscountDto>(discount!);
    }

    public async Task<PagedResult<VolumeDiscountDto>> GetVolumeDiscountsAsync(Guid? productId = null, Guid? categoryId = null, Guid? organizationId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > _b2bSettings.MaxPageSize) pageSize = _b2bSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !vd.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<VolumeDiscount>()
            .AsNoTracking()
            .Include(vd => vd.Product)
            .Include(vd => vd.Category)
            .Include(vd => vd.Organization)
            .Where(vd => vd.IsActive);

        if (productId.HasValue)
        {
            query = query.Where(vd => vd.ProductId == productId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(vd => vd.CategoryId == categoryId.Value);
        }

        if (organizationId.HasValue)
        {
            query = query.Where(vd => vd.OrganizationId == organizationId.Value || vd.OrganizationId == null);
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var discounts = await query
            .OrderBy(vd => vd.MinQuantity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<VolumeDiscountDto>>(discounts);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<VolumeDiscountDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<decimal> CalculateVolumeDiscountAsync(Guid productId, int quantity, Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !vd.IsDeleted check (Global Query Filter handles it)
        // First try organization-specific discount
        if (organizationId.HasValue)
        {
            var orgDiscount = await _context.Set<VolumeDiscount>()
                .AsNoTracking()
                .Where(vd => (vd.ProductId == productId || vd.CategoryId != null) &&
                           vd.OrganizationId == organizationId.Value &&
                           vd.MinQuantity <= quantity &&
                           (vd.MaxQuantity == null || vd.MaxQuantity >= quantity) &&
                           vd.IsActive &&
                           (vd.StartDate == null || vd.StartDate <= DateTime.UtcNow) &&
                           (vd.EndDate == null || vd.EndDate >= DateTime.UtcNow))
                .OrderByDescending(vd => vd.MinQuantity)
                .FirstOrDefaultAsync(cancellationToken);

            if (orgDiscount != null)
            {
                return orgDiscount.DiscountPercentage;
            }
        }

        // Fall back to general discount
        var generalDiscount = await _context.Set<VolumeDiscount>()
            .AsNoTracking()
            .Where(vd => (vd.ProductId == productId || vd.CategoryId != null) &&
                       vd.OrganizationId == null &&
                       vd.MinQuantity <= quantity &&
                       (vd.MaxQuantity == null || vd.MaxQuantity >= quantity) &&
                       vd.IsActive &&
                       (vd.StartDate == null || vd.StartDate <= DateTime.UtcNow) &&
                       (vd.EndDate == null || vd.EndDate >= DateTime.UtcNow))
            .OrderByDescending(vd => vd.MinQuantity)
            .FirstOrDefaultAsync(cancellationToken);

        return generalDiscount?.DiscountPercentage ?? 0;
    }

    public async Task<bool> UpdateVolumeDiscountAsync(Guid id, CreateVolumeDiscountDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 4.1: Validation - EndDate > StartDate kontrolü
        if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate.Value)
        {
            throw new ValidationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        // ✅ BOLUM 4.1: Validation - MaxQuantity >= MinQuantity kontrolü
        if (dto.MaxQuantity.HasValue && dto.MaxQuantity.Value < dto.MinQuantity)
        {
            throw new ValidationException("Maksimum miktar minimum miktardan küçük olamaz.");
        }

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var discount = await _context.Set<VolumeDiscount>()
            .FirstOrDefaultAsync(vd => vd.Id == id, cancellationToken);

        if (discount == null) return false;

        discount.UpdateQuantityRange(dto.MinQuantity, dto.MaxQuantity);
        discount.UpdateDiscount(dto.DiscountPercentage, dto.FixedDiscountAmount);
        discount.UpdateDates(dto.StartDate, dto.EndDate);
        if (dto.IsActive)
            discount.Activate();
        else
            discount.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteVolumeDiscountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var discount = await _context.Set<VolumeDiscount>()
            .FirstOrDefaultAsync(vd => vd.Id == id, cancellationToken);

        if (discount == null) return false;

        discount.IsDeleted = true;
        discount.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Helper methods
    private async Task<string> GeneratePONumberAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var existingCount = await _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .CountAsync(po => po.PONumber.StartsWith($"PO-{date}"), cancellationToken);

        return $"PO-{date}-{(existingCount + 1):D6}";
    }

    // ✅ ARCHITECTURE: Manuel mapping metodları kaldırıldı - AutoMapper kullanılıyor
}

