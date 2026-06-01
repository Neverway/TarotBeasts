using System.Linq;

//Base class for main pieces of the game: Wolf, Fox, and Rabbits
public abstract class CommonAnimal : Animal
{
    public bool BASE_IsUpgraded() => GetBeatenAnimals().OnTeam(ownedByTeamID).Any();
    public override BoardDir[] BASE_GetAttackDirections() => BoardDir.Cardinals;
    public override int BASE_GetScore() => GetBeatenAnimals().NotOnTeam(ownedByTeamID).Length;
}

public class Wolf : CommonAnimal
{
    //Wolf beats Foxes
    public override bool BASE_Beats(Animal piece) => piece.Is<Fox>();

    //Also scores against any other Wolves if upgraded
    public override int BASE_GetScore()
        => base.BASE_GetScore() + GetBeatenAnimals().ThatAre<Wolf>().Length; //Wrong, not "beaten" animals
}
public class Fox : CommonAnimal
{
    //Fox beats Rabbits if either Fox is upgraded, or rabbit is unupgraded
    public override bool BASE_Beats(Animal piece)
        => piece.Is(out Rabbit rabbit) && (this.IsUpgraded() || !rabbit.IsUpgraded());

    //Attacks in all 8 directions if upgraded
    public override BoardDir[] BASE_GetAttackDirections()
        => this.IsUpgraded() ? BoardDir.All8Directions : BoardDir.Cardinals;
}
public class Rabbit : CommonAnimal
{
    //Rabbit beats Wolfs and, if upgraded, beats unupgraded Foxes
    public override bool BASE_Beats(Animal piece)
        => piece.Is<Wolf>() || (this.IsUpgraded() && piece.Is(out Fox fox) && !fox.IsUpgraded());
}