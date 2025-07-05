using UnityEngine.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Pistol : MonoBehaviour
{

    public GameObject bulletPrefab;
    public Transform shootingPoint;
    public float fireRate;
    public float bulletVelocity;
    private bool canShoot = true;
    public bool isAutomatic = false;

    [SerializeField] AudioSource shootSound;

    public GameObject Parent;

    void Start()
    {
        GameObject arm = this.transform.parent.gameObject;
        GameObject player = arm.transform.parent.gameObject;
        Parent = player;
    }

    void Update()
    {  

        if (isAutomatic)
        {
            HandleAutomaticFire();
        }
        else
        {
            HandleSemiAutomaticFire();
        }
    
    }
    private void HandleAutomaticFire()
    {
        if (Mouse.current.leftButton.isPressed && canShoot) 
        {
            StartCoroutine(Shoot());
        }
    }
    private void HandleSemiAutomaticFire()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && canShoot)
        {

            StartCoroutine(Shoot());
        }
    }
    private IEnumerator Shoot()
    {
        canShoot = false;
      
        var bullet = Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity);
        bullet.layer = LayerMask.NameToLayer("PlayerBullet");
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 shootingPointPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = (shootingPointPosition - (Vector2)shootingPoint.position).normalized;

        rb.velocity = direction * bulletVelocity;
        shootSound.Play();

        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        bulletScript.shooter = Parent;

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }    
}
