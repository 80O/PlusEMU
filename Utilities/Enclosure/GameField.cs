using Astar.Algorithm;
using Plus.Utilities.Enclosure.Algorithm;
using System.Drawing;

namespace Plus.Utilities.Enclosure;

public class GameField : IPathNode
{
    private readonly AStarSolver<GameField> _astarSolver;
    private readonly bool _diagonal;
    private readonly Queue<FieldUpdate> _newEntries; // = new Queue<FieldUpdate>();
    private byte[,] _currentField;
    private FieldUpdate _currentlyChecking;

    public GameField(byte[,] theArray, bool diagonalAllowed)
    {
        _currentField = theArray;
        _diagonal = diagonalAllowed;
        _newEntries = new Queue<FieldUpdate>();
        _astarSolver = new AStarSolver<GameField>(diagonalAllowed, AStarHeuristicType.EXPERIMENTAL_SEARCH, this, theArray.GetUpperBound(1) + 1, theArray.GetUpperBound(0) + 1);
    }

    public bool this[int y, int x]
    {
        get
        {
            if (_currentField == null)
                return false;
            if (y < 0 || x < 0)
                return false;
            return y <= _currentField.GetUpperBound(0) && x <= _currentField.GetUpperBound(1);
        }
    }

    public bool IsBlocked(int x, int y, bool lastTile)
    {
        if (_currentlyChecking.X == x && _currentlyChecking.Y == y)
            return true;
        return GetValue(x, y) != _currentlyChecking.Value;
    }

    public void UpdateLocation(int x, int y, byte value)
    {
        _newEntries.Enqueue(new FieldUpdate(x, y, value));
    }

    public List<PointField> DoUpdate()
    {
        var returnList = new List<PointField>();
        while (_newEntries.Count > 0)
        {
            _currentlyChecking = _newEntries.Dequeue();
            var pointList = GetConnectedItems(_currentlyChecking);
            if (pointList == null)
                return null;
            if (pointList.Count > 1)
            {
                var routeList = HandleListOfConnectedPoints(pointList);
                foreach (var nodeList in routeList)
                {
                    if (nodeList.Count >= 4)
                    {
                        var field = FindClosed(nodeList);
                        if (field != null) returnList.Add(field);
                    }
                }
            }
            _currentField[_currentlyChecking.Y, _currentlyChecking.X] = _currentlyChecking.Value;
        }
        return returnList;
    }

    private PointField FindClosed(IEnumerable<AStarSolver<GameField>.PathNode> nodeList)
    {
        var returnList = new PointField(_currentlyChecking.Value);
        var minX = int.MaxValue;
        var maxX = int.MinValue;
        var minY = int.MaxValue;
        var maxY = int.MinValue;
        foreach (var node in nodeList)
        {
            if (node.X < minX)
                minX = node.X;
            if (node.X > maxX)
                maxX = node.X;
            if (node.Y < minY)
                minY = node.Y;
            if (node.Y > maxY)
                maxY = node.Y;
        }
        var middleX = Convert.ToInt32(Math.Ceiling((maxX - minX) / 2f) + minX);
        var middleY = Convert.ToInt32(Math.Ceiling((maxY - minY) / 2f) + minY);
        //Console.WriteLine("Middle: x:[{0}]  y:[{1}]", middleX, middleY);
        var toFill = new List<Point>();
        var checkedItems = new List<Point> { new(_currentlyChecking.X, _currentlyChecking.Y) };
        toFill.Add(new Point(middleX, middleY));
        while (toFill.Count > 0)
        {
            var current = toFill[0];
            var x = current.X;
            var y = current.Y;
            if (x < minX)
                return null; //OOB
            if (x > maxX)
                return null; //OOB
            if (y < minY)
                return null; //OOB
            if (y > maxY)
                return null; //OOB
            Point toAdd;
            if (this[y - 1, x] && _currentField[y - 1, x] == 0)
            {
                toAdd = new Point(x, y - 1);
                if (!toFill.Contains(toAdd) && !checkedItems.Contains(toAdd))
                    toFill.Add(toAdd);
            }
            if (this[y + 1, x] && _currentField[y + 1, x] == 0)
            {
                toAdd = new Point(x, y + 1);
                if (!toFill.Contains(toAdd) && !checkedItems.Contains(toAdd))
                    toFill.Add(toAdd);
            }
            if (this[y, x - 1] && _currentField[y, x - 1] == 0)
            {
                toAdd = new Point(x - 1, y);
                if (!toFill.Contains(toAdd) && !checkedItems.Contains(toAdd))
                    toFill.Add(toAdd);
            }
            if (this[y, x + 1] && _currentField[y, x + 1] == 0)
            {
                toAdd = new Point(x + 1, y);
                if (!toFill.Contains(toAdd) && !checkedItems.Contains(toAdd))
                    toFill.Add(toAdd);
            }
            if (GetValue(current) == 0)
                returnList.Add(current);
            checkedItems.Add(current);
            toFill.RemoveAt(0);
        }
        return returnList;
    }

    private IEnumerable<LinkedList<AStarSolver<GameField>.PathNode>> HandleListOfConnectedPoints(List<Point> pointList)
    {
        var returnList = new List<LinkedList<AStarSolver<GameField>.PathNode>>();
        var amount = 0;
        foreach (var begin in pointList)
        {
            amount++;
            if (amount == pointList.Count / 2 + 1)
                return returnList;
            foreach (var end in pointList)
            {
                if (begin == end)
                    continue;
                LinkedList<AStarSolver<GameField>.PathNode> list = _astarSolver.Search(end, begin);
                if (list != null) returnList.Add(list);
            }
        }
        return returnList;
    }

    private List<Point> GetConnectedItems(FieldUpdate update)
    {
        if (update == null)
            return null;
        var connectedItems = new List<Point>();
        var x = update.X;
        var y = update.Y;
        if (_diagonal)
        {
            if (this[y - 1, x - 1] && _currentField[y - 1, x - 1] == update.Value) connectedItems.Add(new Point(x - 1, y - 1));
            if (this[y - 1, x + 1] && _currentField[y - 1, x + 1] == update.Value) connectedItems.Add(new Point(x + 1, y - 1));
            if (this[y + 1, x - 1] && _currentField[y + 1, x - 1] == update.Value) connectedItems.Add(new Point(x - 1, y + 1));
            if (this[y + 1, x + 1] && _currentField[y + 1, x + 1] == update.Value) connectedItems.Add(new Point(x + 1, y + 1));
        }
        if (this[y - 1, x] && _currentField[y - 1, x] == update.Value) connectedItems.Add(new Point(x, y - 1));
        if (this[y + 1, x] && _currentField[y + 1, x] == update.Value) connectedItems.Add(new Point(x, y + 1));
        if (this[y, x - 1] && _currentField[y, x - 1] == update.Value) connectedItems.Add(new Point(x - 1, y));
        if (this[y, x + 1] && _currentField[y, x + 1] == update.Value) connectedItems.Add(new Point(x + 1, y));
        return connectedItems;
    }

    private void SetValue(int x, int y, byte value)
    {
        if (this[y, x]) _currentField[y, x] = value;
    }

    public byte GetValue(int x, int y) => this[y, x] ? _currentField[y, x] : (byte)0;

    public byte GetValue(Point p) => this[p.Y, p.X] ? _currentField[p.Y, p.X] : (byte)0;

    public void Dispose()
    {
        _currentField = null;
        if (_newEntries != null)
            _newEntries.Clear();
    }
}