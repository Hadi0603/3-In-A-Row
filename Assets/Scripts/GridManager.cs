using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;


public class GridManager : MonoBehaviour
{
    [FormerlySerializedAs("ballPrefabs")] [SerializeField] private GameObject[] emojiPrefabs;
    [SerializeField] GameObject[] aiEmojiPrefabs;
    public Transform playerSpawnPoint;
    public Transform aiSpawnPoint;
    public int rows = 6;
    public int columns = 7;

    private int[,] grid;
    public Transform[,] gridPositions;
    public Transform gridParent;
    [SerializeField] GameObject uiManager;
     
    private int playerEmojiIndex;
    private bool aiSelected = false;
    private int aiEmojiIndex;
    private bool isMoving = false;
    private bool isAITurn = false;
    private bool isSwiping = false;
    private Vector2 startTouchPosition;
    private GameObject currentBall;
    private int currentColumn = -1;

    void Start()
    {
        grid = new int[rows, columns];
        gridPositions = new Transform[rows, columns];
        FillGridPositions();

        SpawnPlayerBall();
        playerEmojiIndex = PlayerPrefs.GetInt("selectedEmoji", 0);
        playerEmojiIndex = Mathf.Clamp(playerEmojiIndex, 0, emojiPrefabs.Length - 1);
    }

    void Update()
    {
        if (isMoving || isAITurn || currentBall == null) return;

#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // The mouse click is over a UI element, so do nothing
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            isSwiping = true;
            // Immediately move ball to swipe start position in X-axis
            Vector3 worldStartPos = Camera.main.ScreenToWorldPoint(new Vector3(startTouchPosition.x, 0, Camera.main.nearClipPlane));
            float clampedX = Mathf.Clamp(worldStartPos.x, gridPositions[0, 0].position.x, gridPositions[0, columns - 1].position.x);
            currentBall.transform.position = new Vector3(clampedX, currentBall.transform.position.y, currentBall.transform.position.z);
            UpdateCurrentColumn();
        }
        else if (Input.GetMouseButton(0) && isSwiping)
        {
            Vector2 currentTouchPosition = Input.mousePosition;
            float deltaX = currentTouchPosition.x - startTouchPosition.x;

            if (Mathf.Abs(deltaX) > 1f)
            {
                Vector3 worldDelta = Camera.main.ScreenToWorldPoint(new Vector3(currentTouchPosition.x, 0, Camera.main.nearClipPlane)) -
                                     Camera.main.ScreenToWorldPoint(new Vector3(startTouchPosition.x, 0, Camera.main.nearClipPlane));

                float newX = currentBall.transform.position.x + worldDelta.x;

                float leftMost = gridPositions[0, 0].position.x;
                float rightMost = gridPositions[0, columns - 1].position.x;
                currentBall.transform.position = new Vector3(Mathf.Clamp(newX, leftMost, rightMost), currentBall.transform.position.y, currentBall.transform.position.z);

                UpdateCurrentColumn();
                startTouchPosition = currentTouchPosition;
            }
        }
        else if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            isSwiping = false;

            SnapBallToColumn();

