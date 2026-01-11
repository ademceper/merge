namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record BulkProductExportDto(
    Guid? CategoryId,
    bool ActiveOnly = true,
    bool IncludeVariants = false
);
