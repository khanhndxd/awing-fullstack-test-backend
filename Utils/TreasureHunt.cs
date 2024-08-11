using System;
using System.Collections.Generic;
using System.Linq;

public class TreasureHunt
{
    public static (double, List<(List<(int, int)>, double)>) MinCostToTreasure(int n, int m, int p, int[,] grid)
    {
        // Hướng đi của ma trận (8 hướng)
        var directions = new (int, int)[]
        {
            (-1, 0), (1, 0), (0, -1), (0, 1),
            (-1, -1), (-1, 1), (1, -1), (1, 1)
        };

        // Tìm vị trí của từng key và khó báu
        var positions = new Dictionary<int, (int, int)>();
        for (int i = 1; i <= p; i++)
        {
            positions[i] = (-1, -1);
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (positions.ContainsKey(grid[i, j]))
                {
                    positions[grid[i, j]] = (i, j);
                }
            }
        }

        (Dictionary<(int, int), double>, Dictionary<(int, int), List<(int, int)>>) Dijkstra((int, int) start)
        {
            // Priority queue cho thuật toán Dijkstra
            var heap = new SortedSet<(double, (int, int))>(Comparer<(double, (int, int))>.Create((a, b) => a.Item1 == b.Item1 ? a.Item2.CompareTo(b.Item2) : a.Item1.CompareTo(b.Item1)));
            heap.Add((0, start));
            var visited = new HashSet<(int, int)>();
            var minCost = new Dictionary<(int, int), double> { [start] = 0 };
            var minPath = new Dictionary<(int, int), List<(int, int)>> { [start] = new List<(int, int)>() };

            while (heap.Count > 0)
            {
                var (cost, (x, y)) = heap.Min;
                heap.Remove(heap.Min);

                if (visited.Contains((x, y)))
                {
                    continue;
                }

                visited.Add((x, y));
                var path = minPath[(x, y)];

                foreach (var (dx, dy) in directions)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < n && ny >= 0 && ny < m)
                    {
                        double newCost = Math.Sqrt((nx - x) * (nx - x) + (ny - y) * (ny - y)); // Công thức tính nhiên liệu theo đề bài
                        double newTotalCost = cost + newCost;
                        var newPath = new List<(int, int)>(path) { (x, y) };
                        if (!visited.Contains((nx, ny)) && (!minCost.ContainsKey((nx, ny)) || newTotalCost < minCost[(nx, ny)]))
                        {
                            minCost[(nx, ny)] = newTotalCost;
                            minPath[(nx, ny)] = newPath;
                            heap.Add((newTotalCost, (nx, ny)));
                        }
                    }
                }
            }

            return (minCost, minPath);
        }

        double totalCost = 0;
        var allPaths = new List<(List<(int, int)>, double)>();

        // Tính toán nhiên liệu từ rương i đến rương i + 1
        for (int i = 1; i < p; i++)
        {
            if (positions[i] != (-1, -1) && positions[i + 1] != (-1, -1))
            {
                var start = positions[i];
                var end = positions[i + 1];
                var (minCostFromStart, minPathFromStart) = Dijkstra(start);

                if (minCostFromStart.ContainsKey(end))
                {
                    double cost = minCostFromStart[end];
                    var path = new List<(int, int)>(minPathFromStart[end]) { end };
                    totalCost += cost;
                    allPaths.Add((path, cost));
                }
            }
        }

        return (totalCost, allPaths);
    }
}
