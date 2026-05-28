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
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardHand : MonoBehaviour
{
    #region========================================( Variables )====================================================== //

    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    [SerializeField] private float hiddenZ = 0;
    [SerializeField] private float revealedZ = 0;
    [SerializeField] private float tweenDuration = 0.3f;
    [SerializeField] private float cardSpacing = 5f;
    public UnityEvent OnHovered = new UnityEvent();
    public UnityEvent OnUnhovered = new UnityEvent();

    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/
    public bool HasCards => cards.Count > 0;

    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private List<SelectableCard> cards = new List<SelectableCard>();
    private SelectableCard pendingCard;
    private bool awaitingTarget;
    private bool isRevealed;


    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    private GameBoard board;
    private MatchController matchController;
    public Transform cardRoot;
    public Collider hoverCollider;



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, hiddenZ);
    }

    private void Update()
    {
        if (hoverCollider == null) return;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        bool hitting = hoverCollider.Raycast(ray, out _, 100f);

        if (hitting && !isRevealed) Reveal();
        else if (!hitting && isRevealed) Hide();
    }

    private void OnDisable()
    {
        OnUnhovered.Invoke();
    }

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    private void Reveal()
    {
        isRevealed = true;
        transform.DOLocalMoveZ(revealedZ, tweenDuration).SetEase(Ease.OutCubic);
        OnHovered.Invoke();
    }
    
    private void Hide()
    {
        isRevealed = false;
        transform.DOLocalMoveZ(hiddenZ, tweenDuration).SetEase(Ease.InCubic);
        OnUnhovered.Invoke();
    }
    
    private void LayoutCards()
    {
        float totalWidth = (cards.Count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.localPosition = new Vector3(startX + i * cardSpacing, 0f, 0f);
        }
    }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/
    public void Init(GameBoard _board, MatchController _matchController)
    {
        board = _board;
        matchController = _matchController;
    }

    public void DealCards(List<SelectableCard> newCards, int ownerSlot)
    {
        foreach (var card in cards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        cards.Clear();

        foreach (var card in newCards)
        {
            card.transform.SetParent(cardRoot, false);
            card.Init(this, ownerSlot);
            cards.Add(card);
        }
        LayoutCards();
    }

    public void OnCardSelected(SelectableCard card)
    {
        if (awaitingTarget) return;
        pendingCard = card;
        awaitingTarget = true;
        card.Activate(board, matchController);
    }

    public void RemoveCard(SelectableCard card)
    {
        cards.Remove(card);
        awaitingTarget = false;
        pendingCard = null;
        LayoutCards();
    }

    public void FinishCardTurn()
    {
        awaitingTarget = false;
        pendingCard = null;
    }

    #endregion
}
