using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    public Grid tileGrid; // Grid 객체를 저장하기 위한 변수
    public GameObject player; // 플레이어 객체를 저장하기 위한 변수

    private Tilemap tilemap; // Tilemap 객체를 저장하기 위한 변수

    private bool isMoving; // 플레이어 이동 중인지 여부를 저장하기 위한 변수
    private float selectionTimer; // 타일 선택 타이머 변수
    private float selectionTimeLimit = 5f; // 타일 선택 타이머 제한 시간 (5초)

    public int moveCount; // 플레이어 이동 횟수
    public TMP_Text timerText;
    public TMP_Text moveCountText; // 이동 횟수를 표시할 TextMeshProUGUI UI를 연결하기 위한 변수

    public TileBase movableTile; // 이동 가능한 타일에 적용할 타일
    private TileBase originalTile; // 원래 타일을 저장하기 위한 변수

    void Start()
    {
        tilemap = tileGrid.GetComponentInChildren<Tilemap>();
        isMoving = false;
        selectionTimer = 0f;

        
        Vector3Int playerPosition = tileGrid.WorldToCell(player.transform.position);
        UpdateTimerText(); // 시작 시 타이머 UI 업데이트
        UpdateMoveCountText(); // 시작 시 이동 횟수 UI 업데이트
        HighlightMovableTiles(playerPosition); // 시작 시 이동 가능한 타일들 강조
    }
    void Update()
    {
        // 게임 오버 상태에서는 동작하지 않도록 처리
        if (moveCount <= 0)
        {
            return;
        }

        // 마우스 왼쪽 버튼을 클릭했을 때
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

        // 타이머를 실시간으로 증가시킴
        if (!isMoving)
        {
            selectionTimer += Time.deltaTime;
            UpdateTimerText(); // 타이머 UI 업데이트

            if (selectionTimer >= selectionTimeLimit)
            {
                // 타이머가 만료되면 인접한 타일 중 랜덤하게 선택하여 이동
                Vector3Int randomCell = GetRandomAdjacentCell(tileGrid.WorldToCell(player.transform.position));
                MovePlayerToCell(randomCell);
                ResetSelection();
            }
        }
    }
    bool IsTileNearPlayer(Vector3Int cellPosition)
    {
        // 플레이어가 선택한 타일이 자신이 위치한 타일 주변의 타일인지 확인
        Vector3Int playerPosition = tileGrid.WorldToCell(player.transform.position);
        int distanceX = Mathf.Abs(cellPosition.x - playerPosition.x);
        int distanceY = Mathf.Abs(cellPosition.y - playerPosition.y);

        return (distanceX <= 1 && distanceY <= 1);
    }

    // 플레이어 이동 함수
    void MovePlayerToCell(Vector3Int cellPosition)
    {
        // 타일맵에서 해당 위치의 타일을 가져옴
        TileBase tile = tilemap.GetTile(cellPosition);

        // 타일이 없는 경우 (null) 이동을 제한
        if (tile == null)
        {
            Debug.Log("Cannot move to an empty tile!");
            return;
        }

        // 상하좌우 타일만 이동 가능하도록 확인
        Vector3Int playerPosition = tileGrid.WorldToCell(player.transform.position);
        int distanceX = Mathf.Abs(cellPosition.x - playerPosition.x);
        int distanceY = Mathf.Abs(cellPosition.y - playerPosition.y);
        if ((distanceX == 1 && distanceY == 0) || (distanceX == 0 && distanceY == 1))
        {
            // 타일이 있는 경우 이동 가능
            Vector3 targetPosition = tileGrid.GetCellCenterWorld(cellPosition);
            Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
            rb2d.MovePosition(targetPosition);
            isMoving = true;

            // 이동할 때마다 moveCount 감소
            moveCount--;

            // 이동 가능한 타일들 다시 강조
            HighlightMovableTiles(cellPosition);

            // 이동 횟수 UI 업데이트
            UpdateMoveCountText();

            // 타이머 UI 업데이트
            UpdateTimerText();
        }
        else
        {
            Debug.Log("Cannot move to this tile!");
        }
    }

    // 선택 시간 초기화
    void ResetSelection()
    {
        isMoving = false;
        selectionTimer = 0f;
    }

    // 인접한 타일 정보 가져오기
    Vector3Int GetRandomAdjacentCell(Vector3Int cellPosition)
    {
        // 현재 위치한 타일 주변의 상하좌우 타일만 선택하여 반환
        List<Vector3Int> adjacentCells = new List<Vector3Int>
        {
            cellPosition + new Vector3Int(0, 1, 0), // 위쪽 타일
            cellPosition + new Vector3Int(0, -1, 0), // 아래쪽 타일
            cellPosition + new Vector3Int(-1, 0, 0), // 왼쪽 타일
            cellPosition + new Vector3Int(1, 0, 0), // 오른쪽 타일
        };

        List<Vector3Int> availableCells = new List<Vector3Int>();

        // 상하좌우 타일 중에 선택 가능한 타일(비어있지 않은 타일)을 필터링하여 리스트에 추가
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
            // 선택 가능한 타일들 중 랜덤하게 선택하여 반환
            return availableCells[Random.Range(0, availableCells.Count)];
        }

        // 이동 가능한 타일이 없는 경우 현재 타일 반환
        return cellPosition;
    }

    // 타이머 UI 업데이트
    void UpdateTimerText()
    {
        if (timerText != null)
        {
            float remainingTime = Mathf.Max(0f, selectionTimeLimit - selectionTimer);
            timerText.text = "Time: " + remainingTime.ToString("F1"); // 소수점 첫째자리까지 표시
        }
    }

    // 이동 횟수 UI 업데이트
    void UpdateMoveCountText()
    {
        if (moveCountText != null)
        {
            moveCountText.text = "Moves: " + moveCount.ToString(); // 이동 횟수를 UI에 표시
        }
    }

    void HighlightMovableTiles(Vector3Int playerPosition)
    {
        // 모든 타일을 원래 타일로 초기화 (강조를 제거)
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

        // 현재 위치한 타일 주변의 상하좌우 타일을 가져옴
        List<Vector3Int> adjacentCells = new List<Vector3Int>
        {
            playerPosition + new Vector3Int(0, 1, 0), // 위쪽 타일
            playerPosition + new Vector3Int(0, -1, 0), // 아래쪽 타일
            playerPosition + new Vector3Int(-1, 0, 0), // 왼쪽 타일
            playerPosition + new Vector3Int(1, 0, 0), // 오른쪽 타일
        };

        // 상하좌우 타일 중에 선택 가능한 타일(비어있지 않은 타일)을 필터링하여 리스트에 추가
        foreach (Vector3Int adjacentCell in adjacentCells)
        {
            TileBase tile = tilemap.GetTile(adjacentCell);
            if (tile == null)
            {
                continue;
            }

            // 원래 타일 저장
            if (originalTile == null)
            {
                originalTile = tile;
            }

            // 이동 가능한 타일의 색깔을 변경 (movableTile로 대체)
            tilemap.SetTile(adjacentCell, movableTile);
        }
    }

}
