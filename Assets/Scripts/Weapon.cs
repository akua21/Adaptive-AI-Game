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
    }

    // Animator
    private Animator _animator;

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
    }
 
    public void Attack()
    {
        _animator.SetTrigger("Attack");
    }

    public IEnumerator CooldownAttack()
    {
        yield return new WaitForSeconds(AttackDelay);
        WeaponCooldown = false;
    }

    // Allow attacking (Triggered on the animation)
    public void AllowAttack()
    {
        IsAttacking = true;
    }   

    // Disallow attacking (Triggered on the animation)
    public void DisallowAttack()
    {
        IsAttacking = false;
    }
}
