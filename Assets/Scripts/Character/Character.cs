using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;


public class Character : MonoBehaviour
{
    // Indicates that the countdown is in process
    public bool timeBlocked;


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
    // Character stamina
    private int _stamina;
    public int Stamina {
        get {
            return _stamina;
        }
        set {
            _stamina = Mathf.Clamp(value, 0, _maxStamina);
            if (_healthBar != null)
            {
                _healthBar.UpdateStaminaBar(_stamina, _maxStamina);
            }
        }
    }

    // Character max stamina
    [SerializeField] private int _maxStamina;

    // Character stamina recover rate
    [SerializeField] private float _staminaRecoverRate;
    public float StaminaRecoverRate {
        get {
            return _staminaRecoverRate;
        }
        set {
            _staminaRecoverRate = value;
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

    private int _currentLives;
    public int CurrentLives {
        get {
            return _currentLives;
        }
        set {
            _currentLives = Mathf.Clamp(value, 0, MatchController.NumberLives);
            
            if (_currentLives == 0 && !_isTraining)
            {
                FinishGame();
            }
        }
    }

    private bool _isDead;
    private bool _isTraining;

    [SerializeField] private bool _isLeftPlayer;

    [Header("IFrames")]
    // Invulnerability frames after hit  in seconds
    [SerializeField] private float _iFramesHit; 

    // Dash
    [SerializeField] private float _dashIFrames;
    [SerializeField] private float _dashStrength;
    public float DashStrength {
        get {
            return _dashStrength;
        }
        set {
            _dashStrength = value;
        }
    }
    // How much a character in knockbacked when blocking
    [SerializeField] private int _blockKnockback; 


    [Header("HUD")]
    // Character health bar
    [SerializeField] private HealthBar _healthBar;
    public HealthBar HealthBarCharacter {
        get {
            return _healthBar;
        }
        set {
            _healthBar = value;
        }
    }
    [SerializeField] private TextMeshProUGUI _countDownText; 


    // Character Weapon
    [SerializeField] private Weapon _weapon;
    public Weapon CharacterWeapon {
        get {
            return _weapon;
        }
    }

    // Character Shield
    [SerializeField] private Shield _shield;
    public Shield CharacterShield {
        get {
            return _shield;
        }
    }
    // Other Character in the scene
    [SerializeField] private Character _otherCharacter;
    public Character OtherCharacter {
        get {
            return _otherCharacter;
        }
        set {
            _otherCharacter = value;
        }
    }

    // Bot probs / genes
    [SerializeField] private float _probIdleToFollow;
    [SerializeField] private float _probIdleToWander;
    [SerializeField] private float _probFollowToIdle;
    [SerializeField] private float _probWanderToIdle;

    [SerializeField] private float _probAttack;
    [SerializeField] private float _probDash;
    [SerializeField] private float _probBlock;
    [SerializeField] private float _probUnblock;


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
    // In case of not being centered on 0, 0, for the
    // wander behaviour to make sense
    private Vector2 _centerPoint;
    public Vector2 CenterPoint {
        get {
            return _centerPoint;
        }
        set {
            _centerPoint = value;
        }
    }

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

    private string _studentID;
    private string _fileName;
    public string FileName {
        get {
            return _fileName;
        }
    }

    private float _startTimer;
    private float _timer;
    public float Timer {
        get {
            return _timer;
        }
    }


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
        if (_currentState == CharacterState.idle && !_weapon.WeaponCooldown && Stamina >= 15)
        {
            StateToAttack();
            _weapon.Attack();

            Stamina -= 15;

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
        if (_currentState == CharacterState.idle && Stamina >= 10)
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
        if (_currentState == CharacterState.idle && Stamina >= 20)
        {
            StateToDash();
            StartCoroutine(IFrames(_dashIFrames));

            GetComponent<Rigidbody2D>().AddForce(transform.up * _dashStrength);
            transform.rotation = Quaternion.Euler(0, 0, _rotation);

            Stamina -= 20;
        }
    }

    private IEnumerator RecoverStamina()
    {
        yield return new WaitForSeconds(_staminaRecoverRate);
        if (_currentState != CharacterState.block)
        {
            Stamina += 1;
        }
        StartCoroutine(RecoverStamina());
    }

    private IEnumerator ConsumeStaminaBlocking()
    {
        yield return new WaitForSeconds(0.3f);
        if (_currentState == CharacterState.block)
        {
            Stamina -= 2;
            if (Stamina == 0)
            {
                Unblock();
            }
        }

        StartCoroutine(ConsumeStaminaBlocking());
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
            if (randomChance < _probIdleToFollow)
            {
                _botMovementState = BotMovementState.follow;
            }
            else if (randomChance < _probIdleToFollow + _probIdleToWander)
            {
                _botMovementState = BotMovementState.wander;
                _wanderingPoint = Random.insideUnitCircle * 8 + _centerPoint;
            }
        }
        else if (_botMovementState == BotMovementState.follow)
        {
            if (randomChance < _probFollowToIdle)
            {
                _botMovementState = BotMovementState.idle;
            }
        }
        else if (_botMovementState == BotMovementState.wander)
        {
            if (randomChance < _probWanderToIdle)
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
                if (randomChance < _probAttack)
                {
                    Attack();
                }
                else if (randomChance < _probAttack + _probDash)
                {
                    Dash();
                }  
                else if (randomChance < _probAttack + _probDash + _probBlock)
                {
                    Block();
                }
            }
            else if (_currentState == CharacterState.block)
            {
                if (randomChance < _probUnblock)
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


    public bool IsBasicBot(BehaviourEnum behaviour)
    {
        return behaviour == BehaviourEnum.bot || behaviour == BehaviourEnum.botGenetic || behaviour == BehaviourEnum.botMany;
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
    // ------------------ Instantiate ------------------
    // -------------------------------------------------

    public void Init(
        float probIdleToFollow,
        float probIdleToWander,
        float probFollowToIdle,
        float probWanderToIdle,
        float probAttack,
        float probDash,
        float probBlock,
        float probUnblock,
        bool isTraining,
        BehaviourEnum behaviour
    )
    {
        _probIdleToFollow = Mathf.Clamp(probIdleToFollow, 0.03f, 1);
        _probIdleToWander = Mathf.Clamp(probIdleToWander, 0.03f, 1);
        _probFollowToIdle = Mathf.Clamp(probFollowToIdle, 0.03f, 1);
        _probWanderToIdle = Mathf.Clamp(probWanderToIdle, 0.03f, 1);
        _probAttack = Mathf.Clamp(probAttack, 0.03f, 1);
        _probDash = Mathf.Clamp(probDash, 0.03f, 1);
        _probBlock = Mathf.Clamp(probBlock, 0.03f, 1);
        _probUnblock = Mathf.Clamp(probUnblock, 0.03f, 1);
        
        _isTraining = isTraining;
        _behaviour = behaviour;
    }

    // -------------------------------------------------
    // -------------------- Generics -------------------
    // -------------------------------------------------
    
    void Awake()
    {
        if (_behaviour == BehaviourEnum.player)
        {
            MatchController.SetPlayer(this);
        }
        else
        {
            MatchController.SetBot(this);
        }
    }

    private void OnEnable() {
        StartCoroutine(RecoverStamina());
        StartCoroutine(ConsumeStaminaBlocking());
    }

    // Start is called before the first frame update
    void Start()
    {
        HP = MaxHP;
        Stamina = _maxStamina;
        
        CurrentLives = MatchController.NumberLives;
        if (_healthBar != null) 
        {
            _healthBar.PlaceHearts(_isLeftPlayer, CurrentLives);
        }
        
        _animator = GetComponent<Animator>();
        _gameInputs = new GameInputs();
        _currentState = CharacterState.idle;
        _botMovementState = BotMovementState.idle;

        _initialPosition = transform.position;
        if (_behaviour == BehaviourEnum.player && MatchController.PlayerID != "")
        {
            _studentID = MatchController.PlayerID;
            string path = Application.streamingAssetsPath + "/Metrics/";
            _fileName = path + _studentID + ".csv";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists(_fileName))
            {
                File.WriteAllText(_fileName, "time,bot,winner,winnerHP,probs\n");
            }
        }

        _startTimer = Time.time;
        

        if (IsBasicBot(_behaviour) && !_isTraining)
        {
            float[] genes = MatchController.CurrentGenes;
            
            BehaviourEnum botBehaviour = BehaviourEnum.bot;

            if (!MatchController.WarmUp && MatchController.CurrentGameMode == GameMode.genetic)
            {
                botBehaviour = BehaviourEnum.botGenetic;
            }
            else if (MatchController.CurrentGameMode == GameMode.manyEnemies)
            {
                botBehaviour = BehaviourEnum.botMany;
            }

            if (genes != null)
            {
                Init(genes[0], genes[1], genes[2], genes[3], genes[4], genes[5], genes[6], genes[7], false, botBehaviour);
            }
        }

        if (_countDownText != null)
        {
            StartCoroutine(StartCountDown());

        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {           
        if (!_otherCharacter._isDead && !_isDead && !timeBlocked && !_otherCharacter.timeBlocked)
        {
            if (_behaviour != BehaviourEnum.player)
            {
                UpdateGameInputs();
                
                if (IsBasicBot(_behaviour)) 
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

        // Recover 10 stamina
        Stamina += 10;

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
    
    public void Die()
    {
        _isDead = true;
        
        _timer = Time.time - _startTimer;
        if (_behaviour == BehaviourEnum.player)
        {
            MatchController.SetTrainingInfo(this, _timer);
        }
        else if (_otherCharacter.Behaviour == BehaviourEnum.player)
        {
            MatchController.SetTrainingInfo(_otherCharacter, _timer);
        }

        _startTimer = Time.time;

        if (!_isTraining)
        {
            CurrentLives -= 1;

            WriteInFile();
            StartCoroutine(StartNewMatch());

            if (_healthBar != null)
            {
                _healthBar.LoseLive();
            }
        }

    }

    private IEnumerator StartNewMatch()
    {        
        yield return new WaitForSeconds(0.5f);
        // THE PLAYER WON -> THE BOT DIED
        if (IsBasicBot(_behaviour))
        {
            MatchController.CurrentDifficulty += 1;
        }
        // THE BOT WON -> THE PLAYER DIED
        else if (_behaviour == BehaviourEnum.player)
        {
            MatchController.CurrentDifficulty -= 1;
        }

        float[] genes = MatchController.CurrentGenes;

        if (genes != null && IsBasicBot(_behaviour))
        {
            Init(genes[0], genes[1], genes[2], genes[3], genes[4], genes[5], genes[6], genes[7], false, _behaviour);
        }

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
                _timer + "," + _otherCharacter.Behaviour + "," + _otherCharacter.Behaviour + "," + _otherCharacter.HP + ","
            );

            File.AppendAllText(_fileName, "[");
            for (int i = 0; i < MatchController.CurrentGenes.Length-1; i++)
            {
                File.AppendAllText(_fileName, MatchController.CurrentGenes[i].ToString() + ",");
            }
            File.AppendAllText(_fileName, MatchController.CurrentGenes[MatchController.CurrentGenes.Length-1].ToString() + "]\n");
        }
        else if (_otherCharacter.FileName != null)
        {
            File.AppendAllText(_otherCharacter.FileName,
                _timer + "," + _behaviour + "," + _otherCharacter.Behaviour + "," + _otherCharacter.HP + ","
            );

            File.AppendAllText(_otherCharacter.FileName, "[");
            for (int i = 0; i < MatchController.CurrentGenes.Length-1; i++)
            {
                File.AppendAllText(_otherCharacter.FileName, MatchController.CurrentGenes[i].ToString() + ",");
            }
            File.AppendAllText(
                _otherCharacter.FileName,
                MatchController.CurrentGenes[MatchController.CurrentGenes.Length-1].ToString() + "]\n"
            );
        }
    }

    public void ResetCharacter()
    {
        _isDead = false;
        HP = MaxHP;
        Stamina = _maxStamina;
        _currentState = CharacterState.idle;
        _botMovementState = BotMovementState.idle;

        if (_countDownText != null)
        {
            StartCoroutine(StartCountDown());
        }
    }

    private void FinishGame()
    {
        if (MatchController.WarmUp && MatchController.CurrentGameMode == GameMode.genetic)
        {
            MatchController.UpdateWarmUp(false);
            SceneManager.LoadScene(2);
        }
        else
        {
            SceneManager.LoadScene(3);
        }
        
    }

    public void ChangeEnemyCharacter(Character otherCharacter)
    {   
        _otherCharacter = otherCharacter;
    }

    public IEnumerator StartCountDown()
    {
        timeBlocked = true;
        _countDownText.text = "3";
        yield return new WaitForSeconds(1.0f);
        _countDownText.text = "2";
        yield return new WaitForSeconds(1.0f);
        _countDownText.text = "1";
        yield return new WaitForSeconds(1.0f);
        _countDownText.text = "";
        timeBlocked = false;
    }
}
