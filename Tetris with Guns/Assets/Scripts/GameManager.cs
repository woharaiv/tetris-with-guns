using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using DG.Tweening;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    new public static Camera camera;

    Gun gun;

    public bool acceptingInput = true;
    [SerializeField] InputActionAsset actionAsset;
    InputActionMap input;
    InputAction shift;
    InputAction softDrop;

    PieceSpawner pieceSpawner;

    [HideInInspector] public Tetramino activePiece;
    [HideInInspector] public bool gameRunning = true;

    public bool gameOver = false;
    float gameOverTimer = -999;
    float pieceBurstTimerMax = 0.25f;
    float pieceBurstTimer = 0;
    
    [Space, SerializeField] float crateSpawnChance = 0.1f;
    
    float tileSize, stepTimer, stepTimerMax, DASWaitTimer, DASMoveTimer, spawnTimer, lineClearTimer;
    [Space, SerializeField, Tooltip("The speed the piece falls down, at a rate of G/256 rows per frame.")] public float gravity;
    [SerializeField, Tooltip("How much faster pieces drop while holding down.")] public float softDropSpeedMult = 2;
    [SerializeField, Tooltip("Number of seconds the player most hold one move button before Delayed Auto Shift (basically a built-in turbo button) activates.")] float DASTimerMax;
    [SerializeField, Tooltip("Number of seconds between horizontal steps once DAS is active.")] float DASInterval;
    [SerializeField, Tooltip("Number of seconds the game waits between placing a piece and spawning the next one.")] float spawnDelay = 0.5f;
    [SerializeField, Tooltip("Number of seconds the game pauses when a line is cleared before continuing play.")] float lineClearDelay = 0.67f;

    [Space, SerializeField] int level = 1;
    [SerializeField] int lineGoal = 5;
    [SerializeField] int lineScore = 0;

    [SerializeField] TextMeshProUGUI scoreField;
    [SerializeField] TextMeshProUGUI hiScoreField;
    int points;
    int lineClearCount = 0;
    float softDropTime = 0;
    int hardDropDist = 0;
    float pieceActiveTime = 0;
    int combo;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Game Manager, destroying this one");
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        camera = Camera.main;
        gun = FindAnyObjectByType<Gun>();
        pieceSpawner = FindAnyObjectByType<PieceSpawner>();
        
        pieceSpawner.InitializeNextPiece();

        stepTimerMax = 256 / (60*gravity);
        stepTimer = stepTimerMax;
        DASWaitTimer = DASTimerMax;
        DASMoveTimer = DASInterval;
        spawnTimer = -999;
        lineClearTimer = -999;

        ScoreManager.lastScore = 0;

        input = actionAsset.FindActionMap("Gameplay");
        shift = input.FindAction("Shift");
        input.FindAction("Shift").started += ShiftInput;
        input.FindAction("Rotate").started += RotatePiece;
        softDrop = input.FindAction("Soft Drop");
        input.FindAction("Hard Drop").started += HardDrop;

        Tetramino.randomIndexMax = (int)(4 / crateSpawnChance) - 1;

        DOTween.SetTweensCapacity(200, 150);
        
    }

    void OnEnable()
    {
        input.Enable();
    }
    void OnDisable()
    {
        input.Disable();
    }

    private void OnDestroy()
    {
        input.FindAction("Shift").started -= ShiftInput;
        input.FindAction("Rotate").started -= RotatePiece;
        input.FindAction("Hard Drop").started -= HardDrop;
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
            if(acceptingInput && activePiece != null)
            {
                pieceActiveTime += Time.deltaTime;
                if (softDrop.phase == InputActionPhase.Started || softDrop.phase == InputActionPhase.Performed)
                    softDropTime += Time.deltaTime;
            }
            if (acceptingInput && softDrop.phase == InputActionPhase.Performed)
            {
                stepTimer -= Time.deltaTime * (softDropSpeedMult - 1);
            }
            if (stepTimer <= 0 || (acceptingInput && softDrop.phase == InputActionPhase.Started))
            {
                stepTimer = stepTimerMax;
                Step();
            }
            if(activePiece == null && spawnTimer > -999)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    SpawnPiece();
                }
            }

            if(acceptingInput && spawnTimer <= 0)
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
            }

        }
        else
        {
            if(gameOver)
            {
                gameOverTimer -= Time.deltaTime;
                if(gameOverTimer <= 0)
                {
                    pieceBurstTimer += Time.deltaTime;
                    if (pieceBurstTimer >= pieceBurstTimerMax)
                    {
                        pieceBurstTimerMax -= 0.01f;
                        pieceBurstTimer = 0;
                        if(Playfield.instance.tilesInPlay.Count > 0)
                        {
                            Playfield.instance.tilesInPlay[Playfield.instance.tilesInPlay.Count - 1].KillTile(Tile.KillType.NoGravCheck, true, false);
                        }
                        else
                        {
                            pieceBurstTimerMax = float.MaxValue;
                            gun.screenFlashAsset.DOColor(new Color(0, 0, 0, 1f), 2f).OnComplete(()=>StartCoroutine(GoToGameOver()));
                        }
                    }
                }
            }
            if (lineClearTimer != -999)
            {
                lineClearTimer -= Time.deltaTime;
                if (lineClearTimer <= 0)
                {
                    Playfield.instance.DropAllTiles();
                    lineClearTimer = -999;
                    gameRunning = true;
                    if(activePiece == null)
                        SpawnPiece();
                }
            }
        }
    }
    IEnumerator GoToGameOver()
    {
        DOTween.KillAll(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameOver");
        while(!asyncLoad.isDone)
        {
            yield return null;
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
        DASWaitTimer = DASTimerMax;
        DASMoveTimer = DASInterval;
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
        if (!acceptingInput || activePiece == null)
            return;
        int panicCount = 50;
        while(!Step())
        {
            hardDropDist++;
            panicCount--;
            if(panicCount <= 0)
            {
                hardDropDist = 0;
                Debug.LogError("Hard Drop function ran too long");
                break;
            }
        }
    }

    Tetramino SpawnPiece()
    {
        if(!gameRunning)
            return null;
        activePiece = pieceSpawner.SpawnNextPiece();
        Physics2D.SyncTransforms();
        //Block Out: If a piece spawns overlapping a tile in the playfield, the player tops out.
        if (activePiece.IsObstructed(Vector2.zero, null, Color.red))
        {
            GameOver();
            PlaceMino();
            return null;
        }
        Step();
        spawnTimer = -999;
        hardDropDist = 0;
        softDropTime = 0;
        pieceActiveTime = 0;
        return activePiece;
    }

    void PlaceMino()
    {
        //Ensures colliders are in the right place for raycast checks
        Physics2D.SyncTransforms();
        
        //Lock out: If a piece is placed completely over the top line, the player tops out.
        //Block Out: If a piece spawns overlapping a tile in the playfield, the player tops out.
        if (activePiece.transform.position.y > 5 || activePiece.transform.position.y == 5 && activePiece.name.Equals("IPiece") 
            || activePiece.IsObstructed(Vector2.zero, null, Color.red))
            GameOver();

        activePiece.PiecePlaced();

        ClearFullLines(true);
        activePiece.isActivePiece = false;
        activePiece = null;

        spawnTimer = spawnDelay;
        ScoreManager.AddScore(CalculateScore());
    }

    public void ClearFullLines(bool canEndCombo = false)
    {
        List<Particle> rowClearParticles;
        List<float> rowsCleared = Playfield.instance.RowClearCheck(out rowClearParticles);
        int ammoToAdd = 0;
        if (rowsCleared.Count > 0)
        {
            InfoPopup rowClearPopup = Instantiate(Resources.Load<GameObject>("Prefabs/InfoPopup"), Vector3.up * rowsCleared[0], Quaternion.identity, null).GetComponent<InfoPopup>();
            gameRunning = false;
            lineClearTimer = lineClearDelay;
            lineClearCount += rowsCleared.Count;
            switch (rowsCleared.Count)
            {
                case 1:
                    rowClearPopup.Initialize(InfoMode.CUSTOM_TEXT, "CLEAR");
                    lineScore += 1;
                    ammoToAdd = 3;
                    break;
                case 2:
                    rowClearPopup.Initialize(InfoMode.CUSTOM_TEXT, "DOUBLE CLEAR!");
                    lineScore += 3;
                    ammoToAdd = 6;
                    break;
                case 3:
                    rowClearPopup.Initialize(InfoMode.CUSTOM_TEXT, "TRIPLE CLEAR!");
                    lineScore += 5;
                    ammoToAdd = 9;
                    break;
                case 4:
                    rowClearPopup.Initialize(InfoMode.CUSTOM_TEXT, "MEGA CLEAR!!");
                    lineScore += 8;
                    ammoToAdd = 12;
                    break;
                default:
                    rowClearPopup.Initialize(InfoMode.CUSTOM_TEXT, "UNSTOPPABLE!!!");
                    lineScore += 10;
                    ammoToAdd = 15;
                    break;
            }
            combo += (2 * rowsCleared.Count) - 2;
            while (lineScore >= lineGoal && lineGoal > 0)
            {
                lineScore -= lineGoal;
                level++;
                lineGoal += 5;
                gravity += 2;
                stepTimerMax = 256 / (60 * gravity);
            }
            gun.ScheduleMoveAmmoToReload(rowClearParticles, ammoToAdd);
        }
        else
        {
            if(canEndCombo)
                combo = 0;
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        gameRunning = false;
        Background.instance.movementVelocity = Vector2.zero;
        gameOver = true;
        lineClearTimer = -999;
        gameOverTimer = 1f;
        Playfield.instance.tilesInPlay.Sort((a,b) => (int)(b.transform.position.y - a.transform.position.y));
    }

    public static GameObject SpawnInfoPopup(Vector3 pos, InfoMode infoMode, string infoText = "TEXT NOT INITIALIZED")
    {
        GameObject infoPopup = Instantiate(Resources.Load<GameObject>("Prefabs/InfoPopup"), pos, Quaternion.identity, null);
        infoPopup.GetComponent<InfoPopup>().Initialize(infoMode, infoText);
        return infoPopup;
    }

    int CalculateScore()
    {
        //Score = ((Level + Lines)/4 + Soft + Sonic) x Lines x Combo + (Level_After_Clear)/2 + Speed
        int score = 0;
        int softDropFrames = (int)(softDropTime * 60);
        int speed = (int)(stepTimerMax * 60) - (int)(pieceActiveTime * 60);
        score = ((((level + lineClearCount) / 4) + 1) + softDropFrames + hardDropDist) * (lineClearCount + 1) * (combo + 1) + ((level / 2) + 1) + Mathf.Max(speed, 1);
        score = Mathf.Max(0, score);
        return score;
    }
}
