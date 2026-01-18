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
    public Guid ProductId { get; private set; }
    public Guid UserId { get; private set; }
    
    private static class ValidationConstants
    {
        public const int MinQuestionLength = 5;
        public const int MaxQuestionLength = 1000;
    }

    private string _question = string.Empty;
    public string Question 
    { 
        get => _question; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Question));
            Guard.AgainstOutOfRange(value.Length, ValidationConstants.MinQuestionLength, ValidationConstants.MaxQuestionLength, nameof(Question));
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
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Product Product { get; private set; } = null!;
    public User User { get; private set; } = null!;
    private readonly List<ProductAnswer> _answers = new();
    public IReadOnlyCollection<ProductAnswer> Answers => _answers.AsReadOnly();
    private readonly List<QuestionHelpfulness> _helpfulnessVotes = new();
    public IReadOnlyCollection<QuestionHelpfulness> HelpfulnessVotes => _helpfulnessVotes.AsReadOnly();
    
    private ProductQuestion() { }
    
    public static ProductQuestion Create(
        Guid productId,
        Guid userId,
        string question)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(question, nameof(question));
        Guard.AgainstOutOfRange(question.Length, ValidationConstants.MinQuestionLength, ValidationConstants.MaxQuestionLength, nameof(question));
        
        var productQuestion = new ProductQuestion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UserId = userId,
            _question = question,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };
        
        productQuestion.ValidateInvariants();
        
        productQuestion.AddDomainEvent(new ProductQuestionCreatedEvent(productQuestion.Id, productId, userId, question));
        
        return productQuestion;
    }
    
    public void Approve()
    {
        if (IsApproved) return;
        
        IsApproved = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new ProductQuestionApprovedEvent(Id, ProductId));
    }
    
    public void IncrementAnswerCount(bool isSellerAnswer = false)
    {
        _answerCount++;
        if (isSellerAnswer)
        {
            HasSellerAnswer = true;
        }
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void IncrementHelpfulCount()
    {
        _helpfulCount++;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void DecrementHelpfulCount()
    {
        if (_helpfulCount > 0)
        {
            _helpfulCount--;
            UpdatedAt = DateTime.UtcNow;
            
            ValidateInvariants();
        }
    }
    
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
            
            ValidateInvariants();
        }
    }
    
    public void SetHasSellerAnswer(bool value)
    {
        HasSellerAnswer = value;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        // Seller answer durumu önemli bir business event'tir
        AddDomainEvent(new ProductQuestionUpdatedEvent(Id, ProductId, UserId, _answerCount, _helpfulCount, HasSellerAnswer));
    }
    
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
        
        ValidateInvariants();
        
        // Answer ekleme önemli bir business event'tir
        AddDomainEvent(new ProductQuestionUpdatedEvent(Id, ProductId, UserId, _answerCount, _helpfulCount, HasSellerAnswer));
    }
    
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
        
        ValidateInvariants();
        
        // Answer silme önemli bir business event'tir
        AddDomainEvent(new ProductQuestionUpdatedEvent(Id, ProductId, UserId, _answerCount, _helpfulCount, HasSellerAnswer));
    }
    
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
        
        ValidateInvariants();
        
        // Helpfulness vote ekleme önemli bir business event'tir
        AddDomainEvent(new ProductQuestionUpdatedEvent(Id, ProductId, UserId, _answerCount, _helpfulCount, HasSellerAnswer));
    }
    
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
        
        ValidateInvariants();
        
        // Helpfulness vote silme önemli bir business event'tir
        AddDomainEvent(new ProductQuestionUpdatedEvent(Id, ProductId, UserId, _answerCount, _helpfulCount, HasSellerAnswer));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new ProductQuestionDeletedEvent(Id, ProductId, UserId));
    }

    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(_question))
            throw new DomainException("Soru boş olamaz");

        Guard.AgainstOutOfRange(_question.Length, ValidationConstants.MinQuestionLength, ValidationConstants.MaxQuestionLength, nameof(Question));

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

