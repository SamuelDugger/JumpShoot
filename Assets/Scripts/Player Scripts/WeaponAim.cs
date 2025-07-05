using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAim : MonoBehaviour
{

    public GameObject player;

    void Update()
    {
        turnPlayer();
        Aim();
    }

    void Aim()
    {
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;


        difference.Normalize();

        float angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg + 90;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void turnPlayer()
    {
        Vector2 mousePosition = Input.mousePosition;

        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Vector2 direction = new Vector2(mousePosition.x - player.transform.position.x, mousePosition.y - player.transform.position.y);


        if (mousePosition.x > player.transform.position.x) //Mouse to the right
        {
            if (player.transform.localScale.x < 0) //Facing Right
            {
                Vector3 scale = player.transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                player.transform.localScale = scale;
            }
        }
        else if (mousePosition.x < player.transform.position.x) // Mouse to the left
        {
            if (player.transform.localScale.x > 0) //Facing Left
            {
                Vector3 scale = player.transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                player.transform.localScale = scale;
            }
        }
    }
}
