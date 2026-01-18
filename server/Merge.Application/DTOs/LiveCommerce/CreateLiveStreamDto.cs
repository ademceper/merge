using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.LiveCommerce;

public record CreateLiveStreamDto(
    [Required] Guid SellerId,
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    string Title,
    [StringLength(2000)] string Description,
    DateTime? ScheduledStartTime,
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    string? StreamUrl,
    [StringLength(200)] string? StreamKey,
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    string? ThumbnailUrl,
    [StringLength(100)] string? Category,
    [StringLength(500)] string? Tags);
