using UnityEngine;

public class SpriteSorter : MonoBehaviour
{
    // Players on 0, Walls on -10, Weapons on -5

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SpriteRenderer>().sortingOrder = -20;    // Player goes from 0 to -20

            Transform weaponSlot = other.transform.Find("WeaponSlot");

            if (weaponSlot != null)
            {
                SpriteRenderer weaponRenderer = weaponSlot.GetComponentInChildren<SpriteRenderer>();
                if (weaponRenderer)
                {
                    weaponRenderer.sortingOrder = -25;                  // Weapon goes from -5 to -25
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SpriteRenderer>().sortingOrder = 0;      // Playuer returns to 0

            Transform weaponSlot = other.transform.Find("WeaponSlot");

            if (weaponSlot != null)
            {
                SpriteRenderer weaponRenderer = weaponSlot.GetComponentInChildren<SpriteRenderer>();
                if (weaponRenderer != null)
                {
                    weaponRenderer.sortingOrder = -5;                   // Weapon returns to -5
                }
            }
        }
    }
}