using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
        }
    }

    // Character health bar
    [SerializeField] private HealthBar _healthBar;


    // Character control inputs
    [SerializeField] private PlayerControl controls;

    // Character movement and rotation
    private Vector2 _move;
    private float _rotation;

    private void Awake() {
        controls = new PlayerControl();

        // Move controls
        controls.Gameplay.Move.performed += ctx => _move = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => _move = Vector2.zero;
        controls.Gameplay.Move.performed += ctx => _rotation = Mathf.Atan2(-ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y)* Mathf.Rad2Deg;


    }

    private void OnEnable() {
        controls.Gameplay.Enable();
    }

    private void OnDisable() {
        controls.Gameplay.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        HP = MaxHP;

    }

    // Update is called once per frame
    void FixedUpdate()
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

    private void MovePlayer()
    {
        // Movement
        GetComponent<Rigidbody2D>().velocity = _move * Speed * Time.deltaTime;

        // Rotation
        transform.rotation = Quaternion.Euler(0, 0, _rotation);
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.tag == "Character")
        {
            GetHit();
        }
    }

    private void GetHit()
    {
        HP -= 1;
        _healthBar.UpdateHealthBar(HP, MaxHP);
    }

}
