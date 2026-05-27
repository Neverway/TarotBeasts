//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTimers
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public readonly float[] RemainingTime;
    public readonly bool[] Eliminated;

    public event Action<int> OnPlayerEliminated;

    public PlayerTimers(int playerCount, float durationSeconds)
    {
        RemainingTime = new float[playerCount];
        Eliminated = new bool[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            RemainingTime[i] = durationSeconds;
        }
    }


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public bool Tick(int activeSlot, float deltaTime)
    {
        if (Eliminated[activeSlot]) return false;

        RemainingTime[activeSlot] -= deltaTime;
        if (RemainingTime[activeSlot] <= 0f)
        {
            RemainingTime[activeSlot] = 0f;
            Eliminated[activeSlot] = true;
            OnPlayerEliminated?.Invoke(activeSlot);
            return true;
        }

        return false;
    }

    public bool AllEliminated()
    {
        foreach (var eliminated in Eliminated)
        {
            if (!eliminated)
            {
                return false;
            }
        }
        return true;

    }

    public int ActivePlayerCount()
    {
        int count = 0;
        foreach (var eliminated in Eliminated)
        {
            if (!eliminated)
            {
                count++;
            }
        }

        return count;
    }


    #endregion
}
