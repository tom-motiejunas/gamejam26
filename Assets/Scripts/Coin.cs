using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    void Start()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.RegisterCoin();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>())
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.CoinCollected();
            }
            Destroy(gameObject);
        }
    }
}
