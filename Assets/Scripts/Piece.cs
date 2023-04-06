using System.Collections;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }

    public Vector3Int position { get; set; }

    public TetrominoData data { get; private set; }

    public Vector3Int[] cells { get; private set; }

    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;

    public float lockDelay = 0.5f;

    private const float LONG_PRESS_TIME = 1.0f;
    private float stepTime;
    private float lockTime;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;
        this.cells ??= new Vector3Int[data.cells.Length];

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    public void Update()
    {
        this.board.Clear(this);
        this.lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) ||Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.Move(Vector2Int.right);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.Move(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.HardDrop();
        } else if (Input.GetKeyDown(KeyCode.Q))
        {
            //Counter-Clockwise
            this.Rotate(-1);
        } else if (Input.GetKeyDown(KeyCode.E))
        {
            // Clockwise
            this.Rotate(1);
        } else if (Input.GetKeyDown(KeyCode.Space))
        {
            // TODO: Add hold function
        }

        if (Time.time >= this.stepTime)
        {
            this.Step();
        }

        this.board.Set(this);
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;
        this.Move(Vector2Int.down);

        if (this.lockTime >= this.lockDelay)
        {
            this.Lock();
        }
    }

    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool ifValid = this.board.IsValidPosition(this, newPosition);
        if (ifValid)
        {
            this.position = newPosition;
            this.lockTime = 0f;
        }

        return ifValid;
    }

    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    private void HardDrop()
    {
        while (this.Move(Vector2Int.down))
        {
        }

        this.Lock();
    }

    private void Rotate(int direction)
    {
        int temp = this.rotationIndex;
        this.rotationIndex += direction;
        if (this.rotationIndex < 0)
        {
            this.rotationIndex += 4;
        }
        else if (this.rotationIndex >= 4)
        {
            this.rotationIndex -= 4;
        }

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = temp;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (this.Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        int length = data.wallKicks.GetLength(0);
        if (wallKickIndex < 0)
        {
            wallKickIndex += length;
        }
        if (wallKickIndex >= length)
        {
            wallKickIndex -= length;
        }
        return wallKickIndex;
    }
}
