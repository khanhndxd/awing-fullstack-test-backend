namespace awing_fullstack_test_backend.Repositories.InputRepo
{
    public class InputRepository : IInputRepository
    {
        private readonly DataContext _dataContext;

        public InputRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<ServiceResponse<List<GetInputDto>>> GetAll()
        {
            var serviceResponse = new ServiceResponse<List<GetInputDto>>();
            try
            {
                serviceResponse.Data = await _dataContext.Inputs
                    .Include(i => i.MatrixElements)
                    .Include(i => i.Output)
                    .Select(i => new GetInputDto
                    {
                        Id = i.Id,
                        Rows = i.Rows,
                        Columns = i.Columns,
                        Treasure = i.Treasure,
                        Output = i.Output != null ? new GetOutputDto
                        {
                            Id = i.Output.Id,
                            Result = i.Output.Result,
                            PathInfo = i.Output.PathInfo
                        } : null
                    })
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
                    serviceResponse.Message = "Không tìm thấy input.";
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
                serviceResponse.Message = $"Đã xảy ra lỗi khi lấy input: {ex.Message}";
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<CreateOutputDto>> FindResult(FindResultRequestDto request)
        {
            var serviceResponse = new ServiceResponse<CreateOutputDto>();

            try
            {
                // Tạo ma trận từ DTO
                int[,] grid = new int[request.N, request.M];
                foreach (var element in request.MatrixElements)
                {
                    grid[element.Row, element.Column] = element.Value;
                }

                // Tính toán kết quả bằng lớp TreasureHunt
                var (totalCost, allPaths) = TreasureHunt.MinCostToTreasure(request.N, request.M, request.P, grid);

                // Tạo đối tượng Input mới
                var input = new Input
                {
                    Rows = request.N,
                    Columns = request.M,
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
                    PathInfo = PathToList(allPaths)
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
        public ServiceResponse<GetOutputDto> FindResultWithoutSaving(FindResultRequestDto request)
        {
            var serviceResponse = new ServiceResponse<GetOutputDto>();

            try
            {
                // Tạo ma trận từ DTO
                int[,] grid = new int[request.N, request.M];
                foreach (var element in request.MatrixElements)
                {
                    grid[element.Row, element.Column] = element.Value;
                }

                // Tính toán kết quả bằng lớp TreasureHunt
                var (totalCost, allPaths) = TreasureHunt.MinCostToTreasure(request.N, request.M, request.P, grid);

                // Tạo đối tượng output DTO để trả về
                serviceResponse.Data = new GetOutputDto
                {
                    Result = totalCost,
                    PathInfo = PathToList(allPaths)
                };

                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Có lỗi xảy ra khi tính toán kết quả: {ex.Message}";
            }

            return serviceResponse;
        }
        public async Task<ServiceResponse<GetInputDto>> UpdateInput(int id, UpdateInputDto updatedInput)
        {
            var serviceResponse = new ServiceResponse<GetInputDto>();

            try
            {
                // Lấy input trong in-memory db theo id
                var existingInput = await _dataContext.Inputs
                    .Include(i => i.MatrixElements)
                    .Include(i => i.Output)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (existingInput == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Input not found.";
                    return serviceResponse;
                }

                // Chỉ cập nhật những trường thay đổi
                bool isUpdated = false;

                if (existingInput.Rows != updatedInput.Rows && updatedInput.Rows.HasValue)
                {
                    existingInput.Rows = updatedInput.Rows.Value;
                    isUpdated = true;
                }

                if (existingInput.Columns != updatedInput.Columns && updatedInput.Columns.HasValue)
                {
                    existingInput.Columns = updatedInput.Columns.Value;
                    isUpdated = true;
                }

                if (existingInput.Treasure != updatedInput.Treasure && updatedInput.Treasure.HasValue)
                {
                    existingInput.Treasure = updatedInput.Treasure.Value;
                    isUpdated = true;
                }

                // Xóa dữ liệu cũ
                if (existingInput.MatrixElements.Any())
                {
                    _dataContext.MatrixElements.RemoveRange(existingInput.MatrixElements);
                    existingInput.MatrixElements.Clear(); // Xóa danh sách các phần tử trong bộ nhớ
                }

                // Thêm dữ liệu mới
                if (updatedInput.MatrixElements != null)
                {
                    existingInput.MatrixElements = updatedInput.MatrixElements
                        .Select(me => new MatrixElement
                        {
                            Row = me.Row,
                            Column = me.Column,
                            Value = me.Value,
                            InputId = id // Đảm bảo ràng buộc với Input
                        }).ToList();
                    isUpdated = true;
                }

                // Tính toán lại kết quả Output
                if (isUpdated)
                {
                    // Tạo ma trận từ MatrixElements
                    int[,] grid = new int[existingInput.Rows, existingInput.Columns];
                    foreach (var element in existingInput.MatrixElements)
                    {
                        grid[element.Row, element.Column] = element.Value;
                    }

                    // Tính toán kết quả bằng lớp TreasureHunt
                    var (totalCost, allPaths) = TreasureHunt.MinCostToTreasure(
                        existingInput.Rows, existingInput.Columns, existingInput.Treasure, grid);

                    // Cập nhật hoặc tạo mới đối tượng Output
                    if (existingInput.Output == null)
                    {
                        existingInput.Output = new Output
                        {
                            InputId = existingInput.Id
                        };
                        _dataContext.Outputs.Add(existingInput.Output);
                    }

                    existingInput.Output.Result = totalCost;
                    existingInput.Output.PathInfo = PathToList(allPaths);
                }

                if (isUpdated)
                {
                    await _dataContext.SaveChangesAsync();
                }

                serviceResponse.Data = new GetInputDto
                {
                    Id = existingInput.Id,
                    Rows = existingInput.Rows,
                    Columns = existingInput.Columns,
                    Treasure = existingInput.Treasure,
                    MatrixElements = existingInput.MatrixElements.Select(me => new MatrixElementDto
                    {
                        Row = me.Row,
                        Column = me.Column,
                        Value = me.Value
                    }).ToList(),
                    Output = existingInput.Output != null ? new GetOutputDto
                    {
                        Id = existingInput.Output.Id,
                        Result = existingInput.Output.Result,
                        PathInfo = existingInput.Output.PathInfo
                    } : null
                };

                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Có lỗi xảy ra khi cập nhật input: {ex.Message}";
            }

            return serviceResponse;
        }
        public async Task<ServiceResponse<string>> DeleteInput(int id)
        {
            var serviceResponse = new ServiceResponse<string>();

            try
            {
                // Lấy input từ cơ sở dữ liệu theo id
                var input = await _dataContext.Inputs
                    .Include(i => i.MatrixElements)
                    .Include(i => i.Output)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (input == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Không tìm thấy input.";
                    return serviceResponse;
                }

                // Xóa các MatrixElements liên kết với input
                if (input.MatrixElements.Any())
                {
                    _dataContext.MatrixElements.RemoveRange(input.MatrixElements);
                }

                // Xóa Output liên kết với input nếu có
                if (input.Output != null)
                {
                    _dataContext.Outputs.Remove(input.Output);
                }

                // Xóa input
                _dataContext.Inputs.Remove(input);
                await _dataContext.SaveChangesAsync();

                serviceResponse.Data = "Input đã được xóa thành công.";
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Có lỗi xảy ra khi xóa input: {ex.Message}";
            }

            return serviceResponse;
        }
        private string PathToList(IEnumerable<(List<(int x, int y)> path, double cost)> allPaths)
        {
            var pathInfoList = new List<string>();
            bool isFirstPath = true;

            foreach (var path in allPaths)
            {
                var pathCoords = path.path
                    .Select(coords => $"({coords.x}, {coords.y})")
                    .ToList();

                if (isFirstPath)
                {
                    pathInfoList.AddRange(pathCoords);
                    isFirstPath = false;
                }
                else
                {
                    // Bỏ qua tọa độ đầu tiên của các path sau path đầu tiên
                    if (pathCoords.Count > 1)
                    {
                        pathInfoList.AddRange(pathCoords.Skip(1));
                    }
                }
            }

            return string.Join(" -> ", pathInfoList);
        }
    }
}
