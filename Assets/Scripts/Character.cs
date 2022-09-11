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

    // Character health
    [SerializeField] private int _hp;
    public int HP {
        get {
            return _hp;
        }
        set {
            _hp = Mathf.Max(value, 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 movementDirection = new Vector2(horizontalInput, verticalInput);

        // Normalize direction so diagonal movements are not faster
        movementDirection.Normalize();

        transform.Translate(movementDirection * Speed * Time.deltaTime);

        Debug.Log(Speed);
    }

}
