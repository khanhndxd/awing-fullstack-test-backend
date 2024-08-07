namespace awing_fullstack_test_backend.Repositories.InputRepo
{
    public class InputRepository : IInputRepository
    {
        private readonly DataContext _dataContext;
        public InputRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<ServiceResponse<List<Input>>> GetAll()
        {
            var serviceResponse = new ServiceResponse<List<Input>>();
            try
            {
                serviceResponse.Data = await _dataContext.Inputs
                    .Include(i => i.MatrixElements)
                    .Include(i => i.Output)
                    .ToListAsync();

                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Có lỗi xảy ra khi lấy danh sách input: {ex.Message}";
            }

            return serviceResponse;
        }
        public async Task<ServiceResponse<Output>> FindResult()
        {
            var serviceResponse = new ServiceResponse<Output>();
            return serviceResponse;
        }
    }
}
