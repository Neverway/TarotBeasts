//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public abstract class SelectableCard : MonoBehaviour
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public SpriteRenderer cardRenderer;
    public float hoverRaiseY = 0.3f;
    public float tweenDuration = 0.15f;

    public float swayMaxAngle = 12f;
    public float swaySpeed = 1.2f;
    public float swayOffset = 0f;

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    protected CardHand hand;
    protected int ownerSlot;
    private bool isHovered;
    private Vector3 restLocalPos;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    public Collider cardCollider;
    public InputSystem_Actions.UIActions inputActions;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    public void Start()
    {
        inputActions = new InputSystem_Actions().UI;
        inputActions.Enable();
    }

    private void Update()
    {
        if (cardCollider == null) return;

        float sway = Mathf.Sin(Time.time * swaySpeed + swayOffset) * swayMaxAngle;
        transform.localRotation = Quaternion.Euler(68, 0, sway);

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool hitting = cardCollider.Raycast(ray, out _, 100f);

        if (hitting && !isHovered)
        {
            isHovered = true;
            transform.DOLocalMoveY(restLocalPos.y + hoverRaiseY, tweenDuration).SetEase(Ease.OutCubic);
        }
        else if (!hitting && isHovered)
        {
            isHovered = false;
            transform.DOLocalMoveY(restLocalPos.y, tweenDuration).SetEase(Ease.InCubic);
        }

        if (hitting && inputActions.Click.WasPressedThisFrame())
            hand.OnCardSelected(this);
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Init(CardHand _hand, int _ownerSlot)
    {
        hand = _hand;
        ownerSlot = _ownerSlot;
        swayOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    public abstract void Activate(GameBoard _board, MatchController _matchController);

    public void Expend()
    {
        hand.RemoveCard(this);
        Destroy(gameObject);
    }
    
    #endregion
}
