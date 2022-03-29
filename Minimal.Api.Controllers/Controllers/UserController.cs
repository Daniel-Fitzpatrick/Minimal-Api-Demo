using Microsoft.AspNetCore.Mvc;
using Minimal.Api.Common.Model;
using Minimal.Api.Common.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Minimal.Api.Controllers.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet("{id:guid}", Name = "GetById")]
        [SwaggerOperation("Gets User by Id")]
        [Produces(typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] Guid id, [FromServices] IUserService userService)
        {
            var user = await userService.GetByIdAsync(id);

            return user is not null ? Ok(user) : NotFound();
        }

        [HttpGet]
        [SwaggerOperation("Gets all users or ones matching a skill")]
        [Produces(typeof(IEnumerable<User>))]
        public async Task<IActionResult> Get([FromQuery] string? skill, [FromServices] IUserService userService)
        {
            return skill is null ? Ok(await userService.GetAllAsync()) : Ok(await userService.SearchBySkill(skill));
        }

        [HttpPost]
        [SwaggerOperation("Creates a new User")]
        [Produces(typeof(User))]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType( StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] User user, [FromServices] IUserService userService)
        {
            if (await userService.GetByEmailAsync(user.Email) is not null)
            {
                return BadRequest(new Dictionary<string, string[]> { { nameof(Common.Model.User.Email), new[] { "User with email address already exists" } } });
            }

            if (await userService.CreateAsync(user))
            {
                return Created($"User/{user.Id}", user);
            }

            return Problem("User could not be created");
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation("Updates a User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] User user, [FromServices] IUserService userService)
        {
            if (await userService.GetByIdAsync(id) is null)
            {
                return NotFound();
            }

            if ((await userService.GetByEmailAsync(user.Email))?.Id != id.ToString())
            {
                return BadRequest(new Dictionary<string, string[]> { { nameof(Common.Model.User.Email), new[] { "User with email address already exists" } } });
            }

            if (await userService.UpdateAsync(id, user))
            {
                return Ok();
            }

            return Problem("User could not be updated");
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation("Deletes a User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromServices] IUserService userService)
        {
            if (await userService.GetByIdAsync(id) is null)
            {
                return NotFound();
            }

            if (await userService.DeleteAsync(id))
            {
                return Ok();
            }

            return Problem("User could not be deleted");
        }
    }

}