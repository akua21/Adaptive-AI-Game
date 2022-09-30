using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class Character : MonoBehaviour
{
    // Indicates if it can be controlled by the user
    [SerializeField] private bool _isPlayer;

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

    // Invulnerability frames after hit  in seconds
    [SerializeField] private float _iFramesHit; 

    // Dash
    [SerializeField] private float _dashIFrames;
    [SerializeField] private float _dashStrength;

    // Is the character vulnerable or not. When it is invulnerable
    // it cannot be hit again, and cannot do any action
    private bool _invulnerable;


    // Character health bar
    [SerializeField] private HealthBar _healthBar;

    // Character Weapon
    [SerializeField] private Weapon _weapon;

    // Character Shield
    [SerializeField] private Shield _shield;

    // Character movement and rotation
    private Vector2 _move;
    private float _rotation;


    // -------------------- ACTIONS --------------------

    public void OnMove(InputAction.CallbackContext ctx)
    {   
        if (_isPlayer)
        {
            _move = ctx.ReadValue<Vector2>();
            // Only update rotation if is moving, otherwise keep old rotation
            if (ctx.ReadValue<Vector2>() != new Vector2(0, 0))
            {
                // Block rotation if attacking
                if (_weapon.IsAttacking == false)
                {
                    _rotation = Mathf.Atan2(-ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y)* Mathf.Rad2Deg;
                }
            }
        }
    }

    private void MovePlayer()
    {
        if (!_invulnerable)
        {
            // Movement
            GetComponent<Rigidbody2D>().velocity = _move * Speed * Time.deltaTime;

            // Rotation
            transform.rotation = Quaternion.Euler(0, 0, _rotation);
        }
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (_isPlayer && ctx.performed)
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
        if (_isPlayer)
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
        if (_isPlayer && ctx.performed)
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
        // if (!_invulnerable && _isPlayer)
        // {
        //     Debug.Log("Additional");
        // }
    }

    // -------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        HP = MaxHP;
    }

    // Update is called once per frame
    void FixedUpdate()
    {        
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
                    if (_shield.IsBlocking && angleHit < 50)
                    {
                        BlockHit(hitDirection, 50);
                        Character attacker = other.transform.parent.gameObject.GetComponent<Character>();
                        attacker.AttackIsBlocked(-hitDirection, 100);

                    } 
                    else
                    {
                        GetHit(hitDirection, 200);
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
        yield return new WaitForSeconds(iFrames);
        _invulnerable = false;
    }
    
    private void Die()
    {
        if (_isPlayer)
        {
            SceneManager.LoadScene(2);
        }
    }
}
