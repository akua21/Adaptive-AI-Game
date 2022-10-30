using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{   

    // Attack delay
    [SerializeField] private float _attackDelay;
    public float AttackDelay {
        get {
            return _attackDelay;
        }
    }

    // Weapon is on cooldown. Indicates if can attack again yet
    private bool _weaponCooldown;
    public bool WeaponCooldown {
        get {
            return _weaponCooldown;
        }
        set {
            _weaponCooldown = value;
        }
    }

    // Strenght damage (nockback)
    [SerializeField] private int _strength;
    public int Strength {
        get {
            return _strength;
        }
        set {
            _strength = value;
        }
    }

    // Animator
    private Animator _animator;

    // Parent character
    private Character _parentCharacter;
    public Character ParentCharacter {
        get {
            return _parentCharacter;
        }
    }

    // Boolean that indicates that it is attacking. Can only hit if 
    // it is
    private bool _isAttacking;
    public bool IsAttacking {
        get {
            return _isAttacking;
        }
        private set {
            _isAttacking = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();    
        _parentCharacter = transform.parent.gameObject.GetComponent<Character>();
    }
 
    public void Attack()
    {
        _animator.SetTrigger("Attack");
        StartCoroutine(CooldownAttack());
        _isAttacking = true;
    }

    public IEnumerator CooldownAttack()
    {
        WeaponCooldown = true;
        yield return new WaitForSeconds(AttackDelay);
        WeaponCooldown = false;
    }
 
    // Stop attacking (Triggered on the animation)
    public void StopAttack()
    {
        if (_parentCharacter.CurrentState == CharacterState.attack)
        {
            _parentCharacter.CurrentState = CharacterState.idle;
        };
        _isAttacking = false;   
    }
}
