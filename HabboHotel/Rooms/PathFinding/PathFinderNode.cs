namespace Plus.HabboHotel.Rooms.PathFinding;

public sealed class PathFinderNode : IComparable<PathFinderNode>
{
    public int Cost = int.MaxValue;
    public bool InClosed = false;
    public bool InOpen = false;
    public PathFinderNode Next;
    public Vector2D Position;

    public PathFinderNode(Vector2D position)
    {
        Position = position;
    }

    public int CompareTo(PathFinderNode other) => Cost.CompareTo(other.Cost);

    public override bool Equals(object obj) => obj is PathFinderNode && ((PathFinderNode)obj).Position.Equals(Position);

    public bool Equals(PathFinderNode breadcrumb) => breadcrumb.Position.Equals(Position);

    public override int GetHashCode() => Position.GetHashCode();
}