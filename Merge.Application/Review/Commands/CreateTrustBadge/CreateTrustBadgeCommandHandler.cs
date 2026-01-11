using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;

namespace Merge.Application.Review.Commands.CreateTrustBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateTrustBadgeCommandHandler : IRequestHandler<CreateTrustBadgeCommand, TrustBadgeDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTrustBadgeCommandHandler> _logger;

    public CreateTrustBadgeCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateTrustBadgeCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TrustBadgeDto> Handle(CreateTrustBadgeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating trust badge. Name: {Name}, BadgeType: {BadgeType}",
            request.Name, request.BadgeType);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var criteriaJson = request.Criteria != null ? JsonSerializer.Serialize(request.Criteria) : string.Empty;
        var badge = TrustBadge.Create(
            request.Name,
            request.Description,
            request.IconUrl,
            request.BadgeType,
            criteriaJson,
            request.DisplayOrder,
            request.Color);

        if (!request.IsActive)
        {
            badge.Deactivate();
        }

        await _context.Set<TrustBadge>().AddAsync(badge, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Trust badge created successfully. BadgeId: {BadgeId}, Name: {Name}",
            badge.Id, badge.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<TrustBadgeDto>(badge);
    }
}