            if (currentColumn != -1)
            {
                TryMoveBallToColumn(currentColumn, 1);
            }
            else
            {
                ResetPlayerBallPosition();
            }
        }
    }
    void SnapBallToColumn()
    {
        if (currentColumn != -1)
        {
            Vector3 snapPos = currentBall.transform.position;
            snapPos.x = gridPositions[0, currentColumn].position.x;
            currentBall.transform.position = snapPos;
        }
    }


    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                // The touch is over a UI element, so do nothing
                return;
            }
            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
                isSwiping = true;
            }
            else if (touch.phase == TouchPhase.Moved && isSwiping)
            {
                Vector2 currentTouchPosition = touch.position;
                float deltaX = currentTouchPosition.x - startTouchPosition.x;

                if (Mathf.Abs(deltaX) > 1f)
                {
                    // Calculate the desired world position based on the swipe
                    Vector3 worldStartPos = Camera.main.ScreenToWorldPoint(startTouchPosition);
                    Vector3 worldCurrentPos = Camera.main.ScreenToWorldPoint(currentTouchPosition);
                    float deltaXWorld = Camera.main.ScreenToWorldPoint(new Vector3(deltaX, 0f, Mathf.Abs(Camera.main.transform.position.z))).x 
                                        - Camera.main.ScreenToWorldPoint(Vector3.zero).x;

                    float newXPosition = currentBall.transform.position.x + deltaXWorld;


                    // Clamp the horizontal movement within the bounds of the grid columns
                    float leftMost = gridPositions[0, 0].position.x;
                    float rightMost = gridPositions[0, columns - 1].position.x;
                    currentBall.transform.position = new Vector3(Mathf.Clamp(newXPosition, leftMost, rightMost), currentBall.transform.position.y, currentBall.transform.position.z);

                    // Update the current column the ball is above
                    UpdateCurrentColumn();
                    startTouchPosition = currentTouchPosition; // Update for continuous swiping
                }
            }
            else if (touch.phase == TouchPhase.Ended && isSwiping)
            {
                isSwiping = false;
                if (currentColumn != -1)
                {
                    TryMoveBallToColumn(currentColumn, 1);
                }
                else
                {
                    ResetPlayerBallPosition();
                }
            }
            else if (touch.phase == TouchPhase.Canceled && isSwiping)
            {
                isSwiping = false;
                ResetPlayerBallPosition();
            }
        }
    }

    void UpdateCurrentColumn()
    {
        if (currentBall == null) return;

        float ballX = currentBall.transform.position.x;
        float closestDistance = Mathf.Infinity;
        currentColumn = -1;

        for (int c = 0; c < columns; c++)
        {
            float columnX = gridPositions[0, c].position.x;
            float distance = Mathf.Abs(ballX - columnX);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentColumn = c;
            }
        }
    }

    void ResetPlayerBallPosition()
    {
        if (currentBall != null)
        {
            currentBall.transform.position = playerSpawnPoint.position;
            currentColumn = -1;
        }
    }

    public new void TryMoveBallToColumn(int col, int playerId) // 'new' to avoid hiding warning
    {
        if (isMoving) return;
        if (playerId == 2 && currentBall != null) // If it's the AI's turn, immediately align the ball
        {
            Vector3 aiBallPos = currentBall.transform.position;
            aiBallPos.x = gridPositions[0, col].position.x;
            currentBall.transform.position = aiBallPos;
        }

        for (int row = rows - 1; row >= 0; row--)
        {
            if (grid[row, col] == 0)
            {
                if (gridPositions[row, col] == null)
                {
                    Debug.LogError($"gridPositions[{row},{col}] is null!");
                    return;
                }

                isMoving = true;
                StartCoroutine(MoveAndHandleTurn(gridPositions[row, col].position, row, col, playerId));
                currentColumn = -1; // Reset current column after dropping
                break;
            }
        }
        // If the column is full, you might want to handle this (e.g., visual feedback).
        if (!isMoving && playerId == 1)
        {
            ResetPlayerBallPosition(); // Return the ball if the column is full
        }
    }

    IEnumerator MoveAndHandleTurn(Vector3 targetPosition, int row, int col, int playerId)
    {
        GameObject ball = currentBall;
        currentBall = null; // Clear currentBall reference during movement
        float duration = 0.4f; // Adjusted duration for quicker movement
        float time = 0f;
        Vector3 start = ball.transform.position;
        Vector3 end = targetPosition;

        while (time < duration)
        {
            ball.transform.position = Vector3.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        ball.transform.position = end;

        grid[row, col] = playerId;

        if (CheckWin(row, col, playerId))
        {
            if (playerId == 1)
                uiManager.GetComponent<UIManager>().TriggerGameWon();
            else
                uiManager.GetComponent<UIManager>().GameOver();

            Debug.Log(playerId == 1 ? "Player Wins!" : "AI Wins!");
            yield break;
        }

        yield return new WaitForSeconds(0.1f);
        isMoving = false;

        // Next turn
        if (playerId == 1)
        {
            isAITurn = true;
            yield return new WaitForSeconds(0.5f);
            AiTurn();
        }
        else
        {
            SpawnPlayerBall();
            isAITurn = false;
        }
    }

    void SpawnPlayerBall()
    {
        int selectedIndex = PlayerPrefs.GetInt("selectedEmoji", 0);
        selectedIndex = Mathf.Clamp(selectedIndex, 0, emojiPrefabs.Length - 1);

        currentBall = Instantiate(emojiPrefabs[selectedIndex], playerSpawnPoint.position, Quaternion.identity);
    }
    
    void SpawnAIBall()
    {
        //currentBall = Instantiate(aiBallPrefab, aiSpawnPoint.position, Quaternion.identity);

        if (aiSelected == false)
        {
            int randomIndex = Random.Range(0, aiEmojiPrefabs.Length);
            currentBall = Instantiate(aiEmojiPrefabs[randomIndex], aiSpawnPoint.position, Quaternion.identity);
            aiSelected = true;
            aiEmojiIndex = randomIndex;
        }
        else if (aiSelected == true)
        {
            currentBall = Instantiate(aiEmojiPrefabs[aiEmojiIndex], aiSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("No different AI emoji available!");
        }
    }

    void AiTurn()
    {
        int col = GetRandomValidColumn();
        if (col != -1)
        {
            SpawnAIBall();
            TryMoveBallToColumn(col, 2);
        }
        else
        {
            Debug.Log("Draw! No valid moves.");
        }
    }

    int GetRandomValidColumn()
    {
        System.Collections.Generic.List<int> validCols = new();

        for (int c = 0; c < columns; c++)
        {
            if (grid[0, c] == 0)
                validCols.Add(c);
        }

        if (validCols.Count == 0)
            return -1;

        return validCols[Random.Range(0, validCols.Count)];
    }

    bool CheckWin(int row, int col, int playerId)
    {
        if (CountConsecutive(row, col, 0, 1, playerId) + CountConsecutive(row, col, 0, -1, playerId) - 1 >= 4)
            return true;
        if (CountConsecutive(row, col, 1, 0, playerId) + CountConsecutive(row, col, -1, 0, playerId) - 1 >= 4)
            return true;
        if (CountConsecutive(row, col, 1, 1, playerId) + CountConsecutive(row, col, -1, -1, playerId) - 1 >= 4)
            return true;
        if (CountConsecutive(row, col, 1, -1, playerId) + CountConsecutive(row, col, -1, 1, playerId) - 1 >= 4)
            return true;

        return false;
    }

    int CountConsecutive(int row, int col, int rowDir, int colDir, int playerId)
    {
        int count = 0;

        while (row >= 0 && row < rows && col >= 0 && col < columns && grid[row, col] == playerId)
        {
            count++;
            row += rowDir;
            col += colDir;
        }

        return count;
    }

    void FillGridPositions()
    {
        foreach (Transform cell in gridParent)
        {
            string[] parts = cell.name.Split('_');
            int row = int.Parse(parts[1]);
            int col = int.Parse(parts[2]);
            gridPositions[row, col] = cell;
        }
    }
}