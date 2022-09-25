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

    // Invulnerability frames in seconds
    [SerializeField] private float _iFrames; 

    // Is the character vulnerable or not. When it is invulnerable
    // it cannot be hit again, and cannot move
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
        _move = ctx.ReadValue<Vector2>();
        _rotation = Mathf.Atan2(-ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y)* Mathf.Rad2Deg;

    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (_isPlayer)
        {
            if (!_weapon.WeaponCooldown)
            {
                _weapon.Attack();
                _weapon.WeaponCooldown = true;
                StartCoroutine(_weapon.CooldownAttack());
            }
        }
    }

    public void OnBlock(InputAction.CallbackContext ctx)
    {
        Debug.Log("Block");
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        Debug.Log("Dash");
    }

    public void OnAdditional(InputAction.CallbackContext ctx)
    {
        Debug.Log("Additional");
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
        if (!_invulnerable)
        {
            if (_isPlayer)
            {
                MovePlayer();
            }
            else
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private void MovePlayer()
    {
        // Movement
        GetComponent<Rigidbody2D>().velocity = _move * Speed * Time.deltaTime;

        // Rotation
        transform.rotation = Quaternion.Euler(0, 0, _rotation);
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

    

    private IEnumerator IFrames()
    {
        _invulnerable = true;
        yield return new WaitForSeconds(_iFrames);
        _invulnerable = false;
    }

    private void GetHit(Vector3 direction, int strength)
    {
        HP -= 1;
        _healthBar.UpdateHealthBar(HP, MaxHP);
        StartCoroutine(IFrames());

        // Move in the opposite direction with certain strength
        GetComponent<Rigidbody2D>().AddForce(direction * strength);
    }

    private void Die()
    {
        if (_isPlayer)
        {
            SceneManager.LoadScene(2);
        }
    }
}
