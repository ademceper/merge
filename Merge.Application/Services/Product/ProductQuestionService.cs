using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Modules.Identity.User;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Product;

public class ProductQuestionService : IProductQuestionService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductQuestionService> _logger;

    public ProductQuestionService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductQuestionService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    public async Task<ProductQuestionDto> AskQuestionAsync(Guid userId, CreateProductQuestionDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product question soruluyor. UserId: {UserId}, ProductId: {ProductId}",
            userId, dto.ProductId);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException("Ürün", dto.ProductId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var question = ProductQuestion.Create(
            productId: dto.ProductId,
            userId: userId,
            question: dto.Question);

        await _context.Set<ProductQuestion>().AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        question = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == question.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product question soruldu. QuestionId: {QuestionId}, UserId: {UserId}, ProductId: {ProductId}",
            question!.Id, userId, dto.ProductId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
        var questionDto = _mapper.Map<ProductQuestionDto>(question) with { HasUserVoted = false }; // Yeni soru, henüz oy verilmemiş
        return questionDto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductQuestionDto?> GetQuestionAsync(Guid questionId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !q.IsDeleted, !a.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question == null) return null;

        // ✅ PERFORMANCE: Batch load QuestionHelpfulness to avoid N+1 queries
        var hasUserVoted = userId.HasValue
            ? await _context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .AnyAsync(qh => qh.QuestionId == question.Id && qh.UserId == userId.Value, cancellationToken)
            : false;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
        var questionDto = _mapper.Map<ProductQuestionDto>(question) with { HasUserVoted = hasUserVoted };
        return questionDto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ProductQuestionDto>> GetProductQuestionsAsync(Guid productId, Guid? userId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !q.IsDeleted, !a.IsDeleted (Global Query Filter)
        var query = _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.ProductId == productId && q.IsApproved);

        var totalCount = await query.CountAsync(cancellationToken);

        var questions = await query
            .OrderByDescending(q => q.HasSellerAnswer)
            .ThenByDescending(q => q.HelpfulCount)
            .ThenByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load QuestionHelpfulness to avoid N+1 queries
        var questionIds = questions.Select(q => q.Id).ToList();
        var userVotes = userId.HasValue && questionIds.Any()
            ? await _context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .Where(qh => questionIds.Contains(qh.QuestionId) && qh.UserId == userId.Value)
                .ToDictionaryAsync(qh => qh.QuestionId, cancellationToken)
            : new Dictionary<Guid, QuestionHelpfulness>();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
        var dtos = new List<ProductQuestionDto>(questions.Count);
        foreach (var question in questions)
        {
            var dto = _mapper.Map<ProductQuestionDto>(question) with { HasUserVoted = userId.HasValue && userVotes.ContainsKey(question.Id) };
            dtos.Add(dto);
        }

        return new PagedResult<ProductQuestionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ProductQuestionDto>> GetUserQuestionsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !q.IsDeleted, !a.IsDeleted (Global Query Filter)
        var query = _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers)
                .ThenInclude(a => a.User)
            .Where(q => q.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load QuestionHelpfulness to avoid N+1 queries
        var questionIds = questions.Select(q => q.Id).ToList();
        var userVotes = questionIds.Any()
            ? await _context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .Where(qh => questionIds.Contains(qh.QuestionId) && qh.UserId == userId)
                .ToDictionaryAsync(qh => qh.QuestionId, cancellationToken)
            : new Dictionary<Guid, QuestionHelpfulness>();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
        var dtos = new List<ProductQuestionDto>(questions.Count);
        foreach (var question in questions)
        {
            var dto = _mapper.Map<ProductQuestionDto>(question) with { HasUserVoted = userVotes.ContainsKey(question.Id) };
            dtos.Add(dto);
        }

        return new PagedResult<ProductQuestionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ApproveQuestionAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        question.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteQuestionAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question == null) return false;

        question.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ProductAnswerDto> AnswerQuestionAsync(Guid userId, CreateProductAnswerDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .Include(q => q.Product)
            .FirstOrDefaultAsync(q => q.Id == dto.QuestionId, cancellationToken);

        if (question == null)
        {
            throw new NotFoundException("Soru", dto.QuestionId);
        }

        // Check if user is seller of the product
        var product = question.Product;
        var isSellerAnswer = product != null && product.SellerId.HasValue && product.SellerId.Value == userId;

        // ✅ PERFORMANCE: Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // Check if user has purchased this product
        var hasOrder = await _context.Set<OrderItem>()
            .AsNoTracking()
            .AnyAsync(oi => oi.ProductId == question.ProductId &&
                          oi.Order.UserId == userId &&
                          oi.Order.PaymentStatus == PaymentStatus.Completed, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var answer = ProductAnswer.Create(
            questionId: dto.QuestionId,
            userId: userId,
            answer: dto.Answer,
            isSellerAnswer: isSellerAnswer,
            isVerifiedPurchase: hasOrder,
            autoApprove: false); // Requires admin approval

        await _context.Set<ProductAnswer>().AddAsync(answer, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        question.IncrementAnswerCount(isSellerAnswer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        answer = await _context.Set<ProductAnswer>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == answer.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
        var answerDto = _mapper.Map<ProductAnswerDto>(answer) with { HasUserVoted = false }; // Yeni cevap, henüz oy verilmemiş
        return answerDto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductAnswerDto>> GetQuestionAnswersAsync(Guid questionId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var answers = await _context.Set<ProductAnswer>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.QuestionId == questionId && a.IsApproved)
            .OrderByDescending(a => a.IsSellerAnswer)
            .ThenByDescending(a => a.HelpfulCount)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load AnswerHelpfulness to avoid N+1 queries
        var answerIds = answers.Select(a => a.Id).ToList();
        var userVotes = userId.HasValue && answerIds.Any()
            ? await _context.Set<AnswerHelpfulness>()
                .AsNoTracking()
                .Where(ah => answerIds.Contains(ah.AnswerId) && ah.UserId == userId.Value)
                .ToDictionaryAsync(ah => ah.AnswerId, cancellationToken)
            : new Dictionary<Guid, AnswerHelpfulness>();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı (object initializer YASAK)
        var dtos = new List<ProductAnswerDto>(answers.Count);
        foreach (var answer in answers)
        {
            var dto = _mapper.Map<ProductAnswerDto>(answer) with { HasUserVoted = userId.HasValue && userVotes.ContainsKey(answer.Id) };
            dtos.Add(dto);
        }

        return dtos;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ApproveAnswerAsync(Guid answerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var answer = await _context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        answer.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteAnswerAsync(Guid answerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var answer = await _context.Set<ProductAnswer>()
            .Include(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer == null) return false;

        answer.MarkAsDeleted();

        // Update question stats
        if (answer.Question != null)
        {
            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            // Check if there are other seller answers
            var hasOtherSellerAnswer = answer.IsSellerAnswer
                ? await _context.Set<ProductAnswer>()
                    .AnyAsync(a => a.QuestionId == answer.QuestionId && a.IsSellerAnswer && a.Id != answerId, cancellationToken)
                : false;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            answer.Question.DecrementAnswerCount(answer.IsSellerAnswer);
            
            if (answer.IsSellerAnswer && !hasOtherSellerAnswer)
            {
                answer.Question.SetHasSellerAnswer(false);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task MarkQuestionHelpfulAsync(Guid userId, Guid questionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !qh.IsDeleted (Global Query Filter)
        var existing = await _context.Set<QuestionHelpfulness>()
            .FirstOrDefaultAsync(qh => qh.QuestionId == questionId && qh.UserId == userId, cancellationToken);

        if (existing != null) return; // Already marked

        var vote = new QuestionHelpfulness
        {
            QuestionId = questionId,
            UserId = userId
        };

        await _context.Set<QuestionHelpfulness>().AddAsync(vote, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            question.IncrementHelpfulCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task UnmarkQuestionHelpfulAsync(Guid userId, Guid questionId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !qh.IsDeleted (Global Query Filter)
        var vote = await _context.Set<QuestionHelpfulness>()
            .FirstOrDefaultAsync(qh => qh.QuestionId == questionId && qh.UserId == userId, cancellationToken);

        if (vote == null) return;

        vote.MarkAsDeleted();

        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            question.DecrementHelpfulCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task MarkAnswerHelpfulAsync(Guid userId, Guid answerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !ah.IsDeleted (Global Query Filter)
        var existing = await _context.Set<AnswerHelpfulness>()
            .FirstOrDefaultAsync(ah => ah.AnswerId == answerId && ah.UserId == userId, cancellationToken);

        if (existing != null) return; // Already marked

        var vote = new AnswerHelpfulness
        {
            AnswerId = answerId,
            UserId = userId
        };

        await _context.Set<AnswerHelpfulness>().AddAsync(vote, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var answer = await _context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            answer.IncrementHelpfulCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task UnmarkAnswerHelpfulAsync(Guid userId, Guid answerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !ah.IsDeleted (Global Query Filter)
        var vote = await _context.Set<AnswerHelpfulness>()
            .FirstOrDefaultAsync(ah => ah.AnswerId == answerId && ah.UserId == userId, cancellationToken);

        if (vote == null) return;

        vote.MarkAsDeleted();

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var answer = await _context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            answer.DecrementHelpfulCount();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<QAStatsDto> GetQAStatsAsync(Guid? productId = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !q.IsDeleted, !a.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var questionsQuery = _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Where(q => q.IsApproved);

        if (productId.HasValue)
        {
            questionsQuery = questionsQuery.Where(q => q.ProductId == productId.Value);
        }

        var totalQuestions = await questionsQuery.CountAsync(cancellationToken);
        var unansweredQuestions = await questionsQuery.CountAsync(q => q.AnswerCount == 0, cancellationToken);
        var questionsWithSellerAnswer = await questionsQuery.CountAsync(q => q.HasSellerAnswer, cancellationToken);

        var answersQuery = _context.Set<ProductAnswer>()
            .AsNoTracking()
            .Where(a => a.IsApproved);

        if (productId.HasValue)
        {
            answersQuery = answersQuery.Where(a => a.Question.ProductId == productId.Value);
        }

        var totalAnswers = await answersQuery.CountAsync(cancellationToken);
        var averageAnswersPerQuestion = totalQuestions > 0 ? (decimal)totalAnswers / totalQuestions : 0;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !q.IsDeleted, !a.IsDeleted (Global Query Filter)
        var recentQuestions = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.IsApproved)
            .OrderByDescending(q => q.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var mostHelpfulQuestions = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.IsApproved)
            .OrderByDescending(q => q.HelpfulCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recentDtos = _mapper.Map<IEnumerable<ProductQuestionDto>>(recentQuestions).ToList();
        var helpfulDtos = _mapper.Map<IEnumerable<ProductQuestionDto>>(mostHelpfulQuestions).ToList();

        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        return new QAStatsDto(
            TotalQuestions: totalQuestions,
            TotalAnswers: totalAnswers,
            UnansweredQuestions: unansweredQuestions,
            QuestionsWithSellerAnswer: questionsWithSellerAnswer,
            AverageAnswersPerQuestion: averageAnswersPerQuestion,
            RecentQuestions: recentDtos,
            MostHelpfulQuestions: helpfulDtos
        );
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductQuestionDto>> GetUnansweredQuestionsAsync(Guid? productId = null, int limit = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !q.IsDeleted (Global Query Filter)
        var query = _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.IsApproved && q.AnswerCount == 0);

        if (productId.HasValue)
        {
            query = query.Where(q => q.ProductId == productId.Value);
        }

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductQuestionDto>>(questions);
    }

}
