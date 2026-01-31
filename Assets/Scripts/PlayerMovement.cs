using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public LayerMask obstacleLayer;

    [Header("Sprites")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    public Vector2 CurrentDirection => _currentInput;

    private Vector3 _targetPosition;
    private bool _isMoving;
    private Vector2 _currentInput;
    private Vector2 _bufferedInput;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic; // Ensure Kinematic
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _targetPosition = transform.position;
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        Move();
    }

    void HandleInput()
    {
        // Simple 4-direction input
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            _bufferedInput = Vector2.up;
            if(spriteUp != null) _spriteRenderer.sprite = spriteUp;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            _bufferedInput = Vector2.down;
            if(spriteDown != null) _spriteRenderer.sprite = spriteDown;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _bufferedInput = Vector2.left;
            if(spriteLeft != null) _spriteRenderer.sprite = spriteLeft;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            _bufferedInput = Vector2.right;
            if(spriteRight != null) _spriteRenderer.sprite = spriteRight;
        }
    }

    void Move()
    {
        if (_isMoving)
        {
            // Move towards target using Rigidbody
            Vector3 newPos = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            // Reached target?
            if (Vector3.Distance(transform.position, _targetPosition) < 0.001f)
            {
                _rb.MovePosition(_targetPosition);
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
            // Continue current direction if key is held
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
