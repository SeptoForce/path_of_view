using System.Collections.Generic;
using UnityEngine;

namespace PathOfView.GameLogic
{
    public class Pathfinder
    {
        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, bool[,] walkableTiles, Vector2Int offset)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            
            start -= offset;
            end -= offset;

            if (!walkableTiles[start.x, start.y])
            {
                return path;
            }

            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
            Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();

            List<Vector2Int> openSet = new List<Vector2Int>() { start };

            gScore[start] = 0;
            fScore[start] = Heuristic(start, end);

            while (openSet.Count > 0)
            {
                Vector2Int current = GetNodeWithLowestFScore(openSet, fScore);

                if (current == end)
                {
                    path = ReconstructPath(cameFrom, current);
                    break;
                }

                openSet.Remove(current);

                foreach (Vector2Int neighbor in GetNeighbors(current, walkableTiles))
                {
                    float tentativeGScore = gScore[current] + Vector2Int.Distance(current, neighbor);

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
            
            path = OffsetPath(path, offset);

            return path;
        }
        
        private static List<Vector2Int> OffsetPath(List<Vector2Int> path, Vector2Int offset)
        {
            List<Vector2Int> offsetPath = new List<Vector2Int>();
            foreach (Vector2Int node in path)
            {
                offsetPath.Add(node + offset);
            }

            return offsetPath;
        }

        private static Vector2Int GetNodeWithLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
        {
            Vector2Int lowest = openSet[0];
            float lowestScore = fScore[lowest];

            foreach (Vector2Int node in openSet)
            {
                if (fScore[node] < lowestScore)
                {
                    lowest = node;
                    lowestScore = fScore[node];
                }
            }

            return lowest;
        }

        private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            List<Vector2Int> path = new List<Vector2Int>() { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private static List<Vector2Int> GetNeighbors(Vector2Int node, bool[,] walkableTiles)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            int width = walkableTiles.GetLength(0);
            int height = walkableTiles.GetLength(1);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int newX = node.x + x;
                    int newY = node.y + y;

                    if (newX >= 0 && newX < width && newY >= 0 && newY < height && walkableTiles[newX, newY] && (x == 0 || y == 0))
                    {
                        neighbors.Add(new Vector2Int(newX, newY));
                    }
                }
            }

            return neighbors;
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}