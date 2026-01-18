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

public class ProductQuestionService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductQuestionService> logger) : IProductQuestionService
{

    public async Task<ProductQuestionDto> AskQuestionAsync(Guid userId, CreateProductQuestionDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Product question soruluyor. UserId: {UserId}, ProductId: {ProductId}",
            userId, dto.ProductId);

        var product = await context.Set<ProductEntity>()
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Ürün", dto.ProductId);
        }

        var question = ProductQuestion.Create(
            productId: dto.ProductId,
            userId: userId,
            question: dto.Question);

        await context.Set<ProductQuestion>().AddAsync(question, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        question = await context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == question.Id, cancellationToken);

        logger.LogInformation(
            "Product question soruldu. QuestionId: {QuestionId}, UserId: {UserId}, ProductId: {ProductId}",
            question!.Id, userId, dto.ProductId);

        var questionDto = mapper.Map<ProductQuestionDto>(question) with { HasUserVoted = false }; // Yeni soru, henüz oy verilmemiş
        return questionDto;
    }

    public async Task<ProductQuestionDto?> GetQuestionAsync(Guid questionId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var question = await context.Set<ProductQuestion>()
            .AsNoTracking()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question is null) return null;

        var hasUserVoted = userId.HasValue
            ? await context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .AnyAsync(qh => qh.QuestionId == question.Id && qh.UserId == userId.Value, cancellationToken)
            : false;

        var questionDto = mapper.Map<ProductQuestionDto>(question) with { HasUserVoted = hasUserVoted };
        return questionDto;
    }

    public async Task<PagedResult<ProductQuestionDto>> GetProductQuestionsAsync(Guid productId, Guid? userId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = context.Set<ProductQuestion>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.ProductId == productId && q.IsApproved);

        var totalCount = await query.CountAsync(cancellationToken);

        var paginatedQuestionsQuery = query
            .OrderByDescending(q => q.HasSellerAnswer)
            .ThenByDescending(q => q.HelpfulCount)
            .ThenByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var questions = await paginatedQuestionsQuery.ToListAsync(cancellationToken);

        var questionIdsSubquery = from q in paginatedQuestionsQuery select q.Id;
        var userVotes = userId.HasValue
            ? await context.Set<QuestionHelpfulness>()
                .AsNoTracking()
                .Where(qh => questionIdsSubquery.Contains(qh.QuestionId) && qh.UserId == userId.Value)
                .ToDictionaryAsync(qh => qh.QuestionId, cancellationToken)
            : new Dictionary<Guid, QuestionHelpfulness>();

        var dtos = new List<ProductQuestionDto>(questions.Count);
        foreach (var question in questions)
        {
            var dto = mapper.Map<ProductQuestionDto>(question) with { HasUserVoted = userId.HasValue && userVotes.ContainsKey(question.Id) };
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

    public async Task<PagedResult<ProductQuestionDto>> GetUserQuestionsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = context.Set<ProductQuestion>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers)
                .ThenInclude(a => a.User)
            .Where(q => q.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var paginatedQuestionsQuery = query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var questions = await paginatedQuestionsQuery.ToListAsync(cancellationToken);

        var questionIdsSubquery = from q in paginatedQuestionsQuery select q.Id;
        var userVotes = await context.Set<QuestionHelpfulness>()
            .AsNoTracking()
            .Where(qh => questionIdsSubquery.Contains(qh.QuestionId) && qh.UserId == userId)
            .ToDictionaryAsync(qh => qh.QuestionId, cancellationToken);

        var dtos = new List<ProductQuestionDto>(questions.Count);
        foreach (var question in questions)
        {
            var dto = mapper.Map<ProductQuestionDto>(question) with { HasUserVoted = userVotes.ContainsKey(question.Id) };
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

    public async Task<bool> ApproveQuestionAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question is null) return false;

        question.Approve();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteQuestionAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question is null) return false;

        question.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ProductAnswerDto> AnswerQuestionAsync(Guid userId, CreateProductAnswerDto dto, CancellationToken cancellationToken = default)
    {
        var question = await context.Set<ProductQuestion>()
            .Include(q => q.Product)
            .FirstOrDefaultAsync(q => q.Id == dto.QuestionId, cancellationToken);

        if (question is null)
        {
            throw new NotFoundException("Soru", dto.QuestionId);
        }

        // Check if user is seller of the product
        var product = question.Product;
        var isSellerAnswer = product is not null && product.SellerId.HasValue && product.SellerId.Value == userId;

        // Check if user has purchased this product
        var hasOrder = await context.Set<OrderItem>()
            .AsNoTracking()
            .AnyAsync(oi => oi.ProductId == question.ProductId &&
                          oi.Order.UserId == userId &&
                          oi.Order.PaymentStatus == PaymentStatus.Completed, cancellationToken);

        var answer = ProductAnswer.Create(
            questionId: dto.QuestionId,
            userId: userId,
            answer: dto.Answer,
            isSellerAnswer: isSellerAnswer,
            isVerifiedPurchase: hasOrder,
            autoApprove: false); // Requires admin approval

        await context.Set<ProductAnswer>().AddAsync(answer, cancellationToken);

        question.IncrementAnswerCount(isSellerAnswer);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        answer = await context.Set<ProductAnswer>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == answer.Id, cancellationToken);

        var hasUserVoted = await context.Set<AnswerHelpfulness>()
            .AsNoTracking()
            .AnyAsync(ah => ah.AnswerId == answer.Id && ah.UserId == userId, cancellationToken);

        return mapper.Map<ProductAnswerDto>(answer!) with { HasUserVoted = hasUserVoted };
    }

    public async Task<IEnumerable<ProductAnswerDto>> GetQuestionAnswersAsync(Guid questionId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var answersQuery = context.Set<ProductAnswer>()
            .AsNoTracking()
            .Where(a => a.QuestionId == questionId && a.IsApproved)
            .OrderByDescending(a => a.IsSellerAnswer)
            .ThenByDescending(a => a.HelpfulCount)
            .ThenByDescending(a => a.CreatedAt);

        var answers = await answersQuery
            .Include(a => a.User)
            .ToListAsync(cancellationToken);

        var answerIdsSubquery = from a in answersQuery select a.Id;
        var userVotes = userId.HasValue
            ? await context.Set<AnswerHelpfulness>()
                .AsNoTracking()
                .Where(ah => answerIdsSubquery.Contains(ah.AnswerId) && ah.UserId == userId.Value)
                .ToDictionaryAsync(ah => ah.AnswerId, cancellationToken)
            : new Dictionary<Guid, AnswerHelpfulness>();

        var dtos = new List<ProductAnswerDto>(answers.Count);
        foreach (var answer in answers)
        {
            var dto = mapper.Map<ProductAnswerDto>(answer) with { HasUserVoted = userId.HasValue && userVotes.ContainsKey(answer.Id) };
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<bool> ApproveAnswerAsync(Guid answerId, CancellationToken cancellationToken = default)
    {
        var answer = await context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer is null) return false;

        answer.Approve();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAnswerAsync(Guid answerId, CancellationToken cancellationToken = default)
    {
        var answer = await context.Set<ProductAnswer>()
            .Include(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer is null) return false;

        answer.MarkAsDeleted();

        // Update question stats
        if (answer.Question is not null)
        {
            // Check if there are other seller answers
            var hasOtherSellerAnswer = answer.IsSellerAnswer
                ? await context.Set<ProductAnswer>()
                    .AnyAsync(a => a.QuestionId == answer.QuestionId && a.IsSellerAnswer && a.Id != answerId, cancellationToken)
                : false;

            answer.Question.DecrementAnswerCount(answer.IsSellerAnswer);
            
            if (answer.IsSellerAnswer && !hasOtherSellerAnswer)
            {
                answer.Question.SetHasSellerAnswer(false);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task MarkQuestionHelpfulAsync(Guid userId, Guid questionId, CancellationToken cancellationToken = default)
    {
        var existing = await context.Set<QuestionHelpfulness>()
            .FirstOrDefaultAsync(qh => qh.QuestionId == questionId && qh.UserId == userId, cancellationToken);

        if (existing is not null) return; // Already marked

        var vote = QuestionHelpfulness.Create(questionId, userId);

        await context.Set<QuestionHelpfulness>().AddAsync(vote, cancellationToken);

        var question = await context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question is not null)
        {
            question.IncrementHelpfulCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnmarkQuestionHelpfulAsync(Guid userId, Guid questionId, CancellationToken cancellationToken = default)
    {
        var vote = await context.Set<QuestionHelpfulness>()
            .FirstOrDefaultAsync(qh => qh.QuestionId == questionId && qh.UserId == userId, cancellationToken);

        if (vote is null) return;

        vote.MarkAsDeleted();

        var question = await context.Set<ProductQuestion>()
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (question is not null)
        {
            question.DecrementHelpfulCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAnswerHelpfulAsync(Guid userId, Guid answerId, CancellationToken cancellationToken = default)
    {
        var existing = await context.Set<AnswerHelpfulness>()
            .FirstOrDefaultAsync(ah => ah.AnswerId == answerId && ah.UserId == userId, cancellationToken);

        if (existing is not null) return; // Already marked

        var vote = AnswerHelpfulness.Create(answerId, userId);

        await context.Set<AnswerHelpfulness>().AddAsync(vote, cancellationToken);

        var answer = await context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer is not null)
        {
            answer.IncrementHelpfulCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnmarkAnswerHelpfulAsync(Guid userId, Guid answerId, CancellationToken cancellationToken = default)
    {
        var vote = await context.Set<AnswerHelpfulness>()
            .FirstOrDefaultAsync(ah => ah.AnswerId == answerId && ah.UserId == userId, cancellationToken);

        if (vote is null) return;

        vote.MarkAsDeleted();

        var answer = await context.Set<ProductAnswer>()
            .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        if (answer is not null)
        {
            answer.DecrementHelpfulCount();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<QAStatsDto> GetQAStatsAsync(Guid? productId = null, CancellationToken cancellationToken = default)
    {
        var questionsQuery = context.Set<ProductQuestion>()
            .AsNoTracking()
            .Where(q => q.IsApproved);

        if (productId.HasValue)
        {
            questionsQuery = questionsQuery.Where(q => q.ProductId == productId.Value);
        }

        var totalQuestions = await questionsQuery.CountAsync(cancellationToken);
        var unansweredQuestions = await questionsQuery.CountAsync(q => q.AnswerCount == 0, cancellationToken);
        var questionsWithSellerAnswer = await questionsQuery.CountAsync(q => q.HasSellerAnswer, cancellationToken);

        var answersQuery = context.Set<ProductAnswer>()
            .AsNoTracking()
            .Where(a => a.IsApproved);

        if (productId.HasValue)
        {
            answersQuery = answersQuery.Where(a => a.Question.ProductId == productId.Value);
        }

        var totalAnswers = await answersQuery.CountAsync(cancellationToken);
        var averageAnswersPerQuestion = totalQuestions > 0 ? (decimal)totalAnswers / totalQuestions : 0;

        var recentQuestions = await context.Set<ProductQuestion>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.IsApproved)
            .OrderByDescending(q => q.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        var mostHelpfulQuestions = await context.Set<ProductQuestion>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(q => q.Product)
            .Include(q => q.User)
            .Include(q => q.Answers.Where(a => a.IsApproved))
                .ThenInclude(a => a.User)
            .Where(q => q.IsApproved)
            .OrderByDescending(q => q.HelpfulCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        var recentDtos = mapper.Map<IEnumerable<ProductQuestionDto>>(recentQuestions).ToList();
        var helpfulDtos = mapper.Map<IEnumerable<ProductQuestionDto>>(mostHelpfulQuestions).ToList();

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

    public async Task<IEnumerable<ProductQuestionDto>> GetUnansweredQuestionsAsync(Guid? productId = null, int limit = 20, CancellationToken cancellationToken = default)
    {
        var query = context.Set<ProductQuestion>()
            .AsNoTracking()
            .AsSplitQuery()
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

        return mapper.Map<IEnumerable<ProductQuestionDto>>(questions);
    }

}
