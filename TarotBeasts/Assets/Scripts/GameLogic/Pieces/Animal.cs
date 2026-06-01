using System;
public abstract class Animal : Piece
{
    public Animal[] GetBeatenAnimals() => throw new NotImplementedException();
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
    public static bool IsUpgraded(this Animal piece)
    {
        return Context.UsingPiece(piece, () =>
        {
            ProcessInput(ref piece);

            bool result = false;
            if (piece.Is(out CommonAnimal asCommon))
                result = asCommon.BASE_IsUpgraded();

            return ProcessOutput(piece, result);
        });
    }
}