using UnityEngine;
using System; // Required for 'Action'

public class ScoreManager : MonoBehaviour
{
    // --- The Singleton Pattern ---
    // This 'instance' variable can be accessed from any script
    // using ScoreManager.instance
    public static ScoreManager instance;
    // ---------------------------

    private int enemiesKilled = 0;

    // This 'event' will notify the UI (or other scripts)
    // whenever the kill count changes.
    public static event Action<int> OnKillsChanged;

    void Awake()
    {
        // Set up the Singleton
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: un-comment if you change scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this method from your enemy script when an enemy dies.
    /// </summary>
    public void AddKill()
    {
        enemiesKilled++;
        
        // Log to console
        Debug.Log("Enemies Killed: " + enemiesKilled);

        // Notify any listeners (like the UI) that the count has changed
        OnKillsChanged?.Invoke(enemiesKilled);
    }

    // Optional: A way to get the current kill count
    public int GetKillCount()
    {
        return enemiesKilled;
    }
}
