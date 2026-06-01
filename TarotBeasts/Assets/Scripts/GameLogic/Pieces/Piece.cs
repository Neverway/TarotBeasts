using System.Linq;

public abstract class Piece
{
    public BoardPos position;
    public int ownedByTeamID;

    public virtual bool Is<T>() where T : Piece => this is T;
    public virtual bool Is<T>(out T casted) where T : Piece
        => (casted = this is T asT ? asT : default) != null;

    public abstract int BASE_GetScore();
}
public static class ExtensionMethods_Piece
{
    public static Piece[] OnTeam(this Piece[] animals, int teamId)
        => animals.Where(animal => animal.ownedByTeamID == teamId).ToArray();
    public static Piece[] NotOnTeam(this Piece[] animals, int teamId)
        => animals.Where(animal => animal.ownedByTeamID != teamId).ToArray();

    public static T[] ThatAre<T>(this Piece[] animals) where T : Piece
        => animals.Where(animal => animal.ownedByTeamID is T).Cast<T>().ToArray();
    public static Piece[] ThatAreNot<T>(this Piece[] animals) where T : Piece
        => animals.Where(animal => animal.ownedByTeamID is not T).ToArray();
}
public abstract class ExoticAnimal : Animal { }