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

        public async Task<ServiceResponse<CreateOutputDto>> FindResult(FindResultRequestDto request)
        {
            var serviceResponse = new ServiceResponse<CreateOutputDto>();

            try
            {
                // Tạo bản đồ kho báu từ request
                var treasureMap = new TreasureMap(request.N, request.M, request.P, request.MatrixElements);
                double minimumFuel = FindMinimumFuel(treasureMap);

                // Tạo đối tượng Input và lưu vào cơ sở dữ liệu
                var input = new Input
                {
                    Rows = request.N,
                    Columns = request.M,
                    Treasure = request.P,
                    MatrixElements = request.MatrixElements.Select(me => new MatrixElement
                    {
                        Row = me.Row,
                        Column = me.Column,
                        Value = me.Value
                    }).ToList()
                };

                _dataContext.Inputs.Add(input);
                await _dataContext.SaveChangesAsync();

                // Tạo đối tượng Output để lưu kết quả
                var output = new Output
                {
                    Result = minimumFuel,
                    InputId = input.Id
                };

                _dataContext.Outputs.Add(output);
                await _dataContext.SaveChangesAsync();

                // Chuyển đổi Output thành OutputDto
                var outputDto = new CreateOutputDto
                {
                    Result = output.Result,
                    InputId = output.InputId
                };

                serviceResponse.Data = outputDto;
                serviceResponse.Message = "Success";
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = $"Có lỗi xảy ra khi tính toán kết quả: {ex.Message}";
            }

            return serviceResponse;
        }

        private double FindMinimumFuel(TreasureMap treasureMap)
        {
            var start = treasureMap.GetChest(1);
            var target = treasureMap.GetChest(treasureMap.MaxChestNumber);

            if (start == null || target == null)
                throw new InvalidOperationException("Start or target chest not found.");

            var allPaths = FindAllPathsDFS(start, target, treasureMap);

            double minFuel = double.MaxValue;

            foreach (var path in allPaths)
            {
                double fuel = CalculateFuel(path);
                Console.WriteLine($"Path with fuel: {fuel}");
                if (fuel < minFuel)
                {
                    minFuel = fuel;
                }
            }

            if (minFuel == double.MaxValue)
            {
                Console.WriteLine("No valid path found.");
            }
            else
            {
                Console.WriteLine($"Minimum fuel required: {minFuel}");
            }

            return minFuel == double.MaxValue ? 0 : minFuel * 2; // Nhân đôi vì cần tính cả đường về
        }

        private List<List<Island>> FindAllPathsDFS(Island start, Island target, TreasureMap treasureMap)
        {
            var allPaths = new List<List<Island>>();
            var path = new List<Island>();
            var visited = new HashSet<Island>();

            void DFS(Island current, List<Island> currentPath, HashSet<Island> visitedSet)
            {
                if (current == null) return;

                currentPath.Add(current);
                visitedSet.Add(current);

                if (current == target)
                {
                    allPaths.Add(new List<Island>(currentPath));
                }
                else
                {
                    foreach (var neighbor in GetNeighbors(current, treasureMap))
                    {
                        if (!visitedSet.Contains(neighbor) && IsValidMove(current, neighbor))
                        {
                            DFS(neighbor, currentPath, visitedSet);
                        }
                    }
                }

                // Backtrack
                currentPath.RemoveAt(currentPath.Count - 1);
                visitedSet.Remove(current);
            }

            DFS(start, path, visited);

            return allPaths;
        }

        private IEnumerable<Island> GetNeighbors(Island island, TreasureMap treasureMap)
        {
            var directions = new (int dx, int dy)[]
            {
                       (-1, 0), (1, 0), (0, -1), (0, 1)
            };

            foreach (var (dx, dy) in directions)
            {
                int newX = island.X + dx;
                int newY = island.Y + dy;

                if (newX >= 0 && newX < treasureMap.Rows && newY >= 0 && newY < treasureMap.Columns)
                {
                    yield return treasureMap.Map[newX, newY];
                }
            }
        }

        private bool IsValidMove(Island current, Island neighbor)
        {
            return neighbor.ChestNumber == current.ChestNumber + 1 || neighbor.ChestNumber == current.ChestNumber - 1;
        }

        private double CalculateFuel(List<Island> path)
        {
            double totalFuel = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];
                totalFuel += Math.Sqrt(Math.Pow(current.X - next.X, 2) + Math.Pow(current.Y - next.Y, 2));
            }

            return totalFuel;
        }
    }

    public class TreasureMap
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int MaxChestNumber { get; set; }
        public Island[,] Map { get; set; }

        public TreasureMap(int rows, int columns, int maxChestNumber, List<MatrixElementDto> matrixElements)
        {
            Rows = rows;
            Columns = columns;
            MaxChestNumber = maxChestNumber;
            Map = new Island[rows, columns];

            foreach (var element in matrixElements)
            {
                Map[element.Row, element.Column] = new Island { X = element.Row, Y = element.Column, ChestNumber = element.Value };
            }
        }

        public Island GetChest(int chestNumber)
        {
            foreach (var island in Map)
            {
                if (island.ChestNumber == chestNumber)
                {
                    return island;
                }
            }
            return null;
        }
    }

    public class Island
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int ChestNumber { get; set; }
    }
}
