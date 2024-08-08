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
        public async Task<ServiceResponse<Output>> FindResult(FindResultRequestDto request)
        {
            var input = request.MatrixElements;
            var n = request.N;
            var m = request.M;
            var p = request.P;

            // Khởi tạo ma trận kho báu và các thông tin cần thiết
            var treasureMap = new int[n, m];
            foreach (var element in input)
            {
                treasureMap[element.Row, element.Column] = element.Value;
            }

            var startX = 0; // Chỉ số hàng bắt đầu (0-based index)
            var startY = 0; // Chỉ số cột bắt đầu (0-based index)
            var treasureX = -1; // Vị trí kho báu (0-based index)
            var treasureY = -1; // Vị trí kho báu (0-based index)

            // Tìm tọa độ của kho báu
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (treasureMap[i, j] == p)
                    {
                        treasureX = i;
                        treasureY = j;
                        break;
                    }
                }
                if (treasureX != -1) break;
            }

            // Nếu không tìm thấy kho báu, trả về giá trị mặc định hoặc thông báo lỗi
            if (treasureX == -1)
            {
                return new ServiceResponse<Output>
                {
                    Success = false,
                    Message = "Không tìm thấy kho báu trong map."
                };
            }

            // Khởi tạo bảng lưu trữ chi phí và queue
            var fuelCost = new double[n, m];
            var visited = new bool[n, m];
            var pq = new PriorityQueue<(int, int), double>();

            // Khởi tạo bảng fuelCost và đưa điểm bắt đầu vào queue
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    fuelCost[i, j] = double.MaxValue;
                }
            }
            fuelCost[startX, startY] = 0;
            pq.Enqueue((startX, startY), 0);

            // Các hướng di chuyển: lên, xuống, trái, phải
            var directions = new (int, int)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };

            while (pq.Count > 0)
            {
                var (x, y) = pq.Dequeue();

                if (visited[x, y]) continue;
                visited[x, y] = true;

                foreach (var (dx, dy) in directions)
                {
                    var nx = x + dx;
                    var ny = y + dy;

                    if (nx >= 0 && nx < n && ny >= 0 && ny < m)
                    {
                        var newCost = fuelCost[x, y] + Math.Sqrt(Math.Pow(x - nx, 2) + Math.Pow(y - ny, 2));

                        if (newCost < fuelCost[nx, ny])
                        {
                            fuelCost[nx, ny] = newCost;
                            pq.Enqueue((nx, ny), newCost);
                        }
                    }
                }
            }

            return new ServiceResponse<Output>
            {
                Data = new Output
                {
                    Result = fuelCost[treasureX, treasureY]
                }
            };
        }                   
    }
}
