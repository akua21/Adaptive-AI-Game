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
                _rotation = Mathf.Atan2(-ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y)* Mathf.Rad2Deg;
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
        if (_isPlayer)
        {
            Attack();
        }
    }

    private void Attack ()
    {
        // Can only attack if weapon is not on cooldown
        if (!_weapon.WeaponCooldown && !_invulnerable)
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
            Debug.Log("Block");
            Block();
        }
    }

    private void Block()
    {
        if (!_invulnerable)
        {
            
        }
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (_isPlayer)
        {
            Dash();
        }
    }

    private void Dash()
    {
        if (!_invulnerable)
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
                if (otherWeapon != _weapon && otherWeapon.CanAttack)
                {

                    Vector3 hitDirection = transform.position - otherWeapon.transform.position;
                    hitDirection.Normalize();

                    GetHit(hitDirection, 200);
                }
            }
        }
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
