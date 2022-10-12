using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum BehaviourEnum { player, bot, botInputs };
public enum CharacterState { idle, attack, block, dash, hitted, recoil };
public enum BotMovementState { idle, follow, wander };

public class GameInputs
{
    public int PlayerHP { get; set; }
    public int BotHP { get; set; }
    public float DistanceToPlayer { get; set; }
    public Vector2 DirectionToPlayer { get; set; }
    public CharacterState PlayerState { get; set; }
    public CharacterState BotState { get; set; }
    public float PlayerRotation { get; set; }
    public float BotRotation { get; set; }

    public GameInputs()
    {
        PlayerHP = 0;
        BotHP = 0;
        DistanceToPlayer = 0f;
        DirectionToPlayer = new Vector2(0, 0);
        PlayerState = CharacterState.idle;
        BotState = CharacterState.idle;
        PlayerRotation = 0f;
        BotRotation = 0f;
    }

    public GameInputs(
        int playerHP,
        int botHP,
        float distanceToPlayer,
        Vector2 directionToPlayer,
        CharacterState playerState,
        CharacterState botState,
        float playerRotation,
        float botRotation
    )
    {
        PlayerHP = playerHP;
        BotHP = botHP;
        DistanceToPlayer = distanceToPlayer;
        DirectionToPlayer = directionToPlayer;
        PlayerState = playerState;
        BotState = botState;
        PlayerRotation = playerRotation;
        BotRotation = botRotation;
    }

}

public class Character : MonoBehaviour
{
    [Header("Behaviour")]
    // Indicates if it can be controlled by the user
    [SerializeField] private BehaviourEnum _behaviour;
    public BehaviourEnum Behaviour {
        get {
            return _behaviour;
        }
    }

    [Header("Characteristics")]
    // Character Speed
    [SerializeField] private int _speed;
    public int Speed {
        get {
            return _speed;
        }
        set {
            _speed = value;
        }
    }

    // Character max health
    [SerializeField] private int _maxHp;
    public int MaxHP {
        get {
            return _maxHp;
        }
        set {
            _maxHp = value;
        }
    }

    // Character health
    private int _hp;
    public int HP {
        get {
            return _hp;
        }
        set {
            _hp = Mathf.Clamp(value, 0, MaxHP);
            if (_healthBar != null)
            {
                _healthBar.UpdateHealthBar(HP, MaxHP);
            }
            if (_hp == 0)
            {
                Die();
            }
        }
    }

    [SerializeField] private int _numberLives;
    private int _currentLives;
    public int CurrentLives {
        get {
            return _currentLives;
        }
        set {
            _currentLives = Mathf.Clamp(value, 0, _numberLives);
            
            if (_currentLives == 0)
            {
                FinishGame();
            }
        }
    }

    private bool _isDead;

    [SerializeField] private bool _isLeftPlayer;

    [Header("IFrames")]
    // Invulnerability frames after hit  in seconds
    [SerializeField] private float _iFramesHit; 

    // Dash
    [SerializeField] private float _dashIFrames;
    [SerializeField] private float _dashStrength;
    // How much a character in knockbacked when blocking
    [SerializeField] private int _blockKnockback; 


    [Header("HUD")]
    // Character health bar
    [SerializeField] private HealthBar _healthBar;

    // Character Weapon
    [SerializeField] private Weapon _weapon;

    // Character Shield
    [SerializeField] private Shield _shield;

    // Other Character in the scene
    [SerializeField] private Character _otherCharacter;


    [Header("BOT")]
    [SerializeField] private float _randIntensityMove;


    // No SerializeField
    // Character movement and rotation
    private Vector2 _move;
    private float _rotation;

    // Animator
    private Animator _animator;

    // Game Inputs
    private GameInputs _gameInputs;

    // Current State
    private CharacterState _currentState;
    public CharacterState CurrentState {
        get {
            return _currentState;
        }
        set {
            _currentState = value;
        }
    }

    // Bot Movement State
    private BotMovementState _botMovementState;

    // Point where the bot wanders
    private Vector2 _wanderingPoint;

    // Mapping function for input bot
    private float[] _chanceMap = new float[] {0.0f, 0.01f, 0.015f, 0.02f, 0.025f, 0.03f, 0.035f, 0.04f, 0.045f, 0.05f, 0.0f};

    // Position at the beginning
    private Vector3 _initialPosition;
    public Vector3 InitialPosition {
        get {
            return _initialPosition;
        }
        set {
            _initialPosition = value;
        }
    }

    [Header("Student ID")]
    [SerializeField] private string _studentID;
    private string _fileName;
    public string FileName {
        get {
            return _fileName;
        }
    }

