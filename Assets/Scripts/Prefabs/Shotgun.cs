using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shotgun : MonoBehaviour
{

    public GameObject bulletPrefab;
    public Transform shootingPoint;
    public float fireRate;
    public float bulletVelocity;
    public int pelletsPerShot = 8;
    public float spreadAngle = 15f;
    private bool canShoot = true;
    public bool isAutomatic;

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
      
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 shootingPointPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = (shootingPointPosition - (Vector2)shootingPoint.position).normalized;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            var bullet = Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity);
            bullet.layer = LayerMask.NameToLayer("PlayerBullet");
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            float angle = Random.Range(-spreadAngle / 2, spreadAngle / 2);
            direction = Quaternion.Euler(0, 0, angle) * direction;

            rb.velocity = direction * bulletVelocity;
            shootSound.Play();

            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            bulletScript.shooter = Parent;
        }
            yield return new WaitForSeconds(fireRate);
            canShoot = true;
    }
}
