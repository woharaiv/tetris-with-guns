using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    new public static Camera camera;

    public bool acceptingInput = true;
    [SerializeField] InputActionAsset actionAsset;
    InputActionMap input;
    InputAction shift;
    InputAction softDrop;

    [SerializeField] PieceSpawner pieceSpawner;

    public Tetramino activePiece;
    bool gameRunning = true;
    float tileSize, stepTimer, stepTimerMax, DASWaitTimer, DASMoveTimer, spawnTimer, lineClearTimer;
    [SerializeField, Tooltip("The speed the piece falls down, at a rate of G/256 rows per frame.")] float gravity;
    [SerializeField, Tooltip("How much faster pieces drop while holding down.")] float softDropSpeedMult = 2;
    [SerializeField, Tooltip("Number of seconds the player most hold one move button before Delayed Auto Shift (basically a built-in turbo button) activates.")] float DASTimerMax;
    [SerializeField, Tooltip("Number of seconds between horizontal steps once DAS is active.")] float DASInterval;
    [SerializeField, Tooltip("Number of seconds the game waits between placing a piece and spawning the next one.")] float spawnDelay = 0.5f;
    [SerializeField, Tooltip("Number of seconds the game pauses when a line is cleared before continuing play.")] float lineClearDelay = 0.67f;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Game Manager, destroying this one");
            Destroy(gameObject);
            return;
        }
        instance = this;
        camera = FindAnyObjectByType<Camera>();

        stepTimerMax = 256 / (60*gravity);
        stepTimer = stepTimerMax;
        DASWaitTimer = DASTimerMax;
        DASMoveTimer = DASInterval;
        spawnTimer = -999;
        lineClearTimer = -999;

        input = actionAsset.FindActionMap("Gameplay");
        shift = input.FindAction("Shift");
        input.FindAction("Shift").started += ShiftInput;
        input.FindAction("Rotate").started += RotatePiece;
        softDrop = input.FindAction("Soft Drop");
        input.FindAction("Hard Drop").started += HardDrop;

    }

    void OnEnable()
    {
        input.Enable();
    }
    void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        tileSize = Playfield.tileSize;
        stepTimer = stepTimerMax;
        SpawnPiece();
    }

    private void Update()
    {
        if(gameRunning)
        {
            stepTimer -= Time.deltaTime;
            if(acceptingInput && softDrop.phase == InputActionPhase.Performed)
                stepTimer -= Time.deltaTime * (softDropSpeedMult - 1);
            if (stepTimer <= 0 || (acceptingInput && softDrop.phase == InputActionPhase.Started))
            {
                stepTimer = stepTimerMax;
                Step();
            }
        
            if(acceptingInput)
            {
                if(shift.phase == InputActionPhase.Performed) //If the move button is being held, perform DAS logic
                {
                    DASWaitTimer -= Time.deltaTime; //Decrement the timer for the initial DAS
                    if( DASWaitTimer <= 0 ) //If the timer is done, run DAS movement logic
                    {
                        DASMoveTimer -= Time.deltaTime; //Decrement the timer between instances of DAS movement.
                        if( DASMoveTimer <= 0 ) //If the timer is done, reset it and shift the tile.
                        {
                            DASMoveTimer = DASInterval;
                            MoveActive(Vector2.right * shift.ReadValue<float>());
                        }
                    }
                }
                else //If the move button is released or a new move button is pressed (ie. going from holding left to holding right), restart the DAS timer.
                {
                    DASWaitTimer = DASTimerMax;
                    DASMoveTimer = DASInterval;
                }
            }
        }
        else
        {
            if (lineClearDelay != -999)
                lineClearDelay -= Time.deltaTime;
            if (lineClearDelay <= 0)
            {
                Playfield.instance.DropAllTiles();
                lineClearDelay = -999;
                gameRunning = true;
                SpawnPiece();
            }
        }
    }

    /// <summary>
    /// Step the piece down once, placing it if it's already at the bottom.
    /// </summary>
    /// <returns>True if the piece hit the bottom and was placed insterad of moving down, false otherwise.</returns>
    bool Step()
    {
        if (activePiece != null)
        {
            if (!MoveActive(Vector2.down)) //Tries to step the piece down. If something is stopping it from moving down, place it.
            {
                PlaceMino();
                return true;
            }
            return false;
        }
        else
        {
            //Debug.Log("Step Failed: No active piece");
            return false;
        }
    }

    /// <summary>
    /// Move the piece in the given direction.
    /// </summary>
    /// <param name="dir">The number of tiles to move the piece.</param>
    /// <returns>True it resulted in a piece moving, false otherwise.</returns>
    bool MoveActive(Vector2 dir)
    {
        if(dir.sqrMagnitude == 0)
        {
            Debug.LogWarning("Tried to shift by 0");
            return false;
        }
        if (activePiece == null)
        {
            Debug.LogWarning("Shift Failed: No active piece");
            return false;
        }
        if (!activePiece.IsObstructed(dir)) //If it isn't obstructed, move in the indicated direction.
        {
            activePiece.gameObject.transform.position += (Vector3)(dir * tileSize);
            return true;
        }
        else
        {
            return false;
        }
    }

    void ShiftInput(InputAction.CallbackContext ctx)
    {
        if (!acceptingInput)
            return;
        MoveActive(Vector2.right * ctx.ReadValue<float>());
    }

    void RotatePiece(InputAction.CallbackContext ctx)
    {
        if (!acceptingInput)
            return;
        if (activePiece != null)
        {
            activePiece.TryRotate(ctx.ReadValue<float>());
        }
    }

    void HardDrop(InputAction.CallbackContext ctx)
    {
        if (!acceptingInput)
            return;
        int panicCount = 50;
        while(!Step())
        {
            panicCount--;
            if(panicCount <= 0)
            {
                Debug.LogError("Hard Drop function ran too long");
                break;
            }
        }
    }

    Tetramino SpawnPiece()
    {
        activePiece = pieceSpawner.SpawnRandomPiece();
        return activePiece;
    }

    void PlaceMino()
    {
        activePiece = null;
        Physics2D.SyncTransforms();
        int rowsCleared = Playfield.instance.RowClearCheck();
        if (rowsCleared > 0)
        {
            gameRunning = false;
            lineClearTimer = lineClearDelay;
        }
        else
            SpawnPiece();
    }
}
