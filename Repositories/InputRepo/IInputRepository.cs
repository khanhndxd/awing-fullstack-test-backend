namespace awing_fullstack_test_backend.Repositories.InputRepo
{
    public interface IInputRepository
    {
        Task<ServiceResponse<List<GetInputDto>>> GetAll();
        Task<ServiceResponse<GetInputDto>> GetInputById(int id);
        Task<ServiceResponse<CreateOutputDto>> FindResult(FindResultRequestDto request);
        ServiceResponse<GetOutputDto> FindResultWithoutSaving(FindResultRequestDto request);
        Task<ServiceResponse<GetInputDto>> UpdateInput(int id, UpdateInputDto updatedInput);
        Task<ServiceResponse<string>> DeleteInput(int id);
    }
}
