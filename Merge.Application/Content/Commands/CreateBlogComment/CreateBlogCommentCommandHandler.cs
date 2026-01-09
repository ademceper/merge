using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.CreateBlogComment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateBlogCommentCommandHandler : IRequestHandler<CreateBlogCommentCommand, BlogCommentDto>
{
    private readonly IRepository<BlogComment> _commentRepository;
    private readonly IRepository<BlogPost> _postRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBlogCommentCommandHandler> _logger;
    private const string CACHE_KEY_POST_COMMENTS = "blog_post_comments_";

    public CreateBlogCommentCommandHandler(
        IRepository<BlogComment> commentRepository,
        IRepository<BlogPost> postRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateBlogCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BlogCommentDto> Handle(CreateBlogCommentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating blog comment. BlogPostId: {BlogPostId}, UserId: {UserId}",
            request.BlogPostId, request.UserId);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Post validation
            var post = await _context.Set<BlogPost>()
                .FirstOrDefaultAsync(p => p.Id == request.BlogPostId && p.AllowComments, cancellationToken);

            if (post == null)
            {
                _logger.LogWarning("Blog post not found or comments not allowed. BlogPostId: {BlogPostId}", request.BlogPostId);
                throw new NotFoundException("Blog yazısı", request.BlogPostId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var autoApprove = request.UserId.HasValue; // Auto-approve for logged-in users
            var comment = BlogComment.Create(
                request.BlogPostId,
                request.Content,
                request.UserId,
                request.ParentCommentId,
                request.AuthorName,
                request.AuthorEmail,
                autoApprove);

            comment = await _commentRepository.AddAsync(comment, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            post.IncrementCommentCount();

            await _postRepository.UpdateAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedComment = await _context.Set<BlogComment>()
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.ParentComment)
                .FirstOrDefaultAsync(c => c.Id == comment.Id, cancellationToken);

            if (reloadedComment == null)
            {
                _logger.LogWarning("Blog comment {CommentId} not found after creation", comment.Id);
                throw new NotFoundException("Blog Yorumu", comment.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_POST_COMMENTS}{request.BlogPostId}", cancellationToken);

            _logger.LogInformation("Blog comment created. CommentId: {CommentId}, BlogPostId: {BlogPostId}",
                comment.Id, request.BlogPostId);

            return _mapper.Map<BlogCommentDto>(reloadedComment);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while creating blog comment for BlogPostId: {BlogPostId}", request.BlogPostId);
            throw new BusinessException("Blog yorumu oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating blog comment for BlogPostId: {BlogPostId}", request.BlogPostId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

