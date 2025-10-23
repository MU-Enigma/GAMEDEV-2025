using UnityEngine;

public class PlayerSummon : MonoBehaviour
{
    public GameObject singer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (singer != null && !singer.activeSelf)
            {
                singer.SetActive(true); 
            }
        }
    }
}
