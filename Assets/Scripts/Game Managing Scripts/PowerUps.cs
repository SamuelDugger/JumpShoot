using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerUps : MonoBehaviour
{

    public static PowerUps Instance { get; private set; }

    [SerializeField] PlayerHealth playerHealth;  // Reference to the PlayerHealth component
    [SerializeField] Transform playerArm;
    [SerializeField] GameObject bulletPrefab;

    private GameObject pistol;
    private GameObject shotgun;
    private GameObject smg;
    private GameObject ar;
    private GameObject sniper;

    [SerializeField] TextMeshProUGUI healthText;

    [SerializeField] TextMeshProUGUI weaponText;

    [SerializeField] TextMeshProUGUI movementText;

    [SerializeField] TextMeshProUGUI bulletText;

    private float baseMaxHealth = 100f;  // Base max health without any modifiers

    public enum Weapon
    {
        Pistol,
        Shotgun,
        SMG,
        AR,
        Sniper
    }

    public Weapon weapon;

    public enum Movement
    {
        Base,
        DoubleJump,
        DoubleDash
    }

    public enum Bullet
    {
        Standard,
        Poison,
        Explosive
    }

    public Movement movement;

    public Bullet bullet;

    // Health reduction constants for each weapon
    private const float PISTOL_HEALTH_DECREASE = 0f;
    private const float SHOTGUN_HEALTH_DECREASE = 20f;
    private const float SMG_HEALTH_DECREASE = 25f;
    private const float AR_HEALTH_DECREASE = 30f;
    private const float SNIPER_HEALTH_DECREASE = 30f;

    // Health reduction constants for each Movement
    private const float BASE_HEALTH_DECREASE = 0f;
    private const float DBJMP_HEALTH_DECREASE = 20f;
    private const float DBDASH_HEALTH_DECREASE = 30f;

    // Health reduction constants for each Bullet

    private const float STANDARD_HEALTH_DECREASE = 0f;
    private const float POISON_HEALTH_DECREASE = 20f;
    private const float EXPLOSIVE_HEALTH_DECREASE = 30f;

    // Store modifiers for different types of power-ups
    private float weaponHealthModifier = 0f;
    private float movementHealthModifier = 0f;

    private float bulletHealthModifier = 0f;



    private void Start()
    {
        // Set the static instance to this PowerUps object
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple PowerUps instances found! Destroying duplicate.");
            Destroy(this);
            return;
        }
        // Ensure that the player's weapon models are properly initialized.
        if (playerArm.childCount >= 5)
        {
            // Initialize weapon GameObjects
            pistol = playerArm.GetChild(0).gameObject;
            shotgun = playerArm.GetChild(1).gameObject;
            smg = playerArm.GetChild(2).gameObject;
            ar = playerArm.GetChild(3).gameObject;
            sniper = playerArm.GetChild(4).gameObject;

            // Load saved weapon choice and health
            weapon = (Weapon)PlayerPrefs.GetInt("SelectedWeapon", 0);
            movement = (Movement)PlayerPrefs.GetInt("SelectedMovement", 0);
            bullet = (Bullet)PlayerPrefs.GetInt("SelectedBullet", 0);
            baseMaxHealth = PlayerPrefs.GetFloat("BaseMaxHealth", 100f);
            weaponHealthModifier = PlayerPrefs.GetFloat("WeaponHealthModifier", 0f);
            movementHealthModifier = PlayerPrefs.GetFloat("MovementHealthModifier", 0f);
            bulletHealthModifier = PlayerPrefs.GetFloat("BulletHealthModifier", 0f);

            RecalculateHealth();

            // Debug.Log("Loaded weapon: " + weapon);
            // Debug.Log("Loaded player max health: " + playerHealth.maxHealth);

            playerHealth.UpdateHealthUI();

            getWeapon();
        }
        else
        {
            Debug.LogError("Player arm does not have enough children to assign weapons.");
        }
    }

    private void getWeapon()
    {
        switch (weapon)
        {
            case Weapon.Pistol:
                pistol.SetActive(true);
                shotgun.SetActive(false);
                smg.SetActive(false);
                ar.SetActive(false);
                sniper.SetActive(false);
                break;
            case Weapon.Shotgun:
                pistol.SetActive(false);
                shotgun.SetActive(true);
                smg.SetActive(false);
                ar.SetActive (false);
                sniper.SetActive(false);
                break;
            case Weapon.SMG:
                pistol.SetActive(false);
                shotgun.SetActive(false);
                smg.SetActive(true);
                ar.SetActive(false);
                sniper.SetActive(false);
                break;
            case Weapon.AR:
                pistol.SetActive(false);
                shotgun.SetActive(false);
                smg.SetActive(false);
                ar.SetActive(true);
                sniper.SetActive(false);
                break;
            case Weapon.Sniper:
                pistol.SetActive (false);
                shotgun.SetActive(false);
                smg.SetActive(false);
                ar.SetActive(false);
                sniper.SetActive(true);
                break;
            default:
                weapon = Weapon.Pistol;
                pistol.SetActive(true);
                shotgun.SetActive(false);
                smg.SetActive(false);
                ar.SetActive(false);
                sniper.SetActive(false);
                break;
        }
    }         

    public void clickPistolBtn()
    {
        if (weapon != Weapon.Pistol)
        {
            weapon = Weapon.Pistol;
            weaponHealthModifier = PISTOL_HEALTH_DECREASE;
            RecalculateHealth();
        }
        Debug.Log("Pistol button clicked");
        SavePowerUpState();
    }

    public void clickShotgunBtn()
    {
        if (weapon != Weapon.Shotgun)
        {
            weapon = Weapon.Shotgun;
            weaponHealthModifier = SHOTGUN_HEALTH_DECREASE;
            RecalculateHealth();
        }
        Debug.Log("Shotgun button clicked");
        SavePowerUpState();
    }

    public void clickSmgBtn()
    {
        if (weapon != Weapon.SMG)
        {
            weapon = Weapon.SMG;
            weaponHealthModifier = SMG_HEALTH_DECREASE;
            RecalculateHealth();
        }
        Debug.Log("SMG button clicked");
        SavePowerUpState();
    }

    public void clickARBtn()
    {
        if (weapon != Weapon.AR)
        {
            weapon = Weapon.AR;
            weaponHealthModifier = AR_HEALTH_DECREASE;
            RecalculateHealth();
        }
        Debug.Log("AR button clicked");
        SavePowerUpState();
    }
    public void clickSniperBtn()
    {
        if (weapon != Weapon.Sniper)
        {
            weapon = Weapon.Sniper;
            weaponHealthModifier = SNIPER_HEALTH_DECREASE;
            RecalculateHealth();
        }
        Debug.Log("Sniper button clicked");
        SavePowerUpState();
    }

    public void clickBaseBtn()
    {
        Debug.Log("Base button clicked");

        if (movement != Movement.Base)
        {
            Debug.Log("Changing movement to Base");
            movement = Movement.Base;
            movementHealthModifier = BASE_HEALTH_DECREASE;
            RecalculateHealth();
        }
        else
        {
            Debug.Log("Movement is " + movement);
        }

        SavePowerUpState();
    }


    public void clickDoubleJmpuBtn()
    {
        if (movement != Movement.DoubleJump)
        {
            movement = Movement.DoubleJump;
            movementHealthModifier = DBJMP_HEALTH_DECREASE;
            RecalculateHealth();
        }
        Debug.Log("Double Jump button clicked");
        Debug.Log("Movement is " + movement);
        SavePowerUpState();
    }

    public void clickDoubleDashBtn()
    {
        if (movement != Movement.DoubleDash)
        {
            movement = Movement.DoubleDash;
            movementHealthModifier = DBDASH_HEALTH_DECREASE;
            RecalculateHealth();
        }
        Debug.Log("Double Dash button clicked");
        Debug.Log("Movement is " + movement);
        SavePowerUpState();
    }

    public void clickStandardBtn()
    {
        if (bullet != Bullet.Standard)
        {
            bullet = Bullet.Standard;
            bulletHealthModifier = STANDARD_HEALTH_DECREASE;
            RecalculateHealth();
        }
        SavePowerUpState();
    }

    public void clickPoisonBtn()
    {
        if (bullet != Bullet.Poison)
        {
            bullet = Bullet.Poison;
            bulletHealthModifier = POISON_HEALTH_DECREASE;
            RecalculateHealth();
        }
        SavePowerUpState();
    }

    public void clickExplosiveBtn()
    {
        if (bullet != Bullet.Explosive)
        {
            bullet = Bullet.Explosive;
            bulletHealthModifier = EXPLOSIVE_HEALTH_DECREASE;
            RecalculateHealth();
        }
        SavePowerUpState();
    }

    // Recalculate max health based on all active modifiers
    private void RecalculateHealth()
    {
        playerHealth.maxHealth = baseMaxHealth - weaponHealthModifier - movementHealthModifier - bulletHealthModifier; // TODO Add other modifiers here
        playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth, playerHealth.maxHealth);

        healthText.text = "Total Health: " + playerHealth.maxHealth;
        weaponText.text = ("" + weapon);
        movementText.text = ("" + movement);
        bulletText.text = ("" + bullet);
    }

    private void SavePowerUpState()
    {
        PlayerPrefs.SetInt("SelectedWeapon", (int)weapon);
        PlayerPrefs.SetInt("SelectedMovement", (int)movement);
        PlayerPrefs.SetInt("SelectedBullet", (int)bullet);
        PlayerPrefs.SetFloat("BaseMaxHealth", baseMaxHealth);
        PlayerPrefs.SetFloat("WeaponHealthModifier", weaponHealthModifier);
        // TODO Add other modifiers here
        PlayerPrefs.SetFloat("MovementHealthModifier", movementHealthModifier);
        PlayerPrefs.SetFloat("BulletHealthModifier", bulletHealthModifier);
        PlayerPrefs.Save();
    }
}
