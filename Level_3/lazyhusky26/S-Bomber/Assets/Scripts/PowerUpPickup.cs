using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerPowerUp playerPowerUp = collision.GetComponent<PlayerPowerUp>();

            if (playerPowerUp != null)
            {
                playerPowerUp.ActivatePowerUp();
                Destroy(gameObject); // Remove power-up from scene
            }
        }
    }
}
