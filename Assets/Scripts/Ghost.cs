using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile;

    public Board board;

    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }

    public Vector3Int[] cells { get; private set; }

    public Vector3Int position { get; private set; }

    private void Awake()
    {
        this.tilemap= GetComponentInChildren<Tilemap>();
        this.cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        this.Clear();
        this.Copy();
        this.Drop();
        this.Set();
    }

    private void Clear()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            this.cells[i] = this.trackingPiece.cells[i];
        }
    }

    private void Drop()
    {
        Vector3Int position = this.trackingPiece.position;

        this.position = position;
        int bottom = -this.board.boardSize.y / 2 - 1;
        this.board.Clear(this.trackingPiece);

        while (position.y >= bottom) {
            if (!this.board.IsValidPosition(this.trackingPiece, position))
            {
                break;
            }
            this.position = position;
            position.y--;
        }

        this.board.Set(this.trackingPiece);
    }

    private void Set()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }
}
