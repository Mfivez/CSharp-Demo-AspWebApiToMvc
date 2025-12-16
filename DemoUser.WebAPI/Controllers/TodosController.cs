using DemoUser.BLL.Services.Interfaces;
using DemoUser.WebAPI.Dtos;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private readonly ITodoService _service;
    public TodosController(ITodoService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll() => Ok(_service.GetAll());

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var todo = _service.GetById(id);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    public IActionResult Create([FromBody] TodoCreateDto dto)
    {
        var created = _service.Create(dto.Title);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] TodoUpdateDto dto)
    {
        _service.Rename(id, dto.Title);
        if (dto.IsDone) _service.MarkAsDone(id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _service.Delete(id);
        return NoContent();
    }
}
