using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    // Animator
    private Animator _animator;

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
