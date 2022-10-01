using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum BehaviourEnum { player, bot };
public enum CharacterState { idle, attack, block, dash };

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

    // Is the character vulnerable or not. When it is invulnerable
    // it cannot be hit again, and cannot do any action
    private bool _invulnerable;


    [Header("HUD")]
    // Character health bar
    [SerializeField] private HealthBar _healthBar;

    // Character Weapon
    [SerializeField] private Weapon _weapon;

    // Character Shield
    [SerializeField] private Shield _shield;

    // Character movement and rotation
    private Vector2 _move;
    private float _rotation;

    // Animator
    private Animator _animator;


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

    private void MovePlayer()
    {
        if (!_invulnerable)
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
        // Can only attack if weapon is not on cooldown
        if (!_weapon.WeaponCooldown && !_invulnerable && !_shield.IsBlocking)
        {
            _weapon.Attack();
            _weapon.WeaponCooldown = true;
            StartCoroutine(_weapon.CooldownAttack());
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
        if (!_invulnerable && !_weapon.IsAttacking)
        {
            _shield.Block();
            _shield.IsBlocking = true;
        }
    }

    private void Unblock()
    {
        if (_shield.IsBlocking)
        {
            _shield.Unblock();
            _shield.IsBlocking = false;
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
        if (!_invulnerable && !_weapon.IsAttacking && !_shield.IsBlocking)
        {
            StartCoroutine(IFrames(_dashIFrames));

            GetComponent<Rigidbody2D>().AddForce(transform.up * _dashStrength);
            transform.rotation = Quaternion.Euler(0, 0, _rotation);
        }
    }

    public void OnAdditional(InputAction.CallbackContext ctx)
    {
        if (!_invulnerable && _behaviour == BehaviourEnum.player)
        {
            Debug.Log("Additional");
        }
    }

    // -------------------------------------------------
    // ----------------- Bot Behaviour -----------------
    // -------------------------------------------------
    private void AgentBehaviour()
    {
        if (_behaviour == BehaviourEnum.bot)
        {
            _move = new Vector2(Random.value - 0.5f, Random.value - 0.5f);
            _move.Normalize();

            // Only update rotation if is moving, otherwise keep old rotation
            if (_move != new Vector2(0, 0))
            {
                // Block rotation if attacking
                // if (_weapon.IsAttacking == false)
                // {
                _rotation = Mathf.Atan2(-_move.x, _move.y)* Mathf.Rad2Deg;
                // }
            }

            if (Random.value < 0.02)
            {
                Attack();
            }
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

    }

    // Update is called once per frame
    void FixedUpdate()
    {           
        AgentBehaviour(); 
        MovePlayer();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {   
        // Only check if player is vulnerable 
        if (!_invulnerable)
        {
            // First check that it is a weapon
            if (other.gameObject.tag == "Weapon")
            {   
                Weapon otherWeapon = other.GetComponent<Weapon>();
                // Check it is not its own weapon, and that it can attack
                if (otherWeapon != _weapon && otherWeapon.IsAttacking)
                {

                    Vector3 hitDirection = transform.position - otherWeapon.transform.position;
                    hitDirection.Normalize();

                    // Angle between the two collisions
                    float angleHit = Mathf.DeltaAngle(transform.localEulerAngles.z, otherWeapon.transform.eulerAngles.z);

                    // Get in absolute value, which would be at most 180. Then remove 180, and the closer to 0, the closer
                    // the frontal collision
                    angleHit = Mathf.Abs(Mathf.Abs(angleHit) - 180);

                    // If the angle is smaller than a threshold, the hit has been blocked
                    if (_shield.IsBlocking && angleHit < _shield.ProtectionDegrees)
                    {
                        BlockHit(hitDirection, _blockKnockback);
                        Character attacker = other.transform.parent.gameObject.GetComponent<Character>();
                        attacker.AttackIsBlocked(-hitDirection, _shield.Strength);

                    } 
                    else
                    {
                        GetHit(hitDirection, otherWeapon.Strength);
                    }
                }
            }
        }
    }

    private void BlockHit(Vector3 direction, int strength)
    {
        StartCoroutine(IFrames(_iFramesHit));

        // Move in the opposite direction with certain strength
        GetComponent<Rigidbody2D>().AddForce(direction * strength);
    }

    public void AttackIsBlocked(Vector3 direction, int strength)
    {
        StartCoroutine(IFrames(_iFramesHit));

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
        _invulnerable = true;
        _animator.SetTrigger("Invulnerable");
        yield return new WaitForSeconds(iFrames);
        _invulnerable = false;
        _animator.SetTrigger("Vulnerable");
    }
    
    private void Die()
    {
        SceneManager.LoadScene(2);
    }
}
