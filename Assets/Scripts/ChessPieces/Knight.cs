using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPieces
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPieces[,] board, int TileCountX, int TileCountY)
    {
        //top right
        List<Vector2Int> r = new List<Vector2Int>();
        int x = currentX + 1;
        int y = currentY + 2;
        if (x < TileCountX && y < TileCountY)
        {
            if (board[x, y] == null || board[x, y].team!=team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
         x = currentX + 2;
         y = currentY + 1;
        if (x < TileCountX && y < TileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        //top left
        x = currentX - 2;
        y = currentY + 1;
        if (x >=0 && y < TileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        x = currentX - 1;
        y = currentY + 2;
        if (x >=0 && y < TileCountY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        //down right
        x = currentX + 1;
        y = currentY - 2;
        if (x <TileCountX && y >=0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        x = currentX + 2;
        y = currentY - 1;
        if (x < TileCountX && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        //down left
        x = currentX - 1;
        y = currentY - 2;
        if ( x>=0 && y>=0 )
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        return r;
    }
}
