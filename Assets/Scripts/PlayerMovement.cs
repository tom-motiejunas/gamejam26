using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public LayerMask obstacleLayer;

    private Vector3 _targetPosition;
    private bool _isMoving;
    private Vector2 _currentInput;
    private Vector2 _bufferedInput;

    void Start()
    {
        _targetPosition = transform.position;
    }

    void Update()
    {
        HandleInput();
        Move();
    }

    void HandleInput()
    {
        // Simple 4-direction input
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) _bufferedInput = Vector2.up;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) _bufferedInput = Vector2.down;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) _bufferedInput = Vector2.left;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) _bufferedInput = Vector2.right;
    }

    void Move()
    {
        if (_isMoving)
        {
            // Move towards target
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);

            // Reached target?
            if (Vector3.Distance(transform.position, _targetPosition) < 0.001f)
            {
                transform.position = _targetPosition;
                _isMoving = false;
            }
        }
        else
        {
            // Try buffered input first
            if (_bufferedInput != Vector2.zero && CanMove(_bufferedInput))
            {
                _currentInput = _bufferedInput;
                _bufferedInput = Vector2.zero; // Clear buffer
                StartMove(_currentInput);
            }
            // Continue current direction if key is held (optional, or just keep moving if pacman style)
            // Pacman keeps moving until it hits a wall.
            else if (_currentInput != Vector2.zero && CanMove(_currentInput))
            {
                StartMove(_currentInput);
            }
        }
    }

    void StartMove(Vector2 direction)
    {
        _targetPosition = transform.position + (Vector3)direction;
        _isMoving = true;
    }

    bool CanMove(Vector2 direction)
    {
        Vector2 checkPos = (Vector2)transform.position + direction;
        // Check for collider at the center of the target tile.
        // Using a small radius to avoid catching edge overlaps.
        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.1f, obstacleLayer);
        return hit == null;
    }
}
