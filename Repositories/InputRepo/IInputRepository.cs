namespace awing_fullstack_test_backend.Repositories.InputRepo
{
    public interface IInputRepository
    {
        Task<ServiceResponse<List<Input>>> GetAll();
        Task<ServiceResponse<GetInputDto>> GetInputById(int id);
        Task<ServiceResponse<CreateOutputDto>> FindResult(FindResultRequestDto request);
    }
}
