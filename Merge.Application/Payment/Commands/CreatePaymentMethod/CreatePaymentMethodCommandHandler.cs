using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Text.Json;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Payment.Commands.CreatePaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class CreatePaymentMethodCommandHandler : IRequestHandler<CreatePaymentMethodCommand, PaymentMethodDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePaymentMethodCommandHandler> _logger;

    public CreatePaymentMethodCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreatePaymentMethodCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentMethodDto> Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating payment method. Name: {Name}, Code: {Code}, IsActive: {IsActive}",
            request.Name, request.Code, request.IsActive);

        // CRITICAL: Transaction baslat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Check if code already exists
            var existing = await _context.Set<PaymentMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(pm => pm.Code == request.Code, cancellationToken);

            if (existing != null)
            {
                _logger.LogWarning("Payment method with code {Code} already exists", request.Code);
                throw new BusinessException($"Bu kod ile ödeme yöntemi zaten mevcut: '{request.Code}'");
            }

            // If this is default, unset other default methods
            if (request.IsDefault)
            {
                var existingDefault = await _context.Set<PaymentMethod>()
                    .Where(pm => pm.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var method in existingDefault)
                {
                    method.UnsetAsDefault();
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var settingsJson = request.Settings != null ? JsonSerializer.Serialize(request.Settings) : null;
            var paymentMethod = PaymentMethod.Create(
                request.Name,
                request.Code,
                request.Description,
                request.IconUrl,
                request.IsActive,
                request.RequiresOnlinePayment,
                request.RequiresManualVerification,
                request.MinimumAmount,
                request.MaximumAmount,
                request.ProcessingFee,
                request.ProcessingFeePercentage,
                settingsJson,
                request.DisplayOrder,
                request.IsDefault);

            await _context.Set<PaymentMethod>().AddAsync(paymentMethod, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Payment method created successfully. PaymentMethodId: {PaymentMethodId}, Name: {Name}, Code: {Code}",
                paymentMethod.Id, request.Name, request.Code);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<PaymentMethodDto>(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment method. Name: {Name}, Code: {Code}", request.Name, request.Code);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
