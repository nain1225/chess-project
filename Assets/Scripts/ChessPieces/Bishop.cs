using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPieces
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPieces[,] board, int TileCountX, int TileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        //top right
        for (int x = currentX + 1, y = currentY + 1; x < TileCountX && y < TileCountY; x++, y++) 
        {
            if (board[x, y] == null) 
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
            }
        }
        //top left
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < TileCountY; x--, y++)
        {
            if (board[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
            }
        }
        //down right
        for (int x = currentX + 1, y = currentY - 1; x <TileCountX && y >=0; x++, y--)
        {
            if (board[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
            }
        }
        //down left
        for (int x = currentX - 1, y = currentY - 1; x >=0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
            }
        }
        return r;
    }
}
