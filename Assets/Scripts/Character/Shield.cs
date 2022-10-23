using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    // Animator
    private Animator _animator;

    // Degrees where the block is effetcive 
    [SerializeField] private float _protectionDegrees;
    public float ProtectionDegrees {
        get {
            return _protectionDegrees;
        }
    }

    // Knockback the shield makes the hitter go back
    [SerializeField] private int _strength;
    public int Strength {
        get {
            return _strength;
        }
    }

    private bool _isBlocking;
    public bool IsBlocking {
        get {
            return _isBlocking;
        }
        set {
            _isBlocking = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();  
    }

    public void Block()
    {
        _animator.SetTrigger("Block");
    }

    public void Unblock()
    {
        _animator.SetTrigger("Unblock");
    }
}
