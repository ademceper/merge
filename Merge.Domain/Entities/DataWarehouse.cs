using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Entities;

/// <summary>
/// DataWarehouse Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class DataWarehouse : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string DataSource { get; private set; } = string.Empty; // Source system identifier
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public EntityStatus Status { get; private set; } = EntityStatus.Active;
    public DateTime? LastSyncAt { get; private set; }
    public DateTime? NextSyncAt { get; private set; }
    public string? SyncFrequency { get; private set; } // Daily, Weekly, Monthly, Real-time
    public int RecordCount { get; private set; } = 0;
    public long DataSize { get; private set; } = 0; // In bytes
    public string? Schema { get; private set; } // JSON schema definition
    public string? Metadata { get; private set; } // JSON metadata

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private DataWarehouse() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static DataWarehouse Create(
        string name,
        string description,
        string dataSource,
        string? syncFrequency = null,
        string? schema = null,
        string? metadata = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(dataSource, nameof(dataSource));

        var dataWarehouse = new DataWarehouse
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            DataSource = dataSource,
            Status = EntityStatus.Active,
            SyncFrequency = syncFrequency,
            Schema = schema,
            Metadata = metadata,
            RecordCount = 0,
            DataSize = 0,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - DataWarehouseCreatedEvent yayınla
        dataWarehouse.AddDomainEvent(new DataWarehouseCreatedEvent(
            dataWarehouse.Id,
            name,
            dataSource));

        return dataWarehouse;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update sync information
    public void UpdateSyncInfo(DateTime lastSyncAt, int recordCount, long dataSize)
    {
        // ✅ BOLUM 1.6: Invariant Validation - RecordCount >= 0, DataSize >= 0
        if (recordCount < 0)
            throw new DomainException("Kayıt sayısı negatif olamaz");
        if (dataSize < 0)
            throw new DomainException("Veri boyutu negatif olamaz");

        LastSyncAt = lastSyncAt;
        RecordCount = recordCount;
        DataSize = dataSize;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - DataWarehouseSyncedEvent yayınla
        AddDomainEvent(new DataWarehouseSyncedEvent(
            Id,
            lastSyncAt,
            recordCount,
            dataSize));
    }

    // ✅ BOLUM 1.1: Domain Logic - Schedule next sync
    public void ScheduleNextSync(DateTime nextSyncAt)
    {
        if (nextSyncAt <= DateTime.UtcNow)
            throw new DomainException("Sonraki senkronizasyon tarihi gelecekte olmalıdır");

        NextSyncAt = nextSyncAt;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate data warehouse
    public void Activate()
    {
        if (Status == EntityStatus.Active)
            return;

        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - DataWarehouseActivatedEvent yayınla
        AddDomainEvent(new DataWarehouseActivatedEvent(Id));
    }

    // ✅ BOLUM 1.1: Domain Logic - Deactivate data warehouse
    public void Deactivate()
    {
        if (Status == EntityStatus.Inactive)
            return;

        Status = EntityStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - DataWarehouseDeactivatedEvent yayınla
        AddDomainEvent(new DataWarehouseDeactivatedEvent(Id));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update schema
    public void UpdateSchema(string schema)
    {
        Guard.AgainstNullOrEmpty(schema, nameof(schema));
        Schema = schema;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update metadata
    public void UpdateMetadata(string metadata)
    {
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if sync is due
    public bool IsSyncDue()
    {
        return Status == EntityStatus.Active && 
               NextSyncAt.HasValue && 
               NextSyncAt.Value <= DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - DataWarehouseDeletedEvent yayınla
        AddDomainEvent(new DataWarehouseDeletedEvent(Id));
    }
}

