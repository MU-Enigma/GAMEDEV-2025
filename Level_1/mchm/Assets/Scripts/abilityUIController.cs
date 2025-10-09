using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This is the new bench UI. We're putting Time.timeScale back in, but because the EventSystem
/// shits the bed when time is frozen, we're now bypassing it entirely and handling clicks manually.
/// This is insane, but it's the only way.
/// </summary>
public class AbilityUIController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the UI BUTTONS for your abilities here. ORDER MATTERS. 0=Dash, 1=Teleport, 2=Echo, 3=Blink.")]
    public List<Button> abilityButtons;
    [Tooltip("Drag a button here that will close the menu.")]
    public Button closeButton;

    [Header("Visuals")]
    public Color selectedColor = Color.white;
    public Color deselectedColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    
    private PlayerAbilities _playerAbilities;
    private PlayerController _playerController; // we need this to re-enable it when the menu closes

    void Awake()
    {
        // FUCKING SANITY CHECK. We still need the EventSystem for its raycaster, even if we don't trust its brain.
        if (FindFirstObjectByType<EventSystem>() == null) 
            Debug.LogError("THERE IS NO EVENTSYSTEM IN THE SCENE! OUR MANUAL RAYCASTER IS BLIND! GO TO GameObject > UI > Event System TO CREATE ONE!");
        

        // find the player abilities script. if we can't, we're useless.
        _playerAbilities = FindFirstObjectByType<PlayerAbilities>();
        if (_playerAbilities == null)
        {
            Debug.LogError("ABILITY UI CANNOT FIND PLAYER ABILITIES! PANIC!", gameObject);
            return;
        }

        // We no longer add listeners in Awake, because we are invoking the methods ourselves.
        // This makes the onClick property in the Inspector completely useless. We are gods now.

        // start with the menu hidden, obviously.
        gameObject.SetActive(false);
    }

    // UPDATE IS GONE AGAIN. We live in the timeless void of a coroutine now.
    
    /// <summary>
    /// This is called by the AbilityBench. It's the "showtime" function.
    /// </summary>
    public void OpenMenu(PlayerController playerToDisable)
    {
        // get a reference to the player controller so we can re-enable it later
        _playerController = playerToDisable;
        
        // DISABLE THE PLAYER'S BRAIN (controller) AND THEIR ABILITIES. NO MOVING. NO SHOOTING.
        if(_playerController != null) _playerController.enabled = false;
        if(_playerAbilities != null) _playerAbilities.enabled = false;

        // PAUSE THE FUCKING GAME. FOR REAL THIS TIME.
        Time.timeScale = 0f;
        
        // show the UI panel
        gameObject.SetActive(true);

        // make sure the visuals are correct, showing the currently selected ability
        UpdateVisuals();
        
        // Launch our custom input handler that works outside of time.
        StartCoroutine(ManualInputCoroutine());
    }

    /// <summary>
    /// Called by our manual input handler.
    /// </summary>
    public void CloseMenu()
    {
        // if the menu is already closed, don't do anything. prevents weird bugs.
        if (!gameObject.activeSelf) return;

        // RE-ENABLE THE PLAYER'S BRAIN AND ABILITIES.
        if (_playerController != null) _playerController.enabled = true;
        if(_playerAbilities != null) _playerAbilities.enabled = true;
        
        // UNPAUSE THE FUCKING GAME.
        Time.timeScale = 1f;

        // hide the UI panel and the coroutine will die with it.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// this is our new god. It listens for input when the rest of the universe is frozen.
    /// </summary>
    private IEnumerator ManualInputCoroutine()
    {
        while (true)
        {
            // Check for escape key first.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMenu();
                yield break; // Exit the loop and kill the coroutine.
            }

            // Now, check for the mouse click.
            if (Input.GetMouseButtonDown(0)) HandleManualClick();
            yield return null; // Wait one frame, unscaled.
        }
    }

    /// <summary>
    /// this is the brain transplant. We fire a ray from the mouse, see what we hit, and act accordingly.
    /// do we have to do it like this? i don't know
    /// am i going to find a better way to do it?
    /// ABSOLUTELY not.
    /// </summary>
    private void HandleManualClick()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject clickedObject = results[0].gameObject;

            // Check if we clicked the close button or its child text element.
            if (closeButton != null && (clickedObject == closeButton.gameObject || clickedObject.transform.IsChildOf(closeButton.transform)))
            {
                CloseMenu();
                return; // We're done here.
            }

            // Loop through our ability buttons to see if we hit one of them.
            for (int i = 0; i < abilityButtons.Count; i++)
            {
                if (abilityButtons[i] != null && (clickedObject == abilityButtons[i].gameObject || clickedObject.transform.IsChildOf(abilityButtons[i].transform)))
                {
                    SelectAbility(i);
                    return; // Found it.
                }
            }
        }
    }


    private void SelectAbility(int abilityIndex)
    {
        // tell the player script to equip the new ability
        _playerAbilities.SetAbility((AbilityType)abilityIndex);
        // update the icons to show the new selection
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        AbilityType currentAbility = _playerAbilities.CurrentAbility;

        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (abilityButtons[i] == null) continue;
            // get the image component from the button to change its color
            var buttonImage = abilityButtons[i].GetComponent<Image>();
            if (buttonImage == null) continue; // if there's no image, just skip it

            if (i == (int)currentAbility) buttonImage.color = selectedColor;
            // selected? make it bright.
                
            else buttonImage.color = deselectedColor;
            // not selected? dim it.
        }
    }
}

