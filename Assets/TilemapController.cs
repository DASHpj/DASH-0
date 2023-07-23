using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    public Grid tileGrid; // Grid ��ü�� �����ϱ� ���� ����
    public GameObject player; // �÷��̾� ��ü�� �����ϱ� ���� ����

    private Tilemap tilemap; // Tilemap ��ü�� �����ϱ� ���� ����

    private bool isMoving; // �÷��̾� �̵� ������ ���θ� �����ϱ� ���� ����
    private float selectionTimer; // Ÿ�� ���� Ÿ�̸� ����
    private float selectionTimeLimit = 5f; // Ÿ�� ���� Ÿ�̸� ���� �ð� (5��)

    public int moveCount; // �÷��̾� �̵� Ƚ��
    public TMP_Text timerText;
    public TMP_Text moveCountText; // �̵� Ƚ���� ǥ���� TextMeshProUGUI UI�� �����ϱ� ���� ����

    public TileBase movableTile; // �̵� ������ Ÿ�Ͽ� ������ Ÿ��
    private TileBase originalTile; // ���� Ÿ���� �����ϱ� ���� ����

    void Start()
    {
        tilemap = tileGrid.GetComponentInChildren<Tilemap>();
        isMoving = false;
        selectionTimer = 0f;

        
        Vector3Int playerPosition = tileGrid.WorldToCell(player.transform.position);
        UpdateTimerText(); // ���� �� Ÿ�̸� UI ������Ʈ
        UpdateMoveCountText(); // ���� �� �̵� Ƚ�� UI ������Ʈ
        HighlightMovableTiles(playerPosition); // ���� �� �̵� ������ Ÿ�ϵ� ����
    }
    void Update()
    {
        // ���� ���� ���¿����� �������� �ʵ��� ó��
        if (moveCount <= 0)
        {
            return;
        }

        // ���콺 ���� ��ư�� Ŭ������ ��
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = tileGrid.WorldToCell(clickPosition);

            if (IsTileNearPlayer(cellPosition))
            {
                Vector3 targetPosition = tileGrid.GetCellCenterWorld(cellPosition);
                MovePlayerToCell(cellPosition);
                ResetSelection();
            }
        }

        // Ÿ�̸Ӹ� �ǽð����� ������Ŵ
        if (!isMoving)
        {
            selectionTimer += Time.deltaTime;
            UpdateTimerText(); // Ÿ�̸� UI ������Ʈ

            if (selectionTimer >= selectionTimeLimit)
            {
                // Ÿ�̸Ӱ� ����Ǹ� ������ Ÿ�� �� �����ϰ� �����Ͽ� �̵�
                Vector3Int randomCell = GetRandomAdjacentCell(tileGrid.WorldToCell(player.transform.position));
                MovePlayerToCell(randomCell);
                ResetSelection();
            }
        }
    }
    bool IsTileNearPlayer(Vector3Int cellPosition)
    {
        // �÷��̾ ������ Ÿ���� �ڽ��� ��ġ�� Ÿ�� �ֺ��� Ÿ������ Ȯ��
        Vector3Int playerPosition = tileGrid.WorldToCell(player.transform.position);
        int distanceX = Mathf.Abs(cellPosition.x - playerPosition.x);
        int distanceY = Mathf.Abs(cellPosition.y - playerPosition.y);

        return (distanceX <= 1 && distanceY <= 1);
    }

    // �÷��̾� �̵� �Լ�
    void MovePlayerToCell(Vector3Int cellPosition)
    {
        // Ÿ�ϸʿ��� �ش� ��ġ�� Ÿ���� ������
        TileBase tile = tilemap.GetTile(cellPosition);

        // Ÿ���� ���� ��� (null) �̵��� ����
        if (tile == null)
        {
            Debug.Log("Cannot move to an empty tile!");
            return;
        }

        // �����¿� Ÿ�ϸ� �̵� �����ϵ��� Ȯ��
        Vector3Int playerPosition = tileGrid.WorldToCell(player.transform.position);
        int distanceX = Mathf.Abs(cellPosition.x - playerPosition.x);
        int distanceY = Mathf.Abs(cellPosition.y - playerPosition.y);
        if ((distanceX == 1 && distanceY == 0) || (distanceX == 0 && distanceY == 1))
        {
            // Ÿ���� �ִ� ��� �̵� ����
            Vector3 targetPosition = tileGrid.GetCellCenterWorld(cellPosition);
            Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
            rb2d.MovePosition(targetPosition);
            isMoving = true;

            // �̵��� ������ moveCount ����
            moveCount--;

            // �̵� ������ Ÿ�ϵ� �ٽ� ����
            HighlightMovableTiles(cellPosition);

            // �̵� Ƚ�� UI ������Ʈ
            UpdateMoveCountText();

            // Ÿ�̸� UI ������Ʈ
            UpdateTimerText();
        }
        else
        {
            Debug.Log("Cannot move to this tile!");
        }
    }

    // ���� �ð� �ʱ�ȭ
    void ResetSelection()
    {
        isMoving = false;
        selectionTimer = 0f;
    }

    // ������ Ÿ�� ���� ��������
    Vector3Int GetRandomAdjacentCell(Vector3Int cellPosition)
    {
        // ���� ��ġ�� Ÿ�� �ֺ��� �����¿� Ÿ�ϸ� �����Ͽ� ��ȯ
        List<Vector3Int> adjacentCells = new List<Vector3Int>
        {
            cellPosition + new Vector3Int(0, 1, 0), // ���� Ÿ��
            cellPosition + new Vector3Int(0, -1, 0), // �Ʒ��� Ÿ��
            cellPosition + new Vector3Int(-1, 0, 0), // ���� Ÿ��
            cellPosition + new Vector3Int(1, 0, 0), // ������ Ÿ��
        };

        List<Vector3Int> availableCells = new List<Vector3Int>();

        // �����¿� Ÿ�� �߿� ���� ������ Ÿ��(������� ���� Ÿ��)�� ���͸��Ͽ� ����Ʈ�� �߰�
        foreach (Vector3Int adjacentCell in adjacentCells)
        {
            TileBase tile = tilemap.GetTile(adjacentCell);
            if (tile != null)
            {
                availableCells.Add(adjacentCell);
            }
        }

        if (availableCells.Count > 0)
        {
            // ���� ������ Ÿ�ϵ� �� �����ϰ� �����Ͽ� ��ȯ
            return availableCells[Random.Range(0, availableCells.Count)];
        }

        // �̵� ������ Ÿ���� ���� ��� ���� Ÿ�� ��ȯ
        return cellPosition;
    }

    // Ÿ�̸� UI ������Ʈ
    void UpdateTimerText()
    {
        if (timerText != null)
        {
            float remainingTime = Mathf.Max(0f, selectionTimeLimit - selectionTimer);
            timerText.text = "Time: " + remainingTime.ToString("F1"); // �Ҽ��� ù°�ڸ����� ǥ��
        }
    }

    // �̵� Ƚ�� UI ������Ʈ
    void UpdateMoveCountText()
    {
        if (moveCountText != null)
        {
            moveCountText.text = "Moves: " + moveCount.ToString(); // �̵� Ƚ���� UI�� ǥ��
        }
    }

    void HighlightMovableTiles(Vector3Int playerPosition)
    {
        // ��� Ÿ���� ���� Ÿ�Ϸ� �ʱ�ȭ (������ ����)
        if (originalTile != null)
        {
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = tilemap.GetTilesBlock(bounds);
            foreach (var tilePosition in bounds.allPositionsWithin)
            {
                TileBase tile = allTiles[tilePosition.x - bounds.x + (tilePosition.y - bounds.y) * bounds.size.x];
                if (tile == movableTile)
                {
                    tilemap.SetTile(tilePosition, originalTile);
                }
            }
        }

        // ���� ��ġ�� Ÿ�� �ֺ��� �����¿� Ÿ���� ������
        List<Vector3Int> adjacentCells = new List<Vector3Int>
        {
            playerPosition + new Vector3Int(0, 1, 0), // ���� Ÿ��
            playerPosition + new Vector3Int(0, -1, 0), // �Ʒ��� Ÿ��
            playerPosition + new Vector3Int(-1, 0, 0), // ���� Ÿ��
            playerPosition + new Vector3Int(1, 0, 0), // ������ Ÿ��
        };

        // �����¿� Ÿ�� �߿� ���� ������ Ÿ��(������� ���� Ÿ��)�� ���͸��Ͽ� ����Ʈ�� �߰�
        foreach (Vector3Int adjacentCell in adjacentCells)
        {
            TileBase tile = tilemap.GetTile(adjacentCell);
            if (tile == null)
            {
                continue;
            }

            // ���� Ÿ�� ����
            if (originalTile == null)
            {
                originalTile = tile;
            }

            // �̵� ������ Ÿ���� ������ ���� (movableTile�� ��ü)
            tilemap.SetTile(adjacentCell, movableTile);
        }
    }

}
