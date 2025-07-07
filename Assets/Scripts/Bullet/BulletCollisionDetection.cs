using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BulletCollisionDetection : MonoBehaviour
{
    private Vector3 bottomLeft;
    private Vector3 topRight;
    private Vector2 pos;
    private float leftXClamp, rightXClamp, downYClamp, upYClamp;
    private float clampSize = 0.5f;
    void Start()
    {
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        //clamp borders
        leftXClamp = bottomLeft.x - clampSize;
        rightXClamp = topRight.x + clampSize;
        downYClamp = bottomLeft.y - clampSize;
        upYClamp = topRight.y + clampSize;
    }

    void Update()
    {
        pos = transform.position;
        if (pos.x < leftXClamp || pos.x > rightXClamp || pos.y < downYClamp || pos.y > upYClamp)
        {
            Debug.Log("LOG Bullet hit screen bounds");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("LOG Bullet hit player");
            Destroy(gameObject);
        }        
    }

}
