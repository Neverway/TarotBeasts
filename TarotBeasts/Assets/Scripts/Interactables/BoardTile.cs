using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The script attached to the tiles on the board
/// The tile is created and referenced by GameBoard.cs through this script
/// </summary>
public class BoardTile : MonoBehaviour
{
    [Header("References")] 
    [Tooltip("The button component on the tile that allows the player to select it")]
    public Button button;
    [Tooltip("The image component that is set to represent the piece placed on this tile")]
    public Image icon;
    [Tooltip("The text component that represents the current score of this tile")]
    public TMP_Text score;
    [Tooltip("The text component that represents the current special of this tile")]
    public TMP_Text special;
    [Tooltip("The game object that holds the particle effects for when a piece is upgraded (this is enabled and disabled when a piece is upgraded and unupgraded)")]
    public GameObject upgradedFX;
    [Tooltip("The game objects that represent the direction the piece on this tile is scoring against")]
    public GameObject[] scoringArrows;
}
