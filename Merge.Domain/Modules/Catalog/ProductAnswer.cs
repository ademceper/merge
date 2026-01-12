using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductAnswer Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class ProductAnswer : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid QuestionId { get; private set; }
    public Guid UserId { get; private set; }
    
    private string _answer = string.Empty;
    public string Answer 
    { 
        get => _answer; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Answer));
            if (value.Length < 5)
            {
                throw new DomainException("Cevap en az 5 karakter olmalıdır");
            }
            if (value.Length > 2000)
            {
                throw new DomainException("Cevap en fazla 2000 karakter olabilir");
            }
            _answer = value;
        } 
    }
    
    public bool IsApproved { get; private set; } = false;
    public bool IsSellerAnswer { get; private set; } = false;
    public bool IsVerifiedPurchase { get; private set; } = false;
    
    private int _helpfulCount = 0;
    public int HelpfulCount 
    { 
        get => _helpfulCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(HelpfulCount));
            _helpfulCount = value;
        } 
    }
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public ProductQuestion Question { get; private set; } = null!;
    public User User { get; private set; } = null!;
    private readonly List<AnswerHelpfulness> _helpfulnessVotes = new();
    public IReadOnlyCollection<AnswerHelpfulness> HelpfulnessVotes => _helpfulnessVotes.AsReadOnly();
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductAnswer() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductAnswer Create(
        Guid questionId,
        Guid userId,
        string answer,
        bool isSellerAnswer = false,
        bool isVerifiedPurchase = false,
        bool autoApprove = false)
    {
        Guard.AgainstDefault(questionId, nameof(questionId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(answer, nameof(answer));
        
        if (answer.Length < 5)
        {
            throw new DomainException("Cevap en az 5 karakter olmalıdır");
        }
        if (answer.Length > 2000)
        {
            throw new DomainException("Cevap en fazla 2000 karakter olabilir");
        }
        
        var productAnswer = new ProductAnswer
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            UserId = userId,
            _answer = answer,
            IsApproved = autoApprove || isSellerAnswer, // Seller answers are auto-approved
            IsSellerAnswer = isSellerAnswer,
            IsVerifiedPurchase = isVerifiedPurchase,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.4: Invariant validation
        productAnswer.ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events
        productAnswer.AddDomainEvent(new ProductAnswerCreatedEvent(productAnswer.Id, questionId, userId, isSellerAnswer));
        
        return productAnswer;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Approve
    public void Approve()
    {
        if (IsApproved) return;
        
        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new ProductAnswerApprovedEvent(Id, QuestionId));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Increment helpful count
    public void IncrementHelpfulCount()
    {
        _helpfulCount++;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Decrement helpful count
    public void DecrementHelpfulCount()
    {
        if (_helpfulCount > 0)
        {
            _helpfulCount--;
            UpdatedAt = DateTime.UtcNow;
            
            // ✅ BOLUM 1.4: Invariant validation
            ValidateInvariants();
        }
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Add helpfulness vote (collection manipulation)
    public void AddHelpfulnessVote(AnswerHelpfulness helpfulness)
    {
        Guard.AgainstNull(helpfulness, nameof(helpfulness));
        if (helpfulness.AnswerId != Id)
        {
            throw new DomainException("Helpfulness vote bu answer'a ait değil");
        }
        if (_helpfulnessVotes.Any(v => v.Id == helpfulness.Id))
        {
            throw new DomainException("Bu vote zaten eklenmiş");
        }
        _helpfulnessVotes.Add(helpfulness);
        IncrementHelpfulCount();
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Remove helpfulness vote (collection manipulation)
    public void RemoveHelpfulnessVote(Guid voteId)
    {
        Guard.AgainstDefault(voteId, nameof(voteId));
        var vote = _helpfulnessVotes.FirstOrDefault(v => v.Id == voteId);
        if (vote == null)
        {
            throw new DomainException("Vote bulunamadı");
        }
        _helpfulnessVotes.Remove(vote);
        DecrementHelpfulCount();
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events - ProductAnswerDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductAnswerDeletedEvent(Id, QuestionId, UserId));
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (Guid.Empty == QuestionId)
            throw new DomainException("Soru ID boş olamaz");

        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");

        if (string.IsNullOrWhiteSpace(_answer))
            throw new DomainException("Cevap boş olamaz");

        if (_answer.Length < 5 || _answer.Length > 2000)
            throw new DomainException("Cevap 5-2000 karakter arasında olmalıdır");

        if (_helpfulCount < 0)
            throw new DomainException("Yardımcı sayısı negatif olamaz");
    }
}

