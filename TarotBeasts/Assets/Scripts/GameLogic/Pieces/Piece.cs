using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public abstract class Piece
{
    public BoardPos position;
    public int ownedByPlayerID;

    public int typeID => TypeToTypeID.ContainsKey(GetType()) ? TypeToTypeID[GetType()] : -1;
    public virtual bool Is<T>() where T : Piece => this is T;
    public virtual bool Is<T>(out T casted) where T : Piece
        => (casted = this is T asT ? asT : default) != null;

    public virtual void OnUpdatePieceInfo() { }

    public abstract int BASE_GetScore();
    


    //------------------------------------------------ Static Piece instance creators -------------------------------------------------
    public static Piece Create<T>(int playerID, BoardPos pos) where T : Piece
        => Create(typeof(T), playerID, pos);
    public static Piece Create(Type type, int playerID, BoardPos pos)
    {
        if (!typeof(Piece).IsAssignableFrom(type))
            throw new ArgumentException("Type provided to Piece.Create(Type) must be of type Piece");

        Piece piece = Activator.CreateInstance(type) as Piece;
        piece.ownedByPlayerID = playerID;
        piece.position = pos;
        return piece;
    }
    public static Piece Create(int typeID, int playerID, BoardPos pos)
    {
        if (TypeIDToType.TryGetValue(typeID, out Type type))
            return Create(type, playerID, pos);
        else
            throw new NoTypeIDFoundException(typeID);
    }
    
    
    
    //------------------------------------------------ TypeID Attiribute and cache ----------------------------------------------------
    //----- TypeID to Type cache and initialization
    private static Dictionary<int, Type> TypeIDToType;
    private static Dictionary<Type, int> TypeToTypeID;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeTypeIDToType()
    {
        TypeIDToType = new Dictionary<int, Type>();
        TypeToTypeID = new Dictionary<Type, int>();

        foreach (var type in TypeCache.GetTypesDerivedFrom<Piece>())
        {
            //Find the TypeIDAttribute and get the typeID assigned to it
            var attribute = type.GetAttribute<TypeIDAttribute>();
            if (attribute == null) continue;
            int typeID = attribute.typeID;

            //If a type ID was already assigned to this type, complain about it to the devs so they are aware
            if (TypeIDToType.ContainsKey(typeID))
                throw new DuplicateTypeIDException(typeID, TypeIDToType[typeID], type);

            //Register typeID to types in dictionaries
            TypeIDToType.Add(typeID, type);
            TypeToTypeID.Add(type, typeID);
        }
    }
    
    //----- TypeID Attribute itself
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TypeIDAttribute : Attribute {
        public int typeID;
        public TypeIDAttribute(int typeID) => this.typeID = typeID;
    }

    //------------------------------------------------ Exceptions ---------------------------------------------------------------------
    public class DuplicateTypeIDException : Exception {
        public DuplicateTypeIDException(int typeID, Type type1, Type type2) : base(
            $"Duplicate type ID \'{typeID}\' found for pieces: {type1.Name} and {type2.Name}. " +
            $"Must change [TypeID({typeID})] attribute for one of these classes to a different int id") { }
    }
    public class NoTypeIDFoundException : Exception {
        public NoTypeIDFoundException(int typeID) : base(
            $"No typeID found for \'{typeID}\'. Make sure the intended Piece class " +
            $"has a [TypeID({typeID})] attribute on it") { }
    }
}
public static class ExtensionMethods_Piece
{
    public static bool IsAllyOf(this Piece piece, Piece other)
        => piece.ownedByPlayerID == other.ownedByPlayerID;
    public static bool IsEnemyOf(this Piece piece, Piece other)
        => piece.ownedByPlayerID != other.ownedByPlayerID;

    public static Piece[] OnTeam(this Piece[] animals, int teamId)
        => animals.Where(animal => animal.ownedByPlayerID == teamId).ToArray();
    public static Piece[] NotOnTeam(this Piece[] animals, int teamId)
        => animals.Where(animal => animal.ownedByPlayerID != teamId).ToArray();

    public static T[] ThatAre<T>(this Piece[] animals) where T : Piece
        => animals.Where(animal => animal.ownedByPlayerID is T).Cast<T>().ToArray();
    public static Piece[] ThatAreNot<T>(this Piece[] animals) where T : Piece
        => animals.Where(animal => animal.ownedByPlayerID is not T).ToArray();
}
