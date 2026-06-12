using System;
using System.Collections.Generic;
using UnityEngine;

public static class Context
{
    public static class Tags
    {
        public static readonly string UPDATING_PIECE_INFO = nameof(UPDATING_PIECE_INFO);
    }

    public static Piece currentPiece;
    public static BoardState currentBoard;
    private static Stack<string> tags = new Stack<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        currentPiece = null;
        currentBoard = null;
        tags = new Stack<string>();
    }

    public static bool HasTag(string tag) => tags.Contains(tag);
    public static void PushTag(string tag) => tags.Push(tag);
    public static void PopTag() => tags.Pop();

    public static void UsingPiece(Piece piece, Action action)
        => Using(ref currentPiece, piece, action);
    public static TResult UsingPiece<TResult>(Piece piece, Func<TResult> func)
        => Using(ref currentPiece, piece, func);
    private static void Using<T>(ref T a, T b, Action action)
    {
        T last = a;
        a = b;
        action.Invoke();
        a = last;
    }
    private static TResult Using<T, TResult>(ref T a, T b, Func<TResult> action)
    {
        T last = a;
        a = b;
        TResult result = action.Invoke();
        a = last;
        return result;
    }
}
