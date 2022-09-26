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
    [SerializeField] private int _attackStrength;
    public float AttackStrength {
        get {
            return _attackStrength;
        }
    }

    // Animator
    private Animator _animator;

    // Boolean that indicates that it is attacking. Can only hit if 
    // it is
    private bool _canAttack;
    public bool CanAttack {
        get {
            return _canAttack;
        }
        private set {
            _canAttack = value;
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
        CanAttack = true;
    }   

    // Disallow attacking (Triggered on the animation)
    public void DisallowAttack()
    {
        CanAttack = false;
    }
}
