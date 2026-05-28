using UnityEngine;

public class GameRuleTestComponent : MonoBehaviour
{
    private GameRule gameRule;

    public void Start()
    {
        gameRule = new GameRule_RabbitsActLikeFoxes();
    }
    public void Update()
    {
        gameRule.Enabled = this.isActiveAndEnabled;
    }
}

public class GameRule_InvertBeats : GameRule
{
    [ModifyOutputOf(nameof(GameFuncs.Beats))]
    public void Beats(BoardTileData tile, BoardTileData target, ref bool result)
    {
        Debug.Log("Inverted!");
        if (tile.piece != target.piece)
            result = !result;
    }
}

public class GameRule_RabbitsActLikeFoxes : GameRule
{
    [ModifyInputOf(nameof(GameFuncs.Beats))]
    public void Beats(ref BoardTileData tile, ref BoardTileData target)
    {
        if (tile.piece == GameFuncs.Rabbit)
            tile.piece = GameFuncs.Fox;
    }
}