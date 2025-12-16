using System.Security.Claims;
using DemoUser.BLL.Services.Interfaces;
using DemoUser.WebAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private readonly ITodoService _service;
    public TodosController(ITodoService service) => _service = service;

    private Guid GetUserId()
    {
        var raw =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var userId))
            throw new UnauthorizedAccessException("UserId claim missing or invalid in JWT.");

        return userId;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var userId = GetUserId();
        return Ok(_service.GetAll(userId));
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var userId = GetUserId();
        var todo = _service.GetById(userId, id);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    public IActionResult Create([FromBody] TodoCreateDto dto)
    {
        var userId = GetUserId();
        var created = _service.Create(userId, dto.Title);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] TodoUpdateDto dto)
    {
        var userId = GetUserId();

        var renamed = _service.Rename(userId, id, dto.Title);
        if (!renamed) return NotFound();

        if (dto.IsDone)
        {
            var done = _service.MarkAsDone(userId, id);
            if (!done) return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = _service.Delete(userId, id);
        return deleted ? NoContent() : NotFound();
    }
}
