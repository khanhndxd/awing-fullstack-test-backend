using System;
using System.Collections.Generic;
using System.Linq;

public class TreasureHunt
{
    // Các vector hướng di chuyển trong 8 hướng (bao gồm các đường chéo)
    private static readonly (int dx, int dy)[] Directions =
    {
        (-1, 0), (1, 0), (0, -1), (0, 1),
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    };

    // Phương thức chính để tính toán chi phí tối thiểu và đường đi
    public static (double totalCost, List<(List<(int x, int y)> path, double cost)> allPaths)
    MinCostToTreasure(int n, int m, int p, int[,] grid)
    {
        // Tìm vị trí của từng kho báu trong ma trận
        var positions = new Dictionary<int, (int x, int y)>();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                // Ghi nhận vị trí của từng kho báu
                if (!positions.ContainsKey(grid[i, j]))
                {
                    positions[grid[i, j]] = (i, j);
                }
            }
        }

        // Hàm thực hiện thuật toán Dijkstra để tìm chi phí và đường đi tối ưu
        (Dictionary<(int x, int y), double> minCost, Dictionary<(int x, int y), List<(int x, int y)>> minPath)
        Dijkstra((int x, int y) start)
        {
            // Priority queue (hàng đợi ưu tiên) cho thuật toán Dijkstra
            var heap = new SortedSet<(double cost, (int x, int y) pos)>(Comparer<(double cost, (int x, int y) pos)>.Create(
                (a, b) => a.cost == b.cost ? a.pos.CompareTo(b.pos) : a.cost.CompareTo(b.cost)));
            heap.Add((0, start));

            var visited = new HashSet<(int x, int y)>();
            var minCost = new Dictionary<(int x, int y), double> { [start] = 0 };
            var minPath = new Dictionary<(int x, int y), List<(int x, int y)>> { [start] = new List<(int x, int y)>() };

            while (heap.Count > 0)
            {
                var (cost, (x, y)) = heap.Min;
                heap.Remove(heap.Min);

                if (visited.Contains((x, y)))
                    continue;

                visited.Add((x, y));

                foreach (var (dx, dy) in Directions)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < n && ny >= 0 && ny < m)
                    {
                        // Tính chi phí di chuyển đến điểm mới
                        double newCost = Math.Sqrt((nx - x) * (nx - x) + (ny - y) * (ny - y));
                        double newTotalCost = cost + newCost;
                        var newPath = new List<(int x, int y)>(minPath[(x, y)]) { (x, y) };
                        var newPos = (nx, ny);

                        // Cập nhật chi phí và đường đi nếu điểm mới có chi phí thấp hơn
                        if (!visited.Contains(newPos) &&
                            (!minCost.ContainsKey(newPos) || newTotalCost < minCost[newPos]))
                        {
                            minCost[newPos] = newTotalCost;
                            minPath[newPos] = newPath;
                            heap.Add((newTotalCost, newPos));
                        }
                    }
                }
            }

            return (minCost, minPath);
        }

        double totalCost = 0;
        var allPaths = new List<(List<(int x, int y)> path, double cost)>();

        // Tính toán chi phí từ từng kho báu số i đến kho báu số i+1
        for (int i = 1; i < p; i++)
        {
            if (positions.ContainsKey(i) && positions.ContainsKey(i + 1))
            {
                var start = positions[i];
                var end = positions[i + 1];
                var (minCostFromStart, minPathFromStart) = Dijkstra(start);

                if (minCostFromStart.ContainsKey(end))
                {
                    double cost = minCostFromStart[end];
                    var path = new List<(int x, int y)>(minPathFromStart[end]) { end };
                    totalCost += cost;
                    allPaths.Add((path, cost));
                }
            }
        }

        return (totalCost, allPaths);
    }
}