    private float _startTimer;


    // -------------------------------------------------
    // ----------------- State Machine -----------------
    // -------------------------------------------------

    private void StateToIdle() 
    {
        _currentState = CharacterState.idle;
    }

    private void StateToAttack()
    {
        _currentState = CharacterState.attack;
    }

    private void StateToBlock()
    {
        _currentState = CharacterState.block;
    }

    private void StateToDash()
    {
        _currentState = CharacterState.dash;
    }

    private void StateToHitted()
    {
        _currentState = CharacterState.hitted;
    }

    private void StateToRecoil()
    {
        _currentState = CharacterState.recoil;
    }


    // -------------------------------------------------
    // ---------- Bot Movement State Machine -----------
    // -------------------------------------------------

    private void MovementStateToIdle() 
    {
        _botMovementState = BotMovementState.idle;
    }

    private void MovementStateToFollow()
    {
        _botMovementState = BotMovementState.follow;
    }

    private void MovementStateToWander()
    {
        _botMovementState = BotMovementState.wander;
    }


    // -------------------------------------------------
    // -------------------- Actions --------------------
    // -------------------------------------------------

    public void OnMove(InputAction.CallbackContext ctx)
    {   
        if (_behaviour == BehaviourEnum.player)
        {
            _move = ctx.ReadValue<Vector2>();
            // Only update rotation if is moving, otherwise keep old rotation
            if (ctx.ReadValue<Vector2>() != new Vector2(0, 0))
            {
                // Block rotation if attacking
                // if (_weapon.IsAttacking == false)
                // {
                _rotation = Mathf.Atan2(-ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y)* Mathf.Rad2Deg;
                // }
            }
        }
    }

