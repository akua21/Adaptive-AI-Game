using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

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


    // Start is called before the first frame update
    void Start()
    {
        HP = MaxHP;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 movementDirection = new Vector2(horizontalInput, verticalInput);

        // Normalize direction so diagonal movements are not faster
        movementDirection.Normalize();

        GetComponent<Rigidbody2D>().velocity = movementDirection * Speed * Time.deltaTime;

        _healthBar.UpdateHealthBar(HP, MaxHP);

    }

}
