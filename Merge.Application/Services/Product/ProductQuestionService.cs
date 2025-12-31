using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Entities.User;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Product;


namespace Merge.Application.Services.Product;

public class ProductQuestionService : IProductQuestionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductQuestionService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductQuestionDto> AskQuestionAsync(Guid userId, CreateProductQuestionDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
        {
            throw new NotFoundException("Ürün", dto.ProductId);
        }

        var question = new ProductQuestion
        {
            ProductId = dto.ProductId,
            UserId = userId,
            Question = dto.Question,
            IsApproved = false // Requires admin approval
        };

        await _context.Set<ProductQuestion>().AddAsync(question);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        question = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == question.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var questionDto = _mapper.Map<ProductQuestionDto>(question);
        questionDto.HasUserVoted = false; // Yeni soru, henüz oy verilmemiş
        return questionDto;
    }

    public async Task<ProductQuestionDto?> GetQuestionAsync(Guid questionId, Guid? userId = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !q.IsDeleted, !a.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null) return null;

        // ✅ PERFORMANCE: Batch load QuestionHelpfulness to avoid N+1 queries
        var hasUserVoted = userId.HasValue
            ? await _context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .AnyAsync(qh => qh.QuestionId == question.Id && qh.UserId == userId.Value)
            : false;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var questionDto = _mapper.Map<ProductQuestionDto>(question);
        questionDto.HasUserVoted = hasUserVoted;
        return questionDto;
    }

    public async Task<IEnumerable<ProductQuestionDto>> GetProductQuestionsAsync(Guid productId, Guid? userId = null, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !q.IsDeleted, !a.IsDeleted (Global Query Filter)
        var questions = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.ProductId == productId && q.IsApproved)
            .OrderByDescending(q => q.HasSellerAnswer)
            .ThenByDescending(q => q.HelpfulCount)
            .ThenByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load QuestionHelpfulness to avoid N+1 queries
        var questionIds = questions.Select(q => q.Id).ToList();
        var userVotes = userId.HasValue
            ? await _context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .Where(qh => questionIds.Contains(qh.QuestionId) && qh.UserId == userId.Value)
                .ToDictionaryAsync(qh => qh.QuestionId)
            : new Dictionary<Guid, QuestionHelpfulness>();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dtos = new List<ProductQuestionDto>();
        foreach (var question in questions)
        {
            var dto = _mapper.Map<ProductQuestionDto>(question);
            dto.HasUserVoted = userId.HasValue && userVotes.ContainsKey(question.Id);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<IEnumerable<ProductQuestionDto>> GetUserQuestionsAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !q.IsDeleted, !a.IsDeleted (Global Query Filter)
        var questions = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers)
                .ThenInclude(a => a.User)
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load QuestionHelpfulness to avoid N+1 queries
        var questionIds = questions.Select(q => q.Id).ToList();
        var userVotes = await _context.Set<QuestionHelpfulness>()
            .AsNoTracking()
            .Where(qh => questionIds.Contains(qh.QuestionId) && qh.UserId == userId)
            .ToDictionaryAsync(qh => qh.QuestionId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dtos = new List<ProductQuestionDto>();
        foreach (var question in questions)
        {
            var dto = _mapper.Map<ProductQuestionDto>(question);
            dto.HasUserVoted = userVotes.ContainsKey(question.Id);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<bool> ApproveQuestionAsync(Guid questionId)
    {
        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null) return false;

        question.IsApproved = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteQuestionAsync(Guid questionId)
    {
        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null) return false;

        question.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<ProductAnswerDto> AnswerQuestionAsync(Guid userId, CreateProductAnswerDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .Include(q => q.Product)
            .FirstOrDefaultAsync(q => q.Id == dto.QuestionId);

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
                          oi.Order.PaymentStatus == "Paid");

        var answer = new ProductAnswer
        {
            QuestionId = dto.QuestionId,
            UserId = userId,
            Answer = dto.Answer,
            IsApproved = false, // Requires admin approval
            IsSellerAnswer = isSellerAnswer,
            IsVerifiedPurchase = hasOrder
        };

        await _context.Set<ProductAnswer>().AddAsync(answer);

        // Update question stats
        question.AnswerCount++;
        if (isSellerAnswer)
        {
            question.HasSellerAnswer = true;
        }

        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        answer = await _context.ProductAnswers
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == answer.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var answerDto = _mapper.Map<ProductAnswerDto>(answer);
        answerDto.HasUserVoted = false; // Yeni cevap, henüz oy verilmemiş
        return answerDto;
    }

    public async Task<IEnumerable<ProductAnswerDto>> GetQuestionAnswersAsync(Guid questionId, Guid? userId = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var answers = await _context.Set<ProductAnswer>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.QuestionId == questionId && a.IsApproved)
            .OrderByDescending(a => a.IsSellerAnswer)
            .ThenByDescending(a => a.HelpfulCount)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load AnswerHelpfulness to avoid N+1 queries
        var answerIds = answers.Select(a => a.Id).ToList();
        var userVotes = userId.HasValue
            ? await _context.Set<AnswerHelpfulness>()
                .AsNoTracking()
                .Where(ah => answerIds.Contains(ah.AnswerId) && ah.UserId == userId.Value)
                .ToDictionaryAsync(ah => ah.AnswerId)
            : new Dictionary<Guid, AnswerHelpfulness>();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dtos = new List<ProductAnswerDto>();
        foreach (var answer in answers)
        {
            var dto = _mapper.Map<ProductAnswerDto>(answer);
            dto.HasUserVoted = userId.HasValue && userVotes.ContainsKey(answer.Id);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<bool> ApproveAnswerAsync(Guid answerId)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var answer = await _context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId);

        if (answer == null) return false;

        answer.IsApproved = true;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAnswerAsync(Guid answerId)
    {
        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var answer = await _context.Set<ProductAnswer>()
            .Include(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == answerId);

        if (answer == null) return false;

        answer.IsDeleted = true;

        // Update question stats
        if (answer.Question != null)
        {
            answer.Question.AnswerCount = Math.Max(0, answer.Question.AnswerCount - 1);

            if (answer.IsSellerAnswer)
            {
                // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
                // Check if there are other seller answers
                var hasOtherSellerAnswer = await _context.Set<ProductAnswer>()
                    .AnyAsync(a => a.QuestionId == answer.QuestionId && a.IsSellerAnswer && a.Id != answerId);

                if (!hasOtherSellerAnswer)
                {
                    answer.Question.HasSellerAnswer = false;
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task MarkQuestionHelpfulAsync(Guid userId, Guid questionId)
    {
        // ✅ PERFORMANCE: Removed manual !qh.IsDeleted (Global Query Filter)
        var existing = await _context.Set<QuestionHelpfulness>()
            .FirstOrDefaultAsync(qh => qh.QuestionId == questionId && qh.UserId == userId);

        if (existing != null) return; // Already marked

        var vote = new QuestionHelpfulness
        {
            QuestionId = questionId,
            UserId = userId
        };

        await _context.Set<QuestionHelpfulness>().AddAsync(vote);

        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question != null)
        {
            question.HelpfulCount++;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnmarkQuestionHelpfulAsync(Guid userId, Guid questionId)
    {
        // ✅ PERFORMANCE: Removed manual !qh.IsDeleted (Global Query Filter)
        var vote = await _context.Set<QuestionHelpfulness>()
            .FirstOrDefaultAsync(qh => qh.QuestionId == questionId && qh.UserId == userId);

        if (vote == null) return;

        vote.IsDeleted = true;

        // ✅ PERFORMANCE: Removed manual !q.IsDeleted (Global Query Filter)
        var question = await _context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question != null)
        {
            question.HelpfulCount = Math.Max(0, question.HelpfulCount - 1);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAnswerHelpfulAsync(Guid userId, Guid answerId)
    {
        // ✅ PERFORMANCE: Removed manual !ah.IsDeleted (Global Query Filter)
        var existing = await _context.Set<AnswerHelpfulness>()
            .FirstOrDefaultAsync(ah => ah.AnswerId == answerId && ah.UserId == userId);

        if (existing != null) return; // Already marked

        var vote = new AnswerHelpfulness
        {
            AnswerId = answerId,
            UserId = userId
        };

        await _context.Set<AnswerHelpfulness>().AddAsync(vote);

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var answer = await _context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId);

        if (answer != null)
        {
            answer.HelpfulCount++;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnmarkAnswerHelpfulAsync(Guid userId, Guid answerId)
    {
        // ✅ PERFORMANCE: Removed manual !ah.IsDeleted (Global Query Filter)
        var vote = await _context.Set<AnswerHelpfulness>()
            .FirstOrDefaultAsync(ah => ah.AnswerId == answerId && ah.UserId == userId);

        if (vote == null) return;

        vote.IsDeleted = true;

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var answer = await _context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId);

        if (answer != null)
        {
            answer.HelpfulCount = Math.Max(0, answer.HelpfulCount - 1);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<QAStatsDto> GetQAStatsAsync(Guid? productId = null)
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

        var totalQuestions = await questionsQuery.CountAsync();
        var unansweredQuestions = await questionsQuery.CountAsync(q => q.AnswerCount == 0);
        var questionsWithSellerAnswer = await questionsQuery.CountAsync(q => q.HasSellerAnswer);

        var answersQuery = _context.Set<ProductAnswer>()
            .AsNoTracking()
            .Where(a => a.IsApproved);

        if (productId.HasValue)
        {
            answersQuery = answersQuery.Where(a => a.Question.ProductId == productId.Value);
        }

        var totalAnswers = await answersQuery.CountAsync();
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
            .ToListAsync();

        var mostHelpfulQuestions = await _context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.IsApproved)
            .OrderByDescending(q => q.HelpfulCount)
            .Take(10)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var recentDtos = _mapper.Map<IEnumerable<ProductQuestionDto>>(recentQuestions).ToList();
        var helpfulDtos = _mapper.Map<IEnumerable<ProductQuestionDto>>(mostHelpfulQuestions).ToList();

        return new QAStatsDto
        {
            TotalQuestions = totalQuestions,
            TotalAnswers = totalAnswers,
            UnansweredQuestions = unansweredQuestions,
            QuestionsWithSellerAnswer = questionsWithSellerAnswer,
            AverageAnswersPerQuestion = averageAnswersPerQuestion,
            RecentQuestions = recentDtos,
            MostHelpfulQuestions = helpfulDtos
        };
    }

    public async Task<IEnumerable<ProductQuestionDto>> GetUnansweredQuestionsAsync(Guid? productId = null, int limit = 20)
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
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductQuestionDto>>(questions);
    }

}