    private void Move()
    {
        if (_currentState != CharacterState.dash && _currentState != CharacterState.hitted && _currentState != CharacterState.recoil)
        {
            // Movement
            GetComponent<Rigidbody2D>().velocity = _move * Speed * Time.deltaTime;

            if (_weapon.IsAttacking == false)
            {
                // Rotation
                transform.rotation = Quaternion.Euler(0, 0, _rotation);
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (_behaviour == BehaviourEnum.player && ctx.performed)
        {
            Attack();
        }
    }

    private void Attack ()
    {
        // Can only attack if idle state and weapon is not on cooldown
        if (_currentState == CharacterState.idle && !_weapon.WeaponCooldown)
        {
            StateToAttack();
            _weapon.Attack();
        }
    }


    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (_behaviour == BehaviourEnum.player)
        {
            if (ctx.performed)
            {
                Block();
            }
            else if (ctx.canceled)
            {
                Unblock();
            }
        }
    }

    private void Block()
    {
        if (_currentState == CharacterState.idle)
        {
            StateToBlock();
            _shield.Block();
        }
    }

    private void Unblock()
    {
        if (_currentState == CharacterState.block)
        {
            StateToIdle();
            _shield.Unblock();
        }
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (_behaviour == BehaviourEnum.player && ctx.performed)
        {
            Dash();
        }
    }

    private void Dash()
    {
        if (_currentState == CharacterState.idle)
        {
            StateToDash();
            StartCoroutine(IFrames(_dashIFrames));

            GetComponent<Rigidbody2D>().AddForce(transform.up * _dashStrength);
            transform.rotation = Quaternion.Euler(0, 0, _rotation);
        }
    }

    // -------------------------------------------------
    // ----------------- Bot Behaviour -----------------
    // -------------------------------------------------

    // Hardcoded Bot
    private void UpdateMovementBotState()
    {
        float randomChance = Random.value;

        if (_botMovementState == BotMovementState.idle)
        {
            if (randomChance < 0.07)
            {
                _botMovementState = BotMovementState.follow;
            }
            else if (randomChance < 0.1)
            {
                _botMovementState = BotMovementState.wander;
                _wanderingPoint = Random.insideUnitCircle * 8;
            }
        }
        else if (_botMovementState == BotMovementState.follow)
        {
            if (randomChance < 0.1)
            {
                _botMovementState = BotMovementState.idle;
            }
        }
        else if (_botMovementState == BotMovementState.wander)
        {
            if (randomChance < 0.1)
            {
                _botMovementState = BotMovementState.idle;
            }
        }

    }

    // Harcoded Bot Actions
    private void BotActions()
    {
        float randomChance = Random.value;

        if (_currentState == CharacterState.idle)
        {
            if (randomChance < 0.02)
            {
                Attack();
            }
            else if (randomChance < 0.04)
            {
                Dash();
            }  
            else if (randomChance < 0.06)
            {
                Block();
            }
        }
        else if (_currentState == CharacterState.block)
        {
            if (randomChance < 0.01)
            {
                Unblock();
            }
        }
    }


    // Hardcoded Bot that uses game inputs for decision making
    private void UpdateMovementInputBotState()
    {
        float randomChance = Random.value;

        if (_botMovementState == BotMovementState.idle)
        {
            if (_gameInputs.DistanceToPlayer > 1 && _gameInputs.DistanceToPlayer < 2)
            {
                _botMovementState = BotMovementState.follow;
            }
            else if (_gameInputs.DistanceToPlayer > 2)
            {
                _botMovementState = BotMovementState.wander;
                _wanderingPoint = Random.insideUnitCircle * 8;
            }
        }
        else if (_botMovementState == BotMovementState.follow)
        {
            if (_gameInputs.DistanceToPlayer < 1 || randomChance < 0.05)
            {
                _botMovementState = BotMovementState.idle;
            }
        }
        else if (_botMovementState == BotMovementState.wander)
        {
            if (_gameInputs.DistanceToPlayer < 2 || randomChance < 0.05)
            {
                _botMovementState = BotMovementState.idle;
            }
        }
    }

    // Harcoded Bot that uses game inputs for decision making Actions
    private void BotInputActions()
    {
        float randomChance = Random.value;

        float attackChance = _chanceMap[_gameInputs.BotHP - _gameInputs.PlayerHP + 5];
        float blockChance = _chanceMap[_gameInputs.PlayerHP - _gameInputs.BotHP + 5];
        float dashChance = 0.1f;
        float unblockChance = 0.02f;

        if (_currentState == CharacterState.idle)
        {
            if (_gameInputs.DistanceToPlayer < 2 && randomChance < attackChance)
            {
                Attack();
            }
            else if (_gameInputs.DistanceToPlayer < 2 && randomChance < attackChance + blockChance)
            {
                Block();
            }
            else if (_gameInputs.DistanceToPlayer > 3 && randomChance < dashChance)
            {
                Dash();
            }  
        }
        else if (_currentState == CharacterState.block)
        {
            if (_gameInputs.DistanceToPlayer > 2 || randomChance < unblockChance)
            {
                Unblock();
            }
        }
    }



    private void AgentMovement()
    {
        {
            // Movement
            if (_botMovementState == BotMovementState.idle)
            {
                _move = new Vector2(0, 0);
            }
            else if (_botMovementState == BotMovementState.follow)
            {
                _move = _gameInputs.DirectionToPlayer;
            }
            else if (_botMovementState == BotMovementState.wander)
            {
                _move = _wanderingPoint - new Vector2(transform.position.x, transform.position.y);
            }

            // Only update rotation if is moving, otherwise keep old rotation
            if (_move != new Vector2(0, 0))
            {
                 _move.Normalize();
                _rotation = Mathf.Atan2(-_move.x, _move.y)* Mathf.Rad2Deg;
            }            
            
        }
    }





    private void UpdateGameInputs() 
    {
        if (_behaviour != BehaviourEnum.player)
        {
            _gameInputs.PlayerHP = _otherCharacter.HP;
            _gameInputs.BotHP = _hp;
            _gameInputs.DistanceToPlayer = Vector2.Distance(transform.position, _otherCharacter.transform.position);
            _gameInputs.DirectionToPlayer = _otherCharacter.transform.position - transform.position;
            _gameInputs.PlayerState = _otherCharacter.CurrentState;
            _gameInputs.BotState = _currentState;
            _gameInputs.PlayerRotation = _otherCharacter.transform.eulerAngles.z;
            _gameInputs.BotRotation = transform.eulerAngles.z;
        }
    }  

    // -------------------------------------------------
    // -------------------- Generics -------------------
    // -------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        HP = MaxHP;
        CurrentLives = _numberLives;
        _animator = GetComponent<Animator>();
        _gameInputs = new GameInputs();
        _currentState = CharacterState.idle;
        _botMovementState = BotMovementState.idle;

        _initialPosition = transform.position;

        if (_studentID != "")
        {
            string path = Application.streamingAssetsPath + "/Metrics/";
            _fileName = path + _studentID + ".csv";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists(_fileName))
            {
                File.WriteAllText(_fileName, "time,bot,winner,winnerHP\n");
            }
        }

