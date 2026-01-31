using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GhostController : MonoBehaviour
{
    public float speed = 4.0f;
    public LayerMask obstacleLayer;
    public Transform player;
    public Faction faction;

    private Vector3 _targetPosition;
    private bool _isMoving;
    private Vector2 _currentDirection;
    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.isKinematic = true; // Ensure it's kinematic
        
        _targetPosition = transform.position;
        
        _currentDirection = GetRandomValidDirection();
        
        // Initialize sprite based on faction
        if (GameController.Instance != null && GameController.Instance.factionGhostSprites.Length > (int)faction)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = GameController.Instance.factionGhostSprites[(int)faction];
            }
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (_isMoving)
        {
            // Use Rigidbody move for physics events
            Vector3 newPos = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.001f)
            {
                // Snap exactly to target
                _rb.MovePosition(_targetPosition);
                transform.position = _targetPosition; // Sync transform just in case
                _isMoving = false;
                ChooseNextDirection();
            }
        }
        else
        {
            ChooseNextDirection();
            if (_currentDirection != Vector2.zero)
            {
                StartMove(_currentDirection);
            }
        }
    }

    void StartMove(Vector2 direction)
    {
        _targetPosition = transform.position + (Vector3)direction;
        _isMoving = true;
    }

    void ChooseNextDirection()
    {
        Vector2[] allDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        System.Collections.Generic.List<Vector2> validMoves = new System.Collections.Generic.List<Vector2>();

        foreach (var dir in allDirections)
        {
            if (CanMove(dir))
            {
                if (dir != -_currentDirection) validMoves.Add(dir);
            }
        }

        if (validMoves.Count == 0 && CanMove(-_currentDirection))
        {
            validMoves.Add(-_currentDirection);
        }

        if (validMoves.Count > 0)
        {
            if (player != null)
            {
                Vector2 bestDir = validMoves[0];
                float minDistance = float.MaxValue;

                foreach (var dir in validMoves)
                {
                    Vector3 nextTilePos = transform.position + (Vector3)dir;
                    float dist = Vector3.Distance(nextTilePos, player.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestDir = dir;
                    }
                }
                _currentDirection = bestDir;
            }
            else
            {
                _currentDirection = validMoves[Random.Range(0, validMoves.Count)];
            }
        }
        else
        {
            _currentDirection = Vector2.zero;
        }
    }

    Vector2 GetRandomValidDirection()
    {
        Vector2[] allDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        System.Collections.Generic.List<Vector2> validMoves = new System.Collections.Generic.List<Vector2>();
        
        foreach (var dir in allDirections)
        {
            if (CanMove(dir)) validMoves.Add(dir);
        }

        if (validMoves.Count > 0)
        {
            return validMoves[Random.Range(0, validMoves.Count)];
        }
        return Vector2.zero;
    }

    bool CanMove(Vector2 direction)
    {
        Vector2 checkPos = (Vector2)transform.position + direction;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.1f, obstacleLayer);
        return hit == null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>())
        {
            if (GameController.Instance.currentDisguise == this.faction)
            {
                Debug.Log("Safe passage granted: Disguise matches faction.");
                return;
            }

            Debug.Log("Player Caught!");
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //TODO: kill player
        }
    }
}
