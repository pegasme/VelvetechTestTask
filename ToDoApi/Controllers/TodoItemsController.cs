using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoApi.Services.Extensions;
using TodoApi.Services.Models;
using TodoApi.Services.Services.Interfaces;

namespace TodoApiDTO.Controllers
{
    /// <summary>
    /// All work with TodoUtems.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemService _service;

        public TodoItemsController(ITodoItemService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets all items from database
        /// </summary>
        /// <returns>List of TodoItems</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            var items = await _service.GetAsync();
            return Ok(items);
        }

        /// <summary>
        /// Get TodoItem with specific id.
        /// </summary>
        /// <param name="id">Id of TodoItem. Required</param>
        /// <returns>TodoItem</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
        {
            Argument.Id(id);

            var todoItem = await _service.GetAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return Ok(todoItem);
        }

        /// <summary>
        /// Update existed TodoItem.
        /// </summary>
        /// <param name="id">Id of TodoItem for update</param>
        /// <param name="todoItemDTO">New TodoItem values</param>
        /// <returns>ActionResult (Ok or Failure)</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(long id, TodoItemDTO todoItemDTO)
        {
            Argument.Id(id);
            Argument.NotNull(todoItemDTO);

            if (!ModelState.IsValid)
                return BadRequest();

            await _service.UpdateAsync(id, todoItemDTO);
            return Ok();
        }

        /// <summary>
        /// Create new TodoItem. Name is required. 
        /// </summary>
        /// <param name="todoItemDTO">New TodoItem</param>
        /// <returns>New TodoItem</returns>
        [HttpPost]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem([FromBody] TodoItemDTO todoItemDTO)
        {
            Argument.NotNull(todoItemDTO);

            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _service.CreateAsync(todoItemDTO);

            return CreatedAtAction(
                nameof(GetTodoItem),
                new { id = result.Id },
                result);
        }

        /// <summary>
        /// Delete existed TodoItem from database.
        /// </summary>
        /// <param name="id">Id of existed TodoItem</param>
        /// <returns>ActionResult (Ok or Failure)</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            Argument.Id(id);

            await _service.DeleteAsync(id);

            //Probably should be Ok()
            return NoContent();
        }
    }
}
