using System.Drawing;

namespace Plus.Utilities.Enclosure;

public class PointField
{
    private static readonly Point BadPoint = new(-1, -1);
    private readonly List<Point> _pointList;
    private Point _mostDown = BadPoint;
    private Point _mostLeft = BadPoint;
    private Point _mostRight = BadPoint;
    private Point _mostTop = BadPoint;

    public PointField(byte forValue)
    {
        _pointList = new List<Point>();
        ForValue = forValue;
    }

    public byte ForValue { get; }

    public List<Point> GetPoints() => _pointList;

    public void Add(Point p)
    {
        if (_mostLeft == BadPoint)
            _mostLeft = p;
        if (_mostRight == BadPoint)
            _mostRight = p;
        if (_mostTop == BadPoint)
            _mostTop = p;
        if (_mostDown == BadPoint)
            _mostDown = p;
        if (p.X < _mostLeft.X)
            _mostLeft = p;
        if (p.X > _mostRight.X)
            _mostRight = p;
        if (p.Y > _mostTop.Y)
            _mostTop = p;
        if (p.Y < _mostDown.Y)
            _mostDown = p;
        _pointList.Add(p);
    }
}