using System.Linq;
using UnityEngine;

public struct BoardPos
{
    public BoardState board;
    public Vector2Int pos;

    public BoardPos(BoardState board, int x, int y) : this(board, new Vector2Int(x, y)) { }
    public BoardPos(BoardState board, int index) : this(board, board.IndexToGrid(index)) { }
    public BoardPos(BoardState board, Vector2Int pos)
    {
        this.board = board;
        this.pos = pos;
    }
    

    public int x => pos.x;
    public int y => pos.y;
    public int Index => IsInBounds ? board.GridToIndex(new Vector2Int(x, y)) : -1;
    public bool IsInBounds => board == null || board.IsInBounds(pos);

    public BoardPos MirrorY() => new BoardPos(board, x, board.height - y);
    public BoardPos MirrorX() => new BoardPos(board, board.width - x, y);
    public BoardPos Rotate180() => new BoardPos(board, board.width - x, board.height - y);
    //public BoardPos Rotate90() => new BoardPos(board, ?, ?);
    //public BoardPos RotateMinus90() => new BoardPos(board , ?, ?);

    public bool Equals(BoardPos other) => board != null && other.board == board && other.x == x && other.y == y;
    public bool Equals(int other) => board != null && other == Index;
    public override bool Equals(object obj)
    {
        if (obj is BoardPos pos) return Equals(pos);
        else if (obj is int i) return Equals(i);
        return false;
    }
    public override int GetHashCode() => (board.GetHashCode() * 23 + x) * 23 + y;

    public static BoardPos operator +(BoardPos p, BoardDir d) => new BoardPos(p.board, p.pos + d.dir);
    public static BoardPos operator -(BoardPos p, BoardDir d) => new BoardPos(p.board, p.pos - d.dir);
    public static BoardPos[] operator +(BoardPos p, BoardDir[] d)
        => d.Select((dir) => new BoardPos(p.board, p.pos + dir.dir)).ToArray();
    public static BoardPos[] operator -(BoardPos p, BoardDir[] d)
        => d.Select((dir) => new BoardPos(p.board, p.pos - dir.dir)).ToArray();
    public static BoardDir operator -(BoardPos a, BoardPos b) => new BoardDir(a.pos - b.pos);

    public static implicit operator int(BoardPos pos) => pos.Index;
    public static implicit operator Vector2Int(BoardPos pos) => pos.pos;
}

