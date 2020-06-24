using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    [Header("Board")]
    public GameObject[] m_Pieces;
    private Node[,] m_Grid = new Node[3, 3];
    private List<Movement> m_Movements = new List<Movement>();
    private int m_Turn = 0;

    [Header("UI")]
    public Text m_TextUI;

    public bool IsValidMove(int x, int y) => m_Grid[x, y] == null;
    public bool IsFilled(int x, int y) => !IsValidMove(x, y);

    private void Update()
    {
        if (IsGameOver()) return;

        for (int key = (int)KeyCode.Alpha0; key < (int)KeyCode.Alpha9; key++)
        {
            int x = (key - 48) / 3;
            int y = (key - 48) % 3;

            if (Input.GetKeyDown((KeyCode)key) && IsValidMove(x, y))
            {
                // Human
                Instantiate(m_Pieces[m_Turn], new Vector3(x, 0, y), Quaternion.identity);
                m_Grid[x, y] = new Node() { x = x, y = y, player = m_Turn };

                // CPU
                if (!IsGameOver())
                {
                    m_Turn = ++m_Turn % 2;

                    var move = BestMove(0, m_Turn);
                    move.player = m_Turn;

                    Instantiate(m_Pieces[m_Turn], new Vector3(move.x, 0, move.y), Quaternion.identity);
                    m_Grid[move.x, move.y] = move;

                    m_Turn = ++m_Turn % 2;
                }
            }
        }
    }

    private int Min(List<int> children)
    {
        int min = int.MaxValue;
        int index = -1;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] < min)
            {
                min = children[i];
                index = i;
            }
        }
        return children[index];
    }

    private int Max(List<int> children)
    {
        int max = int.MinValue;
        int index = -1;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] > max)
            {
                max = children[i];
                index = i;
            }
        }
        return children[index];
    }

    private int MiniMax(int depth, int turn)
    {
        if (HasWon(1)) return +1;
        if (HasWon(0)) return -1;

        List<Node> moves = GetMovesAvailable();
        if (moves.Count == 0) return 0;

        List<int> scores = new List<int>();
        for (int i = 0; i < moves.Count; i++)
        {
            var position = new Node() { x = moves[i].x, y = moves[i].y, player = turn };
            m_Grid[position.x, position.y] = position;

            int score = MiniMax(depth + 1, (turn + 1) % 2);
            scores.Add(score);

            if (depth == 0)
                m_Movements.Add(new Movement(score, position));

            m_Grid[position.x, position.y] = null;
        }

        return turn == 1 ? Max(scores) : Min(scores);
    }

    private Node BestMove(int depth, int turn)
    {
        m_Movements.Clear();
        MiniMax(depth, turn);        

        if (m_Movements.Count > 0)
        {
            m_Movements.Sort((b, a) => a.score.CompareTo(b.score));
            Debug.Log(string.Join(",", m_Movements));

            return new Node() { x = m_Movements[0].position.x, y = m_Movements[0].position.y };
        }

        return null;
    }

    public bool IsGameOver()
    {
        if (GetMovesAvailable().Count == 0)
        {
            m_TextUI.text = "It's a draw!";
            return true;
        }

        if (HasWon(0))
        {
            m_TextUI.text = "You Won!";
            return true;
        }

        if (HasWon(1))
        {
            m_TextUI.text = "You Lost!";
            return true;
        }

        return false;
    }

    public bool HasWon(int player)
    {
        if (IsFilled(0, 0) && IsFilled(1, 1) && IsFilled(2, 2))
            if (m_Grid[0, 0].player == player && m_Grid[1, 1].player == player && m_Grid[2, 2].player == player)
                return true;

        if (IsFilled(2, 0) && IsFilled(1, 1) && IsFilled(0, 2))
            if (m_Grid[2, 0].player == player && m_Grid[1, 1].player == player && m_Grid[0, 2].player == player)
            return true;

        for (int i = 0; i < 3; i++)
        {
            if (IsFilled(i, 0) && IsFilled(i, 1) && IsFilled(i, 2))
                if (m_Grid[i, 0].player == player && m_Grid[i, 1].player == player && m_Grid[i, 2].player == player)
                return true;

            if (IsFilled(0, i) && IsFilled(1, i) && IsFilled(2, i))
                if (m_Grid[0, i].player == player && m_Grid[1, i].player == player && m_Grid[2, i].player == player)
                return true;
        }

        return false;
    }

    public List<Node> GetMovesAvailable()
    {
        List<Node> nodes = new List<Node>();
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                if (IsValidMove(x, y))
                    nodes.Add(new Node() { x = x, y = y });

        return nodes;
    }
}
