using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetMyReferralCode;

// ✅ BOLUM 2.0: CQRS - Query handler'lar SADECE okuma yapmalı, entity oluşturma YASAK
public record GetMyReferralCodeQuery(
    Guid UserId) : IRequest<ReferralCodeDto?>;
