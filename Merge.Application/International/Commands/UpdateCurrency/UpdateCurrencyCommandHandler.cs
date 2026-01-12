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

namespace Merge.Application.International.Commands.UpdateCurrency;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, CurrencyDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCurrencyCommandHandler> _logger;

    public UpdateCurrencyCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateCurrencyCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CurrencyDto> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating currency. CurrencyId: {CurrencyId}", request.Id);

        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        var currency = await _context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (currency == null)
        {
            _logger.LogWarning("Currency not found. CurrencyId: {CurrencyId}", request.Id);
            throw new NotFoundException("Para birimi", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        currency.UpdateDetails(request.Name, request.Symbol, request.DecimalPlaces, request.Format);
        
        // Exchange rate güncellemesi ayrı bir command olmalı, burada sadece details güncelleniyor
        // Eğer exchange rate değiştiyse, UpdateExchangeRateCommand kullanılmalı
        
        if (request.IsActive && !currency.IsActive)
        {
            currency.Activate();
        }
        else if (!request.IsActive && currency.IsActive)
        {
            currency.Deactivate();
        }

        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Currency updated successfully. CurrencyId: {CurrencyId}, Code: {Code}", currency.Id, currency.Code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<CurrencyDto>(currency);
    }
}

