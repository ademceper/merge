using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Application.Product.Commands.AskQuestion;
using Merge.Application.Product.Commands.AnswerQuestion;
using Merge.Application.Product.Commands.ApproveQuestion;
using Merge.Application.Product.Commands.DeleteQuestion;
using Merge.Application.Product.Commands.ApproveAnswer;
using Merge.Application.Product.Commands.DeleteAnswer;
using Merge.Application.Product.Commands.MarkQuestionHelpful;
using Merge.Application.Product.Commands.UnmarkQuestionHelpful;
using Merge.Application.Product.Commands.MarkAnswerHelpful;
using Merge.Application.Product.Commands.UnmarkAnswerHelpful;
using Merge.Application.Product.Queries.GetQuestion;
using Merge.Application.Product.Queries.GetProductQuestions;
using Merge.Application.Product.Queries.GetUserQuestions;
using Merge.Application.Product.Queries.GetQuestionAnswers;
using Merge.Application.Product.Queries.GetQAStats;
using Merge.Application.Product.Queries.GetUnansweredQuestions;
using Merge.API.Middleware;
using Merge.API.Helpers;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
namespace Merge.API.Controllers.Product;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/questions")]
public class ProductQuestionsController : BaseController
{
    private readonly IMediator _mediator;

    public ProductQuestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [Authorize]
    [RateLimit(5, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 5/saat (Spam koruması)
    [ProducesResponseType(typeof(ProductQuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductQuestionDto>> AskQuestion(
        [FromBody] CreateProductQuestionDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new AskQuestionCommand(userId, dto.ProductId, dto.Question);
        var question = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductQuestionLinks(Url, question.Id, dto.ProductId, version);
        
        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, new { question, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ProductQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductQuestionDto>> GetQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserIdOrNull();
        var query = new GetQuestionQuery(id, userId);
        var question = await _mediator.Send(query, cancellationToken);

        if (question == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductQuestionLinks(Url, question.Id, question.ProductId, version);
        
        return Ok(new { question, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductQuestionDto>>> GetProductQuestions(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserIdOrNull();
        var query = new GetProductQuestionsQuery(productId, userId, page, pageSize);
        var questions = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetProductQuestions",
            page,
            pageSize,
            questions.TotalPages,
            new { productId },
            version);
        
        return Ok(new { questions, _links = paginationLinks });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    [HttpGet("my-questions")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<ProductQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductQuestionDto>>> GetMyQuestions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUserQuestionsQuery(userId, page, pageSize);
        var questions = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var paginationLinks = HateoasHelper.CreatePaginationLinks(
            Url,
            "GetMyQuestions",
            page,
            pageSize,
            questions.TotalPages,
            null,
            version);
        
        return Ok(new { questions, _links = paginationLinks });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApproveQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ApproveQuestionCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (DeleteQuestionCommandHandler)
        var command = new DeleteQuestionCommand(id, userId);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("answers")]
    [Authorize]
    [RateLimit(5, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 5/saat (Spam koruması)
    [ProducesResponseType(typeof(ProductAnswerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductAnswerDto>> AnswerQuestion(
        [FromBody] CreateProductAnswerDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new AnswerQuestionCommand(userId, dto.QuestionId, dto.Answer);
        var answer = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQuestionAnswers), new { id = dto.QuestionId }, answer);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}/answers")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductAnswerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductAnswerDto>>> GetQuestionAnswers(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserIdOrNull();
        var query = new GetQuestionAnswersQuery(id, userId);
        var answers = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateProductAnswerLinks(Url, id, version);
        
        return Ok(new { answers, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("answers/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApproveAnswer(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new ApproveAnswerCommand(id);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("answers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteAnswer(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (DeleteAnswerCommandHandler)
        var command = new DeleteAnswerCommand(id, userId);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/helpful")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkQuestionHelpful(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new MarkQuestionHelpfulCommand(userId, id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("{id}/helpful")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UnmarkQuestionHelpful(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UnmarkQuestionHelpfulCommand(userId, id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("answers/{id}/helpful")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAnswerHelpful(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new MarkAnswerHelpfulCommand(userId, id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("answers/{id}/helpful")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UnmarkAnswerHelpful(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UnmarkAnswerHelpfulCommand(userId, id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("stats")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(QAStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<QAStatsDto>> GetQAStats(
        [FromQuery] Guid? productId = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetQAStatsQuery(productId);
        var stats = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetQAStats", new { productId }, version);
        
        return Ok(new { stats, _links = links });
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("unanswered")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductQuestionDto>>> GetUnansweredQuestions(
        [FromQuery] Guid? productId = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (limit > 100) limit = 100;
        if (limit < 1) limit = 20;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetUnansweredQuestionsQuery(productId, limit);
        var questions = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = HateoasHelper.CreateSelfLink(Url, "GetUnansweredQuestions", new { productId, limit }, version);
        
        return Ok(new { questions, _links = links });
    }
}
