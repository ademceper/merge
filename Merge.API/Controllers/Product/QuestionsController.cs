using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;


namespace Merge.API.Controllers.Product;

[ApiController]
[Route("api/products/questions")]
public class ProductQuestionsController : BaseController
{
    private readonly IProductQuestionService _productQuestionService;

    public ProductQuestionsController(IProductQuestionService productQuestionService)
    {
        _productQuestionService = productQuestionService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductQuestionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductQuestionDto>> AskQuestion([FromBody] CreateProductQuestionDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var question = await _productQuestionService.AskQuestionAsync(userId, dto);
        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductQuestionDto>> GetQuestion(Guid id)
    {
        var userId = GetUserIdOrNull();

        var question = await _productQuestionService.GetQuestionAsync(id, userId);

        if (question == null)
        {
            return NotFound();
        }

        return Ok(question);
    }

    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(IEnumerable<ProductQuestionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductQuestionDto>>> GetProductQuestions(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserIdOrNull();

        var questions = await _productQuestionService.GetProductQuestionsAsync(productId, userId, page, pageSize);
        return Ok(questions);
    }

    [HttpGet("my-questions")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ProductQuestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ProductQuestionDto>>> GetMyQuestions()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var questions = await _productQuestionService.GetUserQuestionsAsync(userId);
        return Ok(questions);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApproveQuestion(Guid id)
    {
        var success = await _productQuestionService.ApproveQuestionAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        var success = await _productQuestionService.DeleteQuestionAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("answers")]
    [Authorize]
    [ProducesResponseType(typeof(ProductAnswerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductAnswerDto>> AnswerQuestion([FromBody] CreateProductAnswerDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var answer = await _productQuestionService.AnswerQuestionAsync(userId, dto);
        return CreatedAtAction(nameof(GetQuestionAnswers), new { id = dto.QuestionId }, answer);
    }

    [HttpGet("{id}/answers")]
    [ProducesResponseType(typeof(IEnumerable<ProductAnswerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductAnswerDto>>> GetQuestionAnswers(Guid id)
    {
        var userId = GetUserIdOrNull();

        var answers = await _productQuestionService.GetQuestionAnswersAsync(id, userId);
        return Ok(answers);
    }

    [HttpPost("answers/{id}/approve")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ApproveAnswer(Guid id)
    {
        var success = await _productQuestionService.ApproveAnswerAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("answers/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAnswer(Guid id)
    {
        var success = await _productQuestionService.DeleteAnswerAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id}/helpful")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkQuestionHelpful(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        await _productQuestionService.MarkQuestionHelpfulAsync(userId, id);
        return NoContent();
    }

    [HttpDelete("{id}/helpful")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnmarkQuestionHelpful(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        await _productQuestionService.UnmarkQuestionHelpfulAsync(userId, id);
        return NoContent();
    }

    [HttpPost("answers/{id}/helpful")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAnswerHelpful(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        await _productQuestionService.MarkAnswerHelpfulAsync(userId, id);
        return NoContent();
    }

    [HttpDelete("answers/{id}/helpful")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnmarkAnswerHelpful(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        await _productQuestionService.UnmarkAnswerHelpfulAsync(userId, id);
        return NoContent();
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(QAStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<QAStatsDto>> GetQAStats([FromQuery] Guid? productId = null)
    {
        var stats = await _productQuestionService.GetQAStatsAsync(productId);
        return Ok(stats);
    }

    [HttpGet("unanswered")]
    [ProducesResponseType(typeof(IEnumerable<ProductQuestionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductQuestionDto>>> GetUnansweredQuestions(
        [FromQuery] Guid? productId = null,
        [FromQuery] int limit = 20)
    {
        var questions = await _productQuestionService.GetUnansweredQuestionsAsync(productId, limit);
        return Ok(questions);
    }
}
