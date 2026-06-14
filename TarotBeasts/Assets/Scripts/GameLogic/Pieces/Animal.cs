using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Animal : Piece
{
    public Piece[] GetAttackedPieces()
    {
        List<Piece> attackedPieces = new();
        foreach (BoardDir attackingDir in this.GetAttackDirections())
        {
            BoardPos attackingPos = position + attackingDir;
            if (!attackingPos.IsInBounds) continue;

            Piece targetPiece = position.board.Tiles[position + attackingDir].piece;
            if (targetPiece != null)
                attackedPieces.Add(targetPiece);
        }
        return attackedPieces.ToArray();
    }
    public Animal[] GetBeatenEnemyAnimals()
        => GetAttackedPieces()
        .Where((p) => p is Animal a && this.IsEnemyOf(a) && this.Beats(a))
        .Cast<Animal>()
        .ToArray();

    public abstract bool BASE_Beats(Animal piece);
    public abstract BoardDir[] BASE_GetAttackDirections();
}

public static partial class GameFuncs
{
    public static bool Beats(this Animal piece, Animal other)
    {
        return Context.UsingPiece(piece, () =>
        {
            ProcessInput(ref piece, ref other);
            bool result = piece.BASE_Beats(other);
            return ProcessOutput(piece, other, result);
        });
    }

    public static BoardDir[] GetAttackDirections(this Animal piece)
    {
        return Context.UsingPiece(piece, () =>
        {
            ProcessInput(ref piece);
            BoardDir[] result = piece.BASE_GetAttackDirections();
            return ProcessOutput(piece, result);
        });
    }
}