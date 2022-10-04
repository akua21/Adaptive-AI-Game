using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum BehaviourEnum { player, bot };
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
            if (_hp == 0)
            {
                Die();
            }
        }
    }

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

    private void AgentBehaviour()
    {
        if (_behaviour == BehaviourEnum.bot)
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


            // Actions
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
    }

    private void UpdateGameInputs() 
    {
        if (_behaviour != BehaviourEnum.player)
        {
            _gameInputs.PlayerHP = _otherCharacter.HP;
            _gameInputs.BotHP = _hp;
            _gameInputs.DistanceToPlayer = Vector2.Distance(transform.position, _otherCharacter.transform.position);
            _gameInputs.DirectionToPlayer = _otherCharacter.transform.position - transform.position;
            _gameInputs.PlayerState = _currentState;
            _gameInputs.BotState = _otherCharacter.CurrentState;
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
        _animator = GetComponent<Animator>();
        _gameInputs = new GameInputs();
        _currentState = CharacterState.idle;
        _botMovementState = BotMovementState.idle;
    }

    // Update is called once per frame
    void FixedUpdate()
    {           
        UpdateGameInputs();
        UpdateMovementBotState();
        AgentBehaviour(); 
        Move();
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
        _healthBar.UpdateHealthBar(HP, MaxHP);
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
        SceneManager.LoadScene(2);
    }
}
