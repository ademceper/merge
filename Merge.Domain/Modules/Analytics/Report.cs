using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Analytics;

/// <summary>
/// Report Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Report : BaseAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ReportType Type { get; private set; } = ReportType.Sales;
    public Guid GeneratedBy { get; private set; }
    public User GeneratedByUser { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string? Filters { get; private set; } // JSON object of applied filters
    public string? Data { get; private set; } // JSON serialized report data
    public string? FilePath { get; private set; } // Path to exported file if applicable
    public ReportFormat Format { get; private set; } = ReportFormat.JSON;
    public ReportStatus Status { get; private set; } = ReportStatus.Pending;
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Report() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Report Create(
        string name,
        string description,
        ReportType type,
        Guid generatedBy,
        DateTime startDate,
        DateTime endDate,
        string? filters = null,
        ReportFormat format = ReportFormat.JSON)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstDefault(generatedBy, nameof(generatedBy));

        // ✅ BOLUM 1.6: Invariant Validation - StartDate < EndDate
        if (startDate >= endDate)
            throw new DomainException("Başlangıç tarihi bitiş tarihinden önce olmalıdır");

        if (endDate > DateTime.UtcNow)
            throw new DomainException("Bitiş tarihi gelecekte olamaz");

        var report = new Report
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Type = type,
            GeneratedBy = generatedBy,
            StartDate = startDate,
            EndDate = endDate,
            Filters = filters,
            Format = format,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - ReportCreatedEvent yayınla
        report.AddDomainEvent(new ReportCreatedEvent(
            report.Id,
            generatedBy,
            type.ToString(),
            startDate,
            endDate));

        return report;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark report as processing
    public void MarkAsProcessing()
    {
        if (Status != ReportStatus.Pending)
            throw new InvalidOperationException($"Rapor {Status} durumunda, processing'e geçirilemez");

        Status = ReportStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Complete report
    public void Complete(string data, string? filePath = null)
    {
        if (Status != ReportStatus.Processing)
            throw new InvalidOperationException($"Rapor {Status} durumunda, complete edilemez");

        Guard.AgainstNullOrEmpty(data, nameof(data));

        Status = ReportStatus.Completed;
        Data = data;
        FilePath = filePath;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ReportCompletedEvent yayınla
        AddDomainEvent(new ReportCompletedEvent(
            Id,
            GeneratedBy,
            Type.ToString(),
            CompletedAt.Value));
    }

    // ✅ BOLUM 1.1: Domain Logic - Fail report
    public void Fail(string errorMessage)
    {
        if (Status == ReportStatus.Completed)
            throw new InvalidOperationException("Tamamlanmış rapor başarısız olarak işaretlenemez");

        Guard.AgainstNullOrEmpty(errorMessage, nameof(errorMessage));

        Status = ReportStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - ReportFailedEvent yayınla
        AddDomainEvent(new ReportFailedEvent(
            Id,
            GeneratedBy,
            Type.ToString(),
            errorMessage));
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if report is ready for export
    public bool IsReadyForExport()
    {
        return Status == ReportStatus.Completed && !string.IsNullOrEmpty(Data);
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

