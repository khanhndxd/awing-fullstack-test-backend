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
        public async Task<ActionResult<ServiceResponse<Output>>> FindResult()
        {
            var result = await _inputRepository.FindResult();
            return Ok(result);
        }
    }
}
