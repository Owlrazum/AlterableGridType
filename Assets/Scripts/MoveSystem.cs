using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSystem : MonoBehaviour
{
    public static MoveSystem Instance;
    private void Awake()
    {
        Instance = this;
    }

    LinkedList<Move> moves;
    void Start()
    {
        moves = new LinkedList<Move>();
    }

    public event Action<Move> OnMoveCompleted;
    public void MoveCompleted(Move move)
    {
        moves.AddLast(move);
        OnMoveCompleted?.Invoke(move);
    }

    public event Action OnMoveUndone;
    public void MoveUndone()
    {
        OnMoveUndone?.Invoke();
    }

    public event Action OnMovePop;
    public void MovePop()
    {
        OnMovePop?.Invoke();
    }

    public event Action OnGetMoves;
    public void GetMoves()
    {
        OnGetMoves?.Invoke();
    }

    public event Action<Move> OnSendPoppedMove;
    public void SendPoppedMove(Move move)
    {
        OnSendPoppedMove(move);
    }

    public event Action<LinkedList<Move>> OnSendMoves;
    public void SendMoves(LinkedList<Move> moves)
    {
        OnSendMoves(moves);
    }

    private void ProcessMoveCompletion(Move move)
    {
        moves.AddLast(move);
    }

    private void ProcessMovePop()
    {
        Move move = moves.Last.Value;
        moves.RemoveLast();
        MoveSystem.Instance.SendPoppedMove(move);
    }
}
