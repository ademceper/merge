using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using AutoMapper;
using OrganizationEntity = Merge.Domain.Modules.Identity.Organization;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.CreateCreditTerm;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateCreditTermCommandHandler : IRequestHandler<CreateCreditTermCommand, CreditTermDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCreditTermCommandHandler> _logger;

    public CreateCreditTermCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateCreditTermCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CreditTermDto> Handle(CreateCreditTermCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating credit term. OrganizationId: {OrganizationId}, Name: {Name}",
            request.Dto.OrganizationId, request.Dto.Name);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var organization = await _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Dto.OrganizationId, cancellationToken);

        if (organization == null)
        {
            throw new NotFoundException("Organizasyon", Guid.Empty);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var creditTerm = CreditTerm.Create(
            request.Dto.OrganizationId,
            organization,
            request.Dto.Name,
            request.Dto.PaymentDays,
            request.Dto.CreditLimit,
            request.Dto.Terms);

        await _context.Set<CreditTerm>().AddAsync(creditTerm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        creditTerm = await _context.Set<CreditTerm>()
            .AsNoTracking()
            .Include(ct => ct.Organization)
            .FirstOrDefaultAsync(ct => ct.Id == creditTerm.Id, cancellationToken);

        _logger.LogInformation("Credit term created successfully. CreditTermId: {CreditTermId}", creditTerm!.Id);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<CreditTermDto>(creditTerm);
    }
}

