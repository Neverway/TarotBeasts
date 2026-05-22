using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardTile : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TMP_Text score;
    public GameObject upgradedFX;
    public GameObject[] scoringArrows; // The order of scoring arrows is Up, down, left, right, up left, up right, down right, down left
}
