using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductQuestion Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class ProductQuestion : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    
    private string _question = string.Empty;
    public string Question 
    { 
        get => _question; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Question));
            if (value.Length < 5)
            {
                throw new DomainException("Soru en az 5 karakter olmalıdır");
            }
            if (value.Length > 1000)
            {
                throw new DomainException("Soru en fazla 1000 karakter olabilir");
            }
            _question = value;
        } 
    }
    
    public bool IsApproved { get; private set; } = false;
    
    private int _answerCount = 0;
    public int AnswerCount 
    { 
        get => _answerCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(AnswerCount));
            _answerCount = value;
        } 
    }
    
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
    
    public bool HasSellerAnswer { get; private set; } = false;
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public User User { get; private set; } = null!;
    private readonly List<ProductAnswer> _answers = new();
    public IReadOnlyCollection<ProductAnswer> Answers => _answers.AsReadOnly();
    private readonly List<QuestionHelpfulness> _helpfulnessVotes = new();
    public IReadOnlyCollection<QuestionHelpfulness> HelpfulnessVotes => _helpfulnessVotes.AsReadOnly();
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductQuestion() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductQuestion Create(
        Guid productId,
        Guid userId,
        string question)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(question, nameof(question));
        
        if (question.Length < 5)
        {
            throw new DomainException("Soru en az 5 karakter olmalıdır");
        }
        if (question.Length > 1000)
        {
            throw new DomainException("Soru en fazla 1000 karakter olabilir");
        }
        
        var productQuestion = new ProductQuestion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            _question = question,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.4: Invariant validation
        productQuestion.ValidateInvariants();
        
        // ✅ BOLUM 1.5: Domain Events
        productQuestion.AddDomainEvent(new ProductQuestionCreatedEvent(productQuestion.Id, productId, userId, question));
        
        return productQuestion;
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
        AddDomainEvent(new ProductQuestionApprovedEvent(Id, ProductId));
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Increment answer count
    public void IncrementAnswerCount(bool isSellerAnswer = false)
    {
        _answerCount++;
        if (isSellerAnswer)
        {
            HasSellerAnswer = true;
        }
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
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
    
    // ✅ BOLUM 1.1: Domain Logic - Decrement answer count
    public void DecrementAnswerCount(bool isSellerAnswer = false)
    {
        if (_answerCount > 0)
        {
            _answerCount--;
            UpdatedAt = DateTime.UtcNow;
            
            if (isSellerAnswer)
            {
                // Check if there are other seller answers - this should be checked by caller
                // For now, we'll just mark it as false if it was a seller answer
                HasSellerAnswer = false;
            }
            
            // ✅ BOLUM 1.4: Invariant validation
            ValidateInvariants();
        }
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Set has seller answer
    public void SetHasSellerAnswer(bool value)
    {
        HasSellerAnswer = value;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Add answer (collection manipulation)
    public void AddAnswer(ProductAnswer answer)
    {
        Guard.AgainstNull(answer, nameof(answer));
        if (answer.QuestionId != Id)
        {
            throw new DomainException("Answer bu question'a ait değil");
        }
        if (_answers.Any(a => a.Id == answer.Id))
        {
            throw new DomainException("Bu answer zaten eklenmiş");
        }
        _answers.Add(answer);
        IncrementAnswerCount(answer.IsSellerAnswer);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Remove answer (collection manipulation)
    public void RemoveAnswer(Guid answerId)
    {
        Guard.AgainstDefault(answerId, nameof(answerId));
        var answer = _answers.FirstOrDefault(a => a.Id == answerId);
        if (answer == null)
        {
            throw new DomainException("Answer bulunamadı");
        }
        var wasSellerAnswer = answer.IsSellerAnswer;
        _answers.Remove(answer);
        DecrementAnswerCount(wasSellerAnswer);
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Add helpfulness vote (collection manipulation)
    public void AddHelpfulnessVote(QuestionHelpfulness helpfulness)
    {
        Guard.AgainstNull(helpfulness, nameof(helpfulness));
        if (helpfulness.QuestionId != Id)
        {
            throw new DomainException("Helpfulness vote bu question'a ait değil");
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
        
        // ✅ BOLUM 1.5: Domain Events - ProductQuestionDeletedEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new ProductQuestionDeletedEvent(Id, ProductId, UserId));
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(_question))
            throw new DomainException("Soru boş olamaz");

        if (_question.Length < 5 || _question.Length > 1000)
            throw new DomainException("Soru 5-1000 karakter arasında olmalıdır");

        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");

        if (_answerCount < 0)
            throw new DomainException("Cevap sayısı negatif olamaz");

        if (_helpfulCount < 0)
            throw new DomainException("Yardımcı sayısı negatif olamaz");
    }
}

