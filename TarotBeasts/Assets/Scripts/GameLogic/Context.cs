using System;
using UnityEngine;

public static class Context
{
    public static Piece currentPiece;
    public static BoardState currentBoard;
    public static void UsingPiece(Piece piece, Action action)
        => Using(ref currentPiece, piece, action);
    public static TResult UsingPiece<TResult>(Piece piece, Func<TResult> func)
        => Using(ref currentPiece, piece, func);


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        currentPiece = null;
        currentBoard = null;
    }
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
