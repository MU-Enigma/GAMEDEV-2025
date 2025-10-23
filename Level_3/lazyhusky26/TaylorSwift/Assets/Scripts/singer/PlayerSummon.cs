using UnityEngine;

public class PlayerSummon : MonoBehaviour
{
    public GameObject singer;  // Assign the child SingerGuy here in the Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (singer != null && !singer.activeSelf)
            {
                singer.SetActive(true); // show him
            }
        }
    }
}