        _startTimer = Time.time;
        if (_healthBar != null) 
        {
            _healthBar.PlaceHearts(_isLeftPlayer, _numberLives);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {           
        if (!_otherCharacter._isDead || !_isDead)
        {
            if (_behaviour != BehaviourEnum.player)
            {
                UpdateGameInputs();
                
                if (_behaviour == BehaviourEnum.bot) 
                {
                    UpdateMovementBotState();
                    BotActions();
                }
                else if (_behaviour == BehaviourEnum.botInputs)
                {
                    UpdateMovementInputBotState();
                    BotInputActions();
                }

                AgentMovement(); 
            }
            
            Move();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {   
        // Only check if player is vulnerable 
        if (_currentState != CharacterState.dash && _currentState != CharacterState.hitted)
        {
            // First check that it is a weapon
            if (other.gameObject.tag == "Weapon")
            {   
                Weapon otherWeapon = other.GetComponent<Weapon>();
                // Check it is not its own weapon, and that its character is attacking
                if (otherWeapon != _weapon && otherWeapon.ParentCharacter.CurrentState == CharacterState.attack)
                {

                    Vector3 hitDirection = transform.position - otherWeapon.transform.position;
                    hitDirection.Normalize();

                    // Angle between the two collisions
                    float angleHit = Mathf.DeltaAngle(transform.localEulerAngles.z, otherWeapon.transform.eulerAngles.z);

                    // Get in absolute value, which would be at most 180. Then remove 180, and the closer to 0, the closer
                    // the frontal collision
                    angleHit = Mathf.Abs(Mathf.Abs(angleHit) - 180);

                    // If the angle is smaller than a threshold, the hit has been blocked
                    if (_currentState == CharacterState.block && angleHit < _shield.ProtectionDegrees)
                    {
                        BlockHit(hitDirection, _blockKnockback);
                        otherWeapon.ParentCharacter.AttackIsBlocked(-hitDirection, _shield.Strength);
                    } 
                    else
                    {
                        if (_currentState == CharacterState.block)
                        {
                            _shield.Unblock();
                        }
                        StateToHitted();
                        GetHit(hitDirection, otherWeapon.Strength);
                    }
                }
            }
        }
    }

    private void BlockHit(Vector3 direction, int strength)
    {
        StateToRecoil();
        _shield.Unblock();
        StartCoroutine(IFramesNoAnim(_iFramesHit));

        // Move in the opposite direction with certain strength
        GetComponent<Rigidbody2D>().AddForce(direction * strength);
    }

    public void AttackIsBlocked(Vector3 direction, int strength)
    {
        StateToRecoil();
        StartCoroutine(IFramesNoAnim(_iFramesHit));

        // Move in the opposite direction with certain strength
        GetComponent<Rigidbody2D>().AddForce(direction * strength);
    }

    private void GetHit(Vector3 direction, int strength)
    {
        HP -= 1;
        StartCoroutine(IFrames(_iFramesHit));

        // Move in the opposite direction with certain strength
        GetComponent<Rigidbody2D>().AddForce(direction * strength);
    }

    private IEnumerator IFrames(float iFrames)
    {
        _animator.SetTrigger("Invulnerable");
        yield return new WaitForSeconds(iFrames);
        _animator.SetTrigger("Vulnerable");

        StateToIdle();
    }

    private IEnumerator IFramesNoAnim(float iFrames)
    {
        yield return new WaitForSeconds(iFrames);

        StateToIdle();
    }
    
    private void Die()
    {
        _isDead = true;

        CurrentLives -= 1;

        WriteInFile();
        _startTimer = Time.time;

        StartCoroutine(StartNewMatch());

        if (_healthBar != null)
        {
            _healthBar.LoseLive();
        }
    }

    private IEnumerator StartNewMatch()
    {
        yield return new WaitForSeconds(0.5f);

        transform.position = _initialPosition;
        _otherCharacter.transform.position = _otherCharacter.InitialPosition;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        _otherCharacter.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return new WaitForSeconds(0.5f);

        ResetCharacter();
        _otherCharacter.ResetCharacter();
    }

    private void WriteInFile()
    {
        if (_fileName != null)
        {
            File.AppendAllText(_fileName, 
                Time.time - _startTimer + "," + _otherCharacter.Behaviour + "," + _otherCharacter.Behaviour + "," + _otherCharacter.HP + "\n"
            );
        }
        else if (_otherCharacter.FileName != null)
        {
            File.AppendAllText(_otherCharacter.FileName,
                Time.time - _startTimer + "," + _behaviour + "," + _otherCharacter.Behaviour + "," + _otherCharacter.HP + "\n"
            );
        }
    }

    public void ResetCharacter()
    {
        _isDead = false;
        HP = MaxHP;
    }

    private void FinishGame()
    {
        SceneManager.LoadScene(2);
    }
}
