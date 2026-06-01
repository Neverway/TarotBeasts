using UnityEngine;

public class GameRuleTestComponent : MonoBehaviour
{
    private GameRule gameRule;

    public void Awake() => gameRule = new GameRule_InvertBeats();
    public void OnEnable() => gameRule.Enabled = true;
    public void OnDisable() => gameRule.Enabled = false;
}

public class GameRule_InvertBeats : GameRule
{
    [ModifyOutputOf(nameof(GameFuncs.Beats))]
    public void Beats(BoardTileData tile, BoardTileData target, ref bool result)
    {
        if (tile.piece != target.piece)
            result = !result;
    }
}

