//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTileMap
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public readonly SpecialTileType[] Types;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/

    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/

    public SpecialTileMap(int tileCount)
    {
        Types = new SpecialTileType[tileCount];
    }

    public bool IsSpecial(int tileIndex) => Types[tileIndex] != SpecialTileType.None;

    public static SpecialTileMap Generate(int tileCount, float coverage = 0.25f)
    {
        var map = new SpecialTileMap(tileCount);

        var weighted = new List<SpecialTileType>
        {
            SpecialTileType.DoubleScore,
            SpecialTileType.TripleScore,
            SpecialTileType.NullScore,
            SpecialTileType.AutoUpgrade,
        };

        int specialCount = Mathf.Max(1, Mathf.RoundToInt(tileCount * coverage));

        var indices = new List<int>();
        for (int i = 0; i < tileCount; i++)
        {
            indices.Add(i);
        }

        for (int i = 0; i < specialCount && indices.Count > 0; i++)
        {
            int pick = UnityEngine.Random.Range(0, indices.Count);
            int idx = indices[pick];
            indices.RemoveAt(pick);
            map.Types[idx] = weighted[UnityEngine.Random.Range(0, weighted.Count)];
        }

        return map;
    }


    #endregion
}

public enum SpecialTileType
{
    None = 0,
    DoubleScore = 1,
    TripleScore = 2,
    NullScore = 3,
    AutoUpgrade = 4,
}