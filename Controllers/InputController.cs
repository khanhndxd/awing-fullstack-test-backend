using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using awing_fullstack_test_backend.Repositories.InputRepo;

namespace awing_fullstack_test_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InputController : ControllerBase
    {
        private readonly IInputRepository _inputRepository;
        public InputController(IInputRepository inputRepository)
        {
            _inputRepository = inputRepository;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult<ServiceResponse<List<Input>>>> GetAll()
        {
            var result = await _inputRepository.GetAll();
            if (result.Success == false)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<Input>>> GetById(int id)
        {
            var result = await _inputRepository.GetInputById(id);
            if (result.Success == false)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        [HttpPost("FindResult")]
        public async Task<ActionResult<ServiceResponse<CreateOutputDto>>> FindResult([FromBody] FindResultRequestDto request)
        {
            var result = await _inputRepository.FindResult(request);

            if (result.Success == false)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
