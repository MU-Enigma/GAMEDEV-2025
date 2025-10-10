using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial UI")]
    public GameObject tutorialPanel;        // The UI panel containing tutorial text
    public TextMeshProUGUI tutorialText;    // The text component for messages

    [Header("Tutorial Settings")]
    [Tooltip("Time required for movement input before advancing to next step")]
    public float movementDetectionTime = 1.5f;
    [Tooltip("Number of dashes required before advancing to next step")]
    public int dashesRequired = 3;
    [Tooltip("Number of charge dashes required before advancing to next step")]
    public int chargeDashesRequired = 2;
    [Tooltip("Number of chain dashes required before advancing to next step")]
    public int chainDashesRequired = 3;
    [Tooltip("Minimum time space must be held to count as a charge dash")]
    public float minChargeTime = 0.5f;
    [Tooltip("Time X must be held to remove markers")]
    public float markerRemoveHoldTime = 0.5f;

    [Header("Debug Spawning")]
    [Tooltip("Feather prefab to spawn with P+1")]
    public GameObject featherPrefab;
    [Tooltip("Chain dash upgrade token prefab to spawn with P+2")]
    public GameObject chainDashTokenPrefab;

    private bool tutorialActive = false;
    private bool waitingForInitialChoice = true;
    private bool checkingForMovement = false;
    private bool checkingForDash = false;
    private bool checkingForChargeDash = false;
    private bool checkingForDashCancel = false;
    private bool checkingForMouseMove = false;
    private bool checkingForMarker = false;
    private bool checkingForFlyToMarker = false;
    private bool checkingForSecondMarker = false;
    private bool checkingForRemoveMarker = false;
    private bool checkingForDebugFeather = false;
    private bool checkingForDebugToken = false;
    private bool checkingForChainDash = false;

    private float movementTimer = 0f;
    private int dashCount = 0;
    private int chargeDashCount = 0;
    private int chainDashCount = 0;
    private float spaceHoldStartTime = 0f;
    private bool isHoldingSpace = false;
    private float xHoldTimer = 0f;
    private PlayerController player;

    // Tutorial states
    private enum TutorialStep
    {
        InitialChoice,
        MovementInstruction,
        DashInstruction,
        ChargeDashInstruction,
        DashCancel,
        MoveMouse,
        PlaceMarker,
        FlyToMarker,
        PlaceSecondMarker,
        HoldXRemoveMarker,
        DebugFeatherControl,
        DebugTokenControl,
        ChainDashInstruction,
        TutorialComplete
    }
    private TutorialStep currentStep = TutorialStep.InitialChoice;

    void Start()
    {
        player = FindFirstObjectByType<PlayerController>();
        ShowTutorialChoice();
    }

    void Update()
    {
        // Debug menu controls (always active)
        HandleDebugInput();

        if (tutorialActive)
        {
            if (waitingForInitialChoice)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    StartTutorial();
                }
                else if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.T))
                {
                    SkipTutorial();
                }
            }
            else if (checkingForMovement)
            {
                CheckMovementInput();
            }
            else if (checkingForDash)
            {
                CheckDashInput();
            }
            else if (checkingForChargeDash)
            {
                CheckChargeDashInput();
            }
            else if (checkingForDashCancel)
            {
                CheckDashCancelInput();
            }
            else if (checkingForMouseMove)
            {
                CheckMouseMoveInput();
            }
            else if (checkingForMarker)
            {
                CheckMarkerPlaceInput();
            }
            else if (checkingForFlyToMarker)
            {
                CheckFlyToMarkerInput();
            }
            else if (checkingForSecondMarker)
            {
                CheckSecondMarkerInput();
            }
            else if (checkingForRemoveMarker)
            {
                CheckRemoveMarkerInput();
            }
            else if (checkingForDebugFeather)
            {
                CheckDebugFeatherInput();
            }
            else if (checkingForDebugToken)
            {
                CheckDebugTokenInput();
            }
            else if (checkingForChainDash)
            {
                CheckChainDashInput();
            }
        }
    }

    void HandleDebugInput()
    {
        // P+1: Spawn feathers
        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnFeather();
        }
        
        // P+2: Spawn chain dash upgrade tokens
        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnChainDashToken();
        }
    }

    void SpawnFeather()
    {
        if (featherPrefab != null && player != null)
        {
            Vector3 spawnPos = player.transform.position + Vector3.up * 2f + Random.insideUnitSphere * 2f;
            Instantiate(featherPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Feather spawned!");
        }
        else
        {
            Debug.Log("Feather prefab not assigned or player not found!");
        }
    }

    void SpawnChainDashToken()
    {
        if (chainDashTokenPrefab != null && player != null)
        {
            Vector3 spawnPos = player.transform.position + Vector3.up * 2f + Random.insideUnitSphere * 2f;
            Instantiate(chainDashTokenPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Chain dash token spawned!");
        }
        else
        {
            Debug.Log("Chain dash token prefab not assigned or player not found!");
        }
    }

    void ShowTutorialChoice()
    {
        tutorialActive = true;
        waitingForInitialChoice = true;
        currentStep = TutorialStep.InitialChoice;
        
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
        
        if (tutorialText != null)
            tutorialText.text = "Press T to start tutorial\nPress any other key to skip";
        
        Time.timeScale = 0f;
    }

    void StartTutorial()
    {
        waitingForInitialChoice = false;
        currentStep = TutorialStep.MovementInstruction;
        
        if (tutorialText != null)
            tutorialText.text = "Use WASD keys to move around";
        
        Debug.Log("Tutorial started - Movement phase!");
        
        Time.timeScale = 1f;
        checkingForMovement = true;
        movementTimer = 0f;
    }

    void CheckMovementInput()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                       Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                       Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (isMoving)
        {
            movementTimer += Time.deltaTime;
            if (movementTimer >= movementDetectionTime)
            {
                AdvanceToNextStep();
            }
        }
        else
        {
            movementTimer = 0f;
        }
    }

    void CheckDashInput()
    {
        bool dashPressed = Input.GetButtonDown("Dash") || Input.GetKeyDown(KeyCode.Space);

        if (dashPressed)
        {
            dashCount++;
            int dashesRemaining = dashesRequired - dashCount;
            
            Debug.Log($"Dash count: {dashCount}/{dashesRequired}");
            
            if (dashesRemaining > 0)
            {
                if (tutorialText != null)
                    tutorialText.text = $"Press SPACE to dash ({dashesRemaining} more)";
            }
            
            if (dashCount >= dashesRequired)
            {
                AdvanceToNextStep();
            }
        }
    }

    void CheckChargeDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceHoldStartTime = Time.time;
            isHoldingSpace = true;
        }
        
        if (Input.GetKeyUp(KeyCode.Space) && isHoldingSpace)
        {
            float holdDuration = Time.time - spaceHoldStartTime;
            isHoldingSpace = false;
            
            if (holdDuration >= minChargeTime)
            {
                chargeDashCount++;
                int chargeDashesRemaining = chargeDashesRequired - chargeDashCount;
                
                Debug.Log($"Charge dash count: {chargeDashCount}/{chargeDashesRequired} (held for {holdDuration:F2}s)");
                
                if (chargeDashesRemaining > 0)
                {
                    if (tutorialText != null)
                        tutorialText.text = $"Hold SPACE to charge dash ({chargeDashesRemaining} more)";
                }
                
                if (chargeDashCount >= chargeDashesRequired)
                {
                    AdvanceToNextStep();
                }
            }
            else
            {
                Debug.Log($"Space released too quickly ({holdDuration:F2}s) - not a charge dash");
            }
        }
    }

    void CheckDashCancelInput()
    {
        bool dashPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Dash");
        bool noMovement = !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && 
                         !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D) &&
                         Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0;

        if (dashPressed && noMovement)
        {
            Debug.Log("Dash cancel detected!");
            AdvanceToNextStep();
        }
    }

    void CheckMouseMoveInput()
    {
        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f)
        {
            Debug.Log("Mouse movement detected!");
            AdvanceToNextStep();
        }
    }

    void CheckMarkerPlaceInput()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("First marker placed!");
            AdvanceToNextStep();
        }
    }

    void CheckFlyToMarkerInput()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Flying to marker!");
            AdvanceToNextStep();
        }
    }

    void CheckSecondMarkerInput()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Second marker placed!");
            AdvanceToNextStep();
        }
    }

    void CheckRemoveMarkerInput()
    {
        if (Input.GetKey(KeyCode.X))
        {
            xHoldTimer += Time.deltaTime;
            if (xHoldTimer >= markerRemoveHoldTime)
            {
                Debug.Log("Marker removal detected!");
                AdvanceToNextStep();
            }
        }
        else
        {
            xHoldTimer = 0f;
        }
    }

    void CheckDebugFeatherInput()
    {
        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Debug feather spawn activated!");
            AdvanceToNextStep();
        }
    }

    void CheckDebugTokenInput()
    {
        if (Input.GetKey(KeyCode.P) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Debug token spawn activated!");
            AdvanceToNextStep();
        }
    }

    void CheckChainDashInput()
    {
        // Check for chain dash: holding direction + pressing space
        bool hasDirection = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
                           Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
                           Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool dashPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Dash");

        if (hasDirection && dashPressed)
        {
            chainDashCount++;
            int chainDashesRemaining = chainDashesRequired - chainDashCount;
            
            Debug.Log($"Chain dash count: {chainDashCount}/{chainDashesRequired}");
            
            if (chainDashesRemaining > 0)
            {
                if (tutorialText != null)
                    tutorialText.text = $"Hold a direction mid-dash and press SPACE to chain dash ({chainDashesRemaining} more)";
            }
            
            if (chainDashCount >= chainDashesRequired)
            {
                AdvanceToNextStep();
            }
        }
    }

    void AdvanceToNextStep()
    {
        switch (currentStep)
        {
            case TutorialStep.MovementInstruction:
                checkingForMovement = false;
                currentStep = TutorialStep.DashInstruction;
                if (tutorialText != null)
                    tutorialText.text = $"Press SPACE to dash ({dashesRequired} times)";
                Debug.Log("Movement tutorial completed - Starting dash phase!");
                checkingForDash = true;
                dashCount = 0;
                break;

            case TutorialStep.DashInstruction:
                checkingForDash = false;
                currentStep = TutorialStep.ChargeDashInstruction;
                if (tutorialText != null)
                    tutorialText.text = $"Hold SPACE to charge dash ({chargeDashesRequired} times)";
                Debug.Log("Dash tutorial completed - Starting charge dash phase!");
                checkingForChargeDash = true;
                chargeDashCount = 0;
                break;

            case TutorialStep.ChargeDashInstruction:
                checkingForChargeDash = false;
                currentStep = TutorialStep.DashCancel;
                if (tutorialText != null)
                    tutorialText.text = "Press SPACE without holding any direction to dash cancel\n(instant stop)";
                Debug.Log("Charge dash tutorial completed - Starting dash cancel phase!");
                checkingForDashCancel = true;
                break;

            case TutorialStep.DashCancel:
                checkingForDashCancel = false;
                currentStep = TutorialStep.MoveMouse;
                if (tutorialText != null)
                    tutorialText.text = "Move your mouse to bring up the crosshair";
                Debug.Log("Dash cancel tutorial completed - Starting mouse movement phase!");
                checkingForMouseMove = true;
                break;

            case TutorialStep.MoveMouse:
                checkingForMouseMove = false;
                currentStep = TutorialStep.PlaceMarker;
                if (tutorialText != null)
                    tutorialText.text = "Press X to place down a marker";
                Debug.Log("Mouse movement tutorial completed - Starting marker placement phase!");
                checkingForMarker = true;
                break;

            case TutorialStep.PlaceMarker:
                checkingForMarker = false;
                currentStep = TutorialStep.FlyToMarker;
                if (tutorialText != null)
                    tutorialText.text = "Press X again to fly to the marker";
                Debug.Log("Marker placement tutorial completed - Starting fly to marker phase!");
                checkingForFlyToMarker = true;
                break;

            case TutorialStep.FlyToMarker:
                checkingForFlyToMarker = false;
                currentStep = TutorialStep.PlaceSecondMarker;
                if (tutorialText != null)
                    tutorialText.text = "Press X to place another marker";
                Debug.Log("Fly to marker tutorial completed - Starting second marker phase!");
                checkingForSecondMarker = true;
                break;

            case TutorialStep.PlaceSecondMarker:
                checkingForSecondMarker = false;
                currentStep = TutorialStep.HoldXRemoveMarker;
                if (tutorialText != null)
                    tutorialText.text = "Hold X to remove markers";
                Debug.Log("Second marker tutorial completed - Starting marker removal phase!");
                checkingForRemoveMarker = true;
                xHoldTimer = 0f;
                break;

            case TutorialStep.HoldXRemoveMarker:
                checkingForRemoveMarker = false;
                currentStep = TutorialStep.DebugFeatherControl;
                if (tutorialText != null)
                    tutorialText.text = "Press P+1 to spawn feathers (debug control)\n(Max : 5)";
                Debug.Log("Marker removal tutorial completed - Starting debug feather control!");
                checkingForDebugFeather = true;
                break;

            case TutorialStep.DebugFeatherControl:
                checkingForDebugFeather = false;
                currentStep = TutorialStep.DebugTokenControl;
                if (tutorialText != null)
                    tutorialText.text = "Press P+2 to spawn chain dash tokens (debug control)\n(Max : 3)";
                Debug.Log("Debug feather control completed - Starting debug token control!");
                checkingForDebugToken = true;
                break;

            case TutorialStep.DebugTokenControl:
                checkingForDebugToken = false;
                currentStep = TutorialStep.ChainDashInstruction;
                if (tutorialText != null)
                    tutorialText.text = $"Hold a direction mid-dash and\npress SPACE to chain dash\n({chainDashesRequired} times)";
                Debug.Log("Debug token control completed - Starting chain dash phase!");
                checkingForChainDash = true;
                chainDashCount = 0;
                break;

            case TutorialStep.ChainDashInstruction:
                checkingForChainDash = false;
                currentStep = TutorialStep.TutorialComplete;
                if (tutorialText != null)
                    tutorialText.text = "Tutorial Complete! You're ready to play!";
                Debug.Log("All tutorial steps completed - Tutorial finished!");
                Invoke(nameof(EndTutorial), 4f);
                break;
        }
    }

    void SkipTutorial()
    {
        Debug.Log("Tutorial skipped!");
        EndTutorial();
    }

    void EndTutorial()
    {
        tutorialActive = false;
        waitingForInitialChoice = false;
        checkingForMovement = false;
        checkingForDash = false;
        checkingForChargeDash = false;
        checkingForDashCancel = false;
        checkingForMouseMove = false;
        checkingForMarker = false;
        checkingForFlyToMarker = false;
        checkingForSecondMarker = false;
        checkingForRemoveMarker = false;
        checkingForDebugFeather = false;
        checkingForDebugToken = false;
        checkingForChainDash = false;
        
        dashCount = 0;
        chargeDashCount = 0;
        chainDashCount = 0;
        spaceHoldStartTime = 0f;
        isHoldingSpace = false;
        xHoldTimer = 0f;
        currentStep = TutorialStep.InitialChoice;
        
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        Time.timeScale = 1f;
        
        Debug.Log("Tutorial ended - Normal gameplay resumed");
    }

    [ContextMenu("Restart Tutorial")]
    public void RestartTutorial()
    {
        EndTutorial();
        ShowTutorialChoice();
    }
}
