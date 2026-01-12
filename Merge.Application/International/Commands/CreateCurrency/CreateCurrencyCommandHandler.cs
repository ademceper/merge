using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.International;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.International.Commands.CreateCurrency;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CreateCurrencyCommandHandler : IRequestHandler<CreateCurrencyCommand, CurrencyDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCurrencyCommandHandler> _logger;

    public CreateCurrencyCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateCurrencyCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyDto> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating currency. Code: {Code}, Name: {Name}", request.Code, request.Name);

        // Check if currency code already exists
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var exists = await _context.Set<Currency>()
            .AnyAsync(c => c.Code.ToUpper() == request.Code.ToUpper(), cancellationToken);

        if (exists)
        {
            _logger.LogWarning("Currency code already exists. Code: {Code}", request.Code);
            throw new BusinessException($"Bu para birimi kodu zaten mevcut: {request.Code}");
        }

        // If setting as base currency, update existing base currency
        if (request.IsBaseCurrency)
        {
            // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
            var currentBase = await _context.Set<Currency>()
                .FirstOrDefaultAsync(c => c.IsBaseCurrency, cancellationToken);

            if (currentBase != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                currentBase.RemoveBaseCurrencyStatus();
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var currency = Currency.Create(
            request.Code,
            request.Name,
            request.Symbol,
            request.ExchangeRate,
            request.IsBaseCurrency,
            request.IsActive,
            request.DecimalPlaces,
            request.Format);

        await _context.Set<Currency>().AddAsync(currency, cancellationToken);
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency created successfully. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CurrencyDto>(currency);
    }
}

