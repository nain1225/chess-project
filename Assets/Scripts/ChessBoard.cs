using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [Header("Artwork")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material HighlightMaterial;
    
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float tileSize = 2.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private float spawnPositioningYOffset = 1.1f;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1f;
    [SerializeField] private GameObject VictoryScreen;


    [Header("Prefabs & Materials")]
    public GameObject[] prefabs;
    public Material[] materials;

    private Vector3 bounds;
    private int tileCountX = 8;
    private int tileCountY = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover = -Vector2Int.one;
    private ChessPieces[,] chessPieces;
    private ChessPieces currentlyDragging;
    private List<ChessPieces> whiteDead = new List<ChessPieces>();
    private List<ChessPieces> blackDead = new List<ChessPieces>();
    private List<Vector2Int> avaialableMove = new List<Vector2Int>();
    private bool White_turn;

    private void Awake()
    {
        GenerateAllTiles(tileSize, tileCountX, tileCountY);
        SpawnAllPieces();
        PositionAllPieces();
        White_turn = true;
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile","Hover","Highlight")))
        {
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }
            else if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = tileMaterial;
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    if((chessPieces[hitPosition.x, hitPosition.y].team==0 && White_turn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !White_turn))
                    {
                        //is it our turn ?
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        //get a list where we can highlight the tiles
                        avaialableMove = currentlyDragging.GetAvailableMoves(ref chessPieces, tileCountX, tileCountY);
                        HighlightTiles();
                    }
                    
                }
            }
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousDrag = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                bool valid = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                RemoveHighlightTiles();

                if (!valid) {
                    currentlyDragging.SetDesiredPosition(GetTileCenter(previousDrag.x, previousDrag.y));
                }
                currentlyDragging = null;
            }
        }
        
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainValidMove(ref avaialableMove, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile"); // : GetComponent<MeshRenderer>().material = tileMaterial;
                currentHover = -Vector2Int.one;
            }
            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetDesiredPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
            }
        }

        // We are dragging
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetDesiredPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }

    // Generate tiles
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountY / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int i = 0; i < tileCountX; i++)
        {
            for (int j = 0; j < tileCountY; j++)
            {
                tiles[i, j] = GenerateSingleTile(tileSize, i, j);
            }
        }
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject($"X{x},Y{y}");
        tileObject.transform.parent = transform;
        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // Spawning of the pieces
    public void SpawnAllPieces()
    {
        chessPieces = new ChessPieces[tileCountX, tileCountY];
        int whiteTeam = 0, blackTeam = 1;

        // White team
        chessPieces[0, 0] = SpawnSingleChessPiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSingleChessPiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSingleChessPiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSingleChessPiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSingleChessPiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSingleChessPiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSingleChessPiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSingleChessPiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < tileCountX; i++)
        {
            chessPieces[i, 1] = SpawnSingleChessPiece(ChessPieceType.Pawn, whiteTeam);
        }

        // Black team
        chessPieces[0, 7] = SpawnSingleChessPiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSingleChessPiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSingleChessPiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSingleChessPiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSingleChessPiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSingleChessPiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSingleChessPiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSingleChessPiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < tileCountX; i++)
        {
            chessPieces[i, 6] = SpawnSingleChessPiece(ChessPieceType.Pawn, blackTeam);
        }
    }

    public ChessPieces SpawnSingleChessPiece(ChessPieceType type, int team)
    {
        ChessPieces cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPieces>();
        
        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = materials[team];
        return cp;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetDesiredPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, spawnPositioningYOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    //highlight
    private void HighlightTiles()
    {
        for (int i = 0; i < avaialableMove.Count; i++)
        {
            tiles[avaialableMove[i].x, avaialableMove[i].y].GetComponent<MeshRenderer>().material = HighlightMaterial;
        }
    }

    //ChexckMate
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }
    private void DisplayVictory(int winningTeam)
    {
        VictoryScreen.SetActive(true);
        VictoryScreen.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }
    public void OnResetButton()
    {
        //ui
        VictoryScreen.SetActive(false);
        VictoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        VictoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        //field reset
        currentlyDragging = null;
        avaialableMove = new List<Vector2Int>();
        //clean up remaining things
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    Destroy(chessPieces[x, y].gameObject);
                }
                chessPieces[x, y] = null;
            }
        }
        for (int i = 0; i < whiteDead.Count; i++)
        {
            Destroy(whiteDead[i].gameObject);
        }
        for (int i = 0; i < blackDead.Count; i++)
        {
            Destroy(blackDead[i].gameObject);
        }
        whiteDead.Clear();
        blackDead.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        White_turn = true;
    }
    public void OnExitButton()
    {
        Application.Quit();
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < avaialableMove.Count; i++)
        {
            tiles[avaialableMove[i].x, avaialableMove[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        avaialableMove.Clear();
    }

    // Operations
    public bool ContainValidMove(ref List<Vector2Int> move,Vector2 pos)
    {
        for (int i = 0; i < move.Count; i++)
        {
            if(move[i].x==pos.x && move[i].y == pos.y)
            {
                return true;
            }
        }
        return false;
    }
    public bool MoveTo(ChessPieces cp, int x, int y)
    {
        if (!ContainValidMove(ref avaialableMove, new Vector2(x, y)))
        {
            return false;
        }
        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);
        if (chessPieces[x, y] != null)
        {
            ChessPieces ocp = chessPieces[x, y];
            if (ocp.team == cp.team)
            {
                return false;
            }

            // If it is enemy team then
            if (ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                {
                    CheckMate(1);
                }
                whiteDead.Add(ocp);
                ocp.SetDesiredScale(Vector3.one * deathSize);
                ocp.SetDesiredPosition(new Vector3(tileSize * 8, yOffset, tileSize * -1) - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.forward * deathSpacing) * whiteDead.Count);
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                {
                    CheckMate(0);
                }
                blackDead.Add(ocp);
                ocp.SetDesiredScale(Vector3.one * deathSize);
                ocp.SetDesiredPosition(new Vector3(tileSize * -1, yOffset, tileSize * 8) - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.back * deathSpacing) * blackDead.Count);
            }
        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        PositionSinglePiece(x, y);
        White_turn = !White_turn;

        return true;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int i = 0; i < tileCountX; i++)
        {
            for (int j = 0; j < tileCountY; j++)
            {
                if (tiles[i, j] == hitInfo)
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        return -Vector2Int.one;
    }
}
