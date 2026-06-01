using UnityEngine;

public struct BoardDir
{
    public Vector2Int dir;
    public BoardDir(int x, int y) => this.dir = new Vector2Int(x, y);
    public BoardDir(Vector2Int dir) => this.dir = dir;

    public int x => dir.x;
    public int y => dir.y;

    public static BoardDir Zero => new BoardDir(0, 0);
    public static BoardDir[] NoDirections => new BoardDir[] { };

    public static BoardDir North => new BoardDir(0, 1);
    public static BoardDir South => new BoardDir(0, -1);
    public static BoardDir West => new BoardDir(-1, 0);
    public static BoardDir East => new BoardDir(1, 0);
    public static BoardDir[] Cardinals => new[] { North, East, South, West };

    public static BoardDir NorthEast => new BoardDir(1, 1);
    public static BoardDir SouthEast => new BoardDir(1, -1);
    public static BoardDir SouthWest => new BoardDir(-1, -1);
    public static BoardDir NorthWest => new BoardDir(-1, 1);
    public static BoardDir[] Diagonals => new[] { NorthEast, SouthEast, SouthWest, NorthWest };

    public static BoardDir[] All8Directions => new[] { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };

    public BoardDir MirrorY() => new BoardDir(x, -y);
    public BoardDir MirrorX() => new BoardDir(-x, y);
    public BoardDir Rotate180() => new BoardDir(-x, -y);
    public BoardDir Rotate90() => new BoardDir(y, -x);
    public BoardDir RotateMinus90() => new BoardDir(-y, x);

    public static BoardDir operator +(BoardDir a, BoardDir b) => new BoardDir(a.dir + b.dir);
    public static BoardDir operator -(BoardDir a, BoardDir b) => new BoardDir(a.dir - b.dir);
    public static BoardDir operator *(BoardDir d, int m) => new BoardDir(d.dir * m);

    public static implicit operator Vector2Int(BoardDir dir) => dir.dir;
}