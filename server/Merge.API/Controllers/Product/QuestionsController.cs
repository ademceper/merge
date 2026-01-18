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
using Merge.Application.Exceptions;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Product;

/// <summary>
/// Product Questions API endpoints.
/// Ürün soru-cevap işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/questions")]
[Tags("ProductQuestions")]
public class ProductQuestionsController(IMediator mediator) : BaseController
{
            [HttpPost]
    [Authorize]
    [RateLimit(5, 3600)]
    [ProducesResponseType(typeof(ProductQuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductQuestionDto>> AskQuestion(
        [FromBody] CreateProductQuestionDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new AskQuestionCommand(userId, dto.ProductId, dto.Question);
        var question = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductQuestionDto>> GetQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdOrNull();
        var query = new GetQuestionQuery(id, userId);
        var question = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("ProductQuestion", id);

        return Ok(question);
    }

    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ProductQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductQuestionDto>>> GetProductQuestions(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdOrNull();
        var query = new GetProductQuestionsQuery(productId, userId, page, pageSize);
        var questions = await mediator.Send(query, cancellationToken);
        return Ok(questions);
    }

    [HttpGet("my-questions")]
    [Authorize]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<ProductQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<ProductQuestionDto>>> GetMyQuestions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var query = new GetUserQuestionsQuery(userId, page, pageSize);
        var questions = await mediator.Send(query, cancellationToken);
        return Ok(questions);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApproveQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new ApproveQuestionCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("ProductQuestion", id);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteQuestion(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new DeleteQuestionCommand(id, userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("ProductQuestion", id);

        return NoContent();
    }

    [HttpPost("answers")]
    [Authorize]
    [RateLimit(5, 3600)]
    [ProducesResponseType(typeof(ProductAnswerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductAnswerDto>> AnswerQuestion(
        [FromBody] CreateProductAnswerDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new AnswerQuestionCommand(userId, dto.QuestionId, dto.Answer);
        var answer = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetQuestionAnswers), new { id = dto.QuestionId }, answer);
    }

    [HttpGet("{id}/answers")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductAnswerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductAnswerDto>>> GetQuestionAnswers(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserIdOrNull();
        var query = new GetQuestionAnswersQuery(id, userId);
        var answers = await mediator.Send(query, cancellationToken);
        return Ok(answers);
    }

    [HttpPost("answers/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ApproveAnswer(Guid id, CancellationToken cancellationToken = default)
    {
        var command = new ApproveAnswerCommand(id);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("ProductAnswer", id);

        return NoContent();
    }

    [HttpDelete("answers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteAnswer(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new DeleteAnswerCommand(id, userId);
        var success = await mediator.Send(command, cancellationToken);

        if (!success)
            throw new NotFoundException("ProductAnswer", id);

        return NoContent();
    }

    [HttpPost("{id}/helpful")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkQuestionHelpful(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new MarkQuestionHelpfulCommand(userId, id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}/helpful")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UnmarkQuestionHelpful(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new UnmarkQuestionHelpfulCommand(userId, id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("answers/{id}/helpful")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAnswerHelpful(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new MarkAnswerHelpfulCommand(userId, id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("answers/{id}/helpful")]
    [Authorize]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UnmarkAnswerHelpful(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var command = new UnmarkAnswerHelpfulCommand(userId, id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("stats")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(QAStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<QAStatsDto>> GetQAStats(
        [FromQuery] Guid? productId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetQAStatsQuery(productId);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    [HttpGet("unanswered")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductQuestionDto>>> GetUnansweredQuestions(
        [FromQuery] Guid? productId = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (limit > 100) limit = 100;
        if (limit < 1) limit = 20;
        var query = new GetUnansweredQuestionsQuery(productId, limit);
        var questions = await mediator.Send(query, cancellationToken);
        return Ok(questions);
    }
}
