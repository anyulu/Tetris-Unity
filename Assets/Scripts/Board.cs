using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrominoData[] tetrominoDatas;

    public Tilemap tilemap { get; private set; }

    public Piece activePiece { get; private set; }

    public Vector3Int spawnPosition;

    public Vector2Int boardSize = new Vector2Int(10, 20);

    private List<int> permutations;

    public RectInt Bound
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y/2);
            return new RectInt(position, this.boardSize);
        }
    }

    public void Awake()
    {
        Screen.SetResolution(960, 1280, false);
        this.permutations = this.GetPermutation();
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        for (int i = 0;i < this.tetrominoDatas.Length; i++)
        {
            this.tetrominoDatas[i].Initialize();
        }
    }

    public void Start()
    {
        this.SpawnPiece();
    }

    public void GameOver()
    {
        this.tilemap.ClearAllTiles();
    }

    public void SpawnPiece()
    {
        int random = this.permutations[0];
        this.permutations.RemoveAt(0);
        if (this.permutations.Count < 7)
        {
            this.permutations.AddRange(this.GetPermutation());
        }
        TetrominoData data = this.tetrominoDatas[random];
        this.activePiece.Initialize(this, this.spawnPosition, data);

        if (this.IsValidPosition(this.activePiece, this.spawnPosition))
        {
            this.Set(this.activePiece);
        }
        else
        {
            this.GameOver();
        }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public void ClearLines()
    {
        int row = this.Bound.yMin;
        while (row < this.Bound.yMax)
        {
            if (this.IsLineFull(row))
            {
                this.LineClear(row);
            } 
            else
            {
                row++;
            }
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        foreach (var cell in piece.cells)
        {
            Vector3Int tilePosition = cell + position;

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }

            if (!this.Bound.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsLineFull(int row)
    {
        for (int col = this.Bound.xMin; col < this.Bound.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    private void LineClear(int row)
    {
        for (int col = this.Bound.xMin; col < this.Bound.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < this.Bound.yMax)
        {
            for (int col = this.Bound.xMin; col < this.Bound.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row, 0);
                Vector3Int abovePosition = new Vector3Int(col, row+1, 0);
                TileBase aboveTile = this.tilemap.GetTile(abovePosition);
                this.tilemap.SetTile(position, aboveTile);
            }

            row++;
        }
    }

    private List<int> GetPermutation()
    {
        List<int> temp = Enumerable.Range(0, this.tetrominoDatas.Length).ToList();
        var rdm = new System.Random();
        return temp.OrderBy(i => rdm.Next()).ToList();
    }
}
