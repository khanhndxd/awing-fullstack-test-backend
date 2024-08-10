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
        public async Task<ServiceResponse<GetInputDto>> GetInputById(int id)
        {
            var serviceResponse = new ServiceResponse<GetInputDto>();
            try
            {
                var input = await _dataContext.Inputs
                    .Include(i => i.MatrixElements)
                    .Include(i => i.Output)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (input == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Input not found.";
                }
                else
                {
                    // Map Input to InputDto
                    var inputDto = new GetInputDto
                    {
                        Id = input.Id,
                        Rows = input.Rows,
                        Columns = input.Columns,
                        Treasure = input.Treasure,
                        MatrixElements = input.MatrixElements.Select(me => new MatrixElementDto
                        {
                            Row = me.Row,
                            Column = me.Column,
                            Value = me.Value
                        }).ToList(),
                        Output = input.Output != null ? new GetOutputDto
                        {
                            Id = input.Output.Id,
                            Result = input.Output.Result,
                            PathInfo = input.Output.PathInfo
                        } : null
                    };

                    serviceResponse.Data = inputDto;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Error retrieving input: {ex.Message}";
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<CreateOutputDto>> FindResult(FindResultRequestDto request)
        {
            var serviceResponse = new ServiceResponse<CreateOutputDto>();

            try
            {
                // Tạo ma trận từ DTO
                int[,] grid = new int[request.M, request.N];
                foreach (var element in request.MatrixElements)
                {
                    grid[element.Row, element.Column] = element.Value;
                }

                // Tính toán kết quả bằng lớp TreasureHunt
                var (totalCost, allPaths) = TreasureHunt.MinCostToTreasure(request.M, request.N, request.P, grid);

                // Tạo đối tượng Input mới
                var input = new Input
                {
                    Rows = request.M,
                    Columns = request.N,
                    Treasure = request.P,
                    MatrixElements = request.MatrixElements.Select(e => new MatrixElement
                    {
                        Row = e.Row,
                        Column = e.Column,
                        Value = e.Value
                    }).ToList()
                };

                // Lưu Input vào cơ sở dữ liệu
                _dataContext.Inputs.Add(input);
                await _dataContext.SaveChangesAsync();

                // Tạo đối tượng Output mới
                var output = new Output
                {
                    InputId = input.Id,
                    Result = totalCost,
                    PathInfo = string.Join(" -> ", allPaths.SelectMany(p => p.path).Select(p => $"({p.x}, {p.y})")) // Chuyển đổi đường đi thành chuỗi
                };

                // Lưu Output vào cơ sở dữ liệu
                _dataContext.Outputs.Add(output);
                await _dataContext.SaveChangesAsync();

                // Trả về kết quả
                serviceResponse.Data = new CreateOutputDto
                {
                    Result = output.Result,
                    InputId = output.InputId
                };
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Có lỗi xảy ra khi tính toán kết quả: {ex.Message}";
            }

            return serviceResponse;
        }
    }
}
