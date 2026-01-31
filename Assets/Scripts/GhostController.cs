using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class GhostController : MonoBehaviour
{
    public float speed = 4.0f;
    public LayerMask obstacleLayer;
    public Transform player;
    public Faction faction;
    public Tilemap tilemap;
    private Vector3? _wanderTarget;

    [Header("Animations")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    private Vector3 _targetPosition;
    private bool _isMoving;
    private Vector2 _currentDirection;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    public string gameOverScene = "game_over_scene";

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.isKinematic = true; // Ensure it's kinematic
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _targetPosition = transform.position;
        
        _currentDirection = GetRandomValidDirection();
        if (tilemap == null) tilemap = FindObjectOfType<Tilemap>();
        
        // Initialize sprite based on faction (Fallback/Default)
        if (GameController.Instance != null && GameController.Instance.factionGhostSprites.Length > (int)faction)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = GameController.Instance.factionGhostSprites[(int)faction];
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
        UpdateSprite(direction);
    }

    void UpdateSprite(Vector2 direction)
    {
        if (_spriteRenderer == null) return;

        if (direction == Vector2.up && spriteUp != null) _spriteRenderer.sprite = spriteUp;
        else if (direction == Vector2.down && spriteDown != null) _spriteRenderer.sprite = spriteDown;
        else if (direction == Vector2.left && spriteLeft != null) _spriteRenderer.sprite = spriteLeft;
        else if (direction == Vector2.right && spriteRight != null) _spriteRenderer.sprite = spriteRight;
    }

    void ChooseNextDirection()
    {
        if (player == null)
        {
            MoveInternal_Random();
            return;
        }

        // Check disguise interaction: If player has matching disguise, ghost just wanders
        if (GameController.Instance != null && GameController.Instance.currentDisguise == this.faction)
        {
            MoveInternal_Random();
            return;
        }

        switch (faction)
        {
            case Faction.Infinity:
                MoveInternal_Chase();
                break;
            case Faction.Knot:
                MoveInternal_Ambush();
                break;
            case Faction.Bee:
                MoveInternal_Random();
                break;
            case Faction.Pentagram:
                MoveInternal_Stalk();
                break;
            default:
                MoveInternal_Random();
                break;
        }
    }

    // --- Behavior Implementations ---

    // Infinity: Direct Chase
    void MoveInternal_Chase()
    {
        SetDirectionTowards(player.position);
    }

    // Knot: Ambush (Target 4 tiles ahead of player)
    void MoveInternal_Ambush()
    {
        Vector3 target = player.position;
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            target += (Vector3)pm.CurrentDirection * 4.0f;
        }
        SetDirectionTowards(target);
    }

    // Bee: Random Wander (Long Distance)
    void MoveInternal_Random()
    {
        if (tilemap == null)
        {
            MoveInternal_RandomLocal();
            return;
        }

        if (_wanderTarget.HasValue)
        {
            if (Vector3.Distance(transform.position, _wanderTarget.Value) < 0.5f)
            {
                _wanderTarget = null;
            }
        }

        if (!_wanderTarget.HasValue)
        {
            _wanderTarget = GetRandomMapPosition();
        }

        if (!_wanderTarget.HasValue)
        {
            MoveInternal_RandomLocal();
            return;
        }

        SetDirectionTowards(_wanderTarget.Value);

        if (_currentDirection == Vector2.zero)
        {
            _wanderTarget = null;
            MoveInternal_RandomLocal();
        }
    }

    void MoveInternal_RandomLocal()
    {
        var validMoves = GetAvailableMoves();
        if (validMoves.Count > 0)
        {
            // Prefer not to reverse if possible, unless dead end
            var forwardMoves = new System.Collections.Generic.List<Vector2>();
            foreach(var dir in validMoves)
            {
                if (dir != -_currentDirection) forwardMoves.Add(dir);
            }

            if (forwardMoves.Count > 0)
            {
                _currentDirection = forwardMoves[Random.Range(0, forwardMoves.Count)];
            }
            else
            {
                 _currentDirection = validMoves[Random.Range(0, validMoves.Count)];
            }
        }
        else
        {
            // If completely stuck (shouldn't happen with checking reverse), try reverse
             if (CanMove(-_currentDirection))
                 _currentDirection = -_currentDirection;
             else
                 _currentDirection = Vector2.zero;
        }
    }

    Vector3? GetRandomMapPosition()
    {
        if (tilemap == null) return null;
        BoundsInt bounds = tilemap.cellBounds;
        int maxAttempts = 10;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cellPos = new Vector3Int(x, y, 0);

            if (!tilemap.HasTile(cellPos)) continue;
            
            Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
            
            Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.1f, obstacleLayer);
            if (hit == null)
            {
                return worldPos;
            }
        }
        return null;
    }

    // Pentagram: Stalk (Chase if far, Flee/Random if close)
    void MoveInternal_Stalk()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > 5.0f)
        {
            // Far away: Chase
            SetDirectionTowards(player.position);
        }
        else
        {
            // Close: Act erratically (Random for now)
            MoveInternal_Random();
        }
    }

    // --- Helper Methods ---

    void SetDirectionTowards(Vector3 targetPos)
    {
        var validMoves = GetAvailableMoves();
        // Filter out direct reverse moves unless it's the only option
        var forwardMoves = new System.Collections.Generic.List<Vector2>();
        foreach(var m in validMoves)
        {
            if(m != -_currentDirection) forwardMoves.Add(m);
        }

        if (forwardMoves.Count == 0)
        {
            // Dead end, must turn back
            if (validMoves.Contains(-_currentDirection))
                _currentDirection = -_currentDirection;
            else
                _currentDirection = Vector2.zero;
            return;
        }

        // Pick best forward move
        Vector2 bestDir = forwardMoves[0];
        float minDistance = float.MaxValue;

        foreach (var dir in forwardMoves)
        {
            Vector3 nextTilePos = transform.position + (Vector3)dir;
            float d = Vector3.Distance(nextTilePos, targetPos);
            if (d < minDistance)
            {
                minDistance = d;
                bestDir = dir;
            }
        }
        _currentDirection = bestDir;
    }

    System.Collections.Generic.List<Vector2> GetAvailableMoves()
    {
        Vector2[] allDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        System.Collections.Generic.List<Vector2> validMoves = new System.Collections.Generic.List<Vector2>();
        
        foreach (var dir in allDirections)
        {
            if (CanMove(dir)) validMoves.Add(dir);
        }
        return validMoves;
    }

    Vector2 GetRandomValidDirection()
    {
        var validMoves = GetAvailableMoves();
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
            SceneManager.LoadScene(gameOverScene);
            //TODO: kill player
        }
    }
}
