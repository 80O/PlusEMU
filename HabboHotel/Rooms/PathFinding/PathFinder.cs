namespace Plus.HabboHotel.Rooms.PathFinding;

public static class PathFinder
{
    public static Vector2D[] DiagMovePoints =
    {
        new(-1, -1),
        new(0, -1),
        new(1, -1),
        new(1, 0),
        new(1, 1),
        new(0, 1),
        new(-1, 1),
        new(-1, 0)
    };

    public static Vector2D[] NoDiagMovePoints =
    {
        new(0, -1),
        new(1, 0),
        new(0, 1),
        new(-1, 0)
    };

    public static List<Vector2D> FindPath(RoomUser user, bool diag, Gamemap map, Vector2D start, Vector2D end)
    {
        var path = new List<Vector2D>();
        var nodes = FindPathReversed(user, diag, map, start, end);
        if (nodes != null)
        {
            path.Add(end);
            while (nodes.Next != null)
            {
                path.Add(nodes.Next.Position);
                nodes = nodes.Next;
            }
        }
        return path;
    }

    public static PathFinderNode FindPathReversed(RoomUser user, bool diag, Gamemap map, Vector2D start,
        Vector2D end)
    {
        var openList = new MinHeap<PathFinderNode>(256);
        var pfMap = new PathFinderNode[map.Model.MapSizeX, map.Model.MapSizeY];
        PathFinderNode node;
        Vector2D tmp;
        int cost;
        int diff;
        var current = new PathFinderNode(start)
        {
            Cost = 0
        };
        var finish = new PathFinderNode(end);
        pfMap[current.Position.X, current.Position.Y] = current;
        openList.Add(current);
        while (openList.Count > 0)
        {
            current = openList.ExtractFirst();
            current.InClosed = true;
            for (var i = 0; diag ? i < DiagMovePoints.Length : i < NoDiagMovePoints.Length; i++)
            {
                tmp = current.Position + (diag ? DiagMovePoints[i] : NoDiagMovePoints[i]);
                var isFinalMove = tmp.X == end.X && tmp.Y == end.Y;
                if (map.IsValidStep(new Vector2D(current.Position.X, current.Position.Y), tmp, isFinalMove, user.AllowOverride))
                {
                    if (pfMap[tmp.X, tmp.Y] == null)
                    {
                        node = new PathFinderNode(tmp);
                        pfMap[tmp.X, tmp.Y] = node;
                    }
                    else
                        node = pfMap[tmp.X, tmp.Y];
                    if (!node.InClosed)
                    {
                        diff = 0;
                        if (current.Position.X != node.Position.X) diff += 1;
                        if (current.Position.Y != node.Position.Y) diff += 1;
                        cost = current.Cost + diff + node.Position.GetDistanceSquared(end);
                        if (cost < node.Cost)
                        {
                            node.Cost = cost;
                            node.Next = current;
                        }
                        if (!node.InOpen)
                        {
                            if (node.Equals(finish))
                            {
                                node.Next = current;
                                return node;
                            }
                            node.InOpen = true;
                            openList.Add(node);
                        }
                    }
                }
            }
        }
        return null;
    }
}