using System;
using System.Linq;

//Base class for main pieces of the game: Wolf, Fox, and Rabbits
public abstract class CommonAnimal : Animal
{
    private bool _isUpgraded = false;
    public bool IsUpgraded => _isUpgraded && !Context.HasTag(Context.Tags.UPDATING_PIECE_INFO);

    public override void OnUpdatePieceInfo()
    {
        base.OnUpdatePieceInfo();
        _isUpgraded = false;
        _isUpgraded = GameFuncs.CalculateIsUpgraded(this);
    }

    //Animal is upgraded if it attacks and beats any allied pieces
    public virtual bool BASE_IsUpgraded() => GetAttackedPieces()
        .Any((p) => p is Animal a && this.IsAllyOf(a) && this.Beats(a));
    public override BoardDir[] BASE_GetAttackDirections() => BoardDir.Cardinals;
    public override int BASE_GetScore() => GetBeatenEnemyAnimals().Length;
}

[Serializable, TypeID(1)]
public class Wolf : CommonAnimal
{
    //Wolf beats Foxes
    public override bool BASE_Beats(Animal piece) => piece.Is<Fox>();

    //Also scores against any other Wolves if upgraded
    public override int BASE_GetScore()
        => base.BASE_GetScore() + GetAttackedPieces().ThatAre<Wolf>().Length;
}

[Serializable, TypeID(2)]
public class Fox : CommonAnimal
{
    //Fox beats Rabbits if either Fox is upgraded, or rabbit is unupgraded
    public override bool BASE_Beats(Animal piece)
        => piece.Is(out Rabbit rabbit) && (IsUpgraded || !rabbit.IsUpgraded);

    //Attacks in all 8 directions if upgraded
    public override BoardDir[] BASE_GetAttackDirections()
    {
        return IsUpgraded ? BoardDir.All8Directions : BoardDir.Cardinals;
    }
}

[Serializable, TypeID(3)]
public class Rabbit : CommonAnimal
{
    //Rabbit beats Wolfs and, if upgraded, beats unupgraded Foxes
    public override bool BASE_Beats(Animal piece)
        => piece.Is<Wolf>() || (IsUpgraded && piece.Is(out Fox fox) && !fox.IsUpgraded);
}


public static partial class GameFuncs
{
    public static bool CalculateIsUpgraded(CommonAnimal piece)
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