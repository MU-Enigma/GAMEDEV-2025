using UnityEngine;
using TMPro; // you'll need TextMeshPro for this to work, it's just better than the default text

/// <summary>
/// This script goes on the actual bench object in your scene. Its only job is to detect
/// when the player is close enough to use it, and then tell the UI to open when the player
/// presses the interact key. It's the bouncer at the club door.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class AbilityBench : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the main Ability Bench UI Panel GameObject here.")]
    public AbilityUIController abilityUIController;
    [Tooltip("Drag a TextMeshPro UI element here to show the 'Press E' prompt.")]
    public GameObject interactPrompt;

    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;

    private bool _isPlayerInRange = false;
    private PlayerController _playerController; // cache the player's controller when they get close

    private void Awake()
    {
        // make sure the collider is a trigger, because i don't trust you to remember
        GetComponent<BoxCollider2D>().isTrigger = true;
        // hide the "press E to use" text by default
        if(interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        // if the player is in range AND they press the button...
        if (_isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            // ...tell the UI to open. The UI will handle pausing and disabling the player.
            abilityUIController.OpenMenu(_playerController);
            // hide the prompt since we're now in the menu
            if(interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // did the player just walk up to me?
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
            _playerController = other.GetComponent<PlayerController>();
            // show the "hey, you can use me" text
            if(interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // did the player just walk away like i meant nothing to them?
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            _playerController = null;
            // hide the text. fine. be that way.
            if(interactPrompt != null) interactPrompt.SetActive(false);
        }
    }
}
