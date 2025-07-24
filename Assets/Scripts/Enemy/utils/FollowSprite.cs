using UnityEngine;

public class FollowSprite : MonoBehaviour
{
    public Transform lider;
    public float xOffset;
    public float yOffset;
    private bool isFollowing = false;
    private void FixedUpdate()
    {
        if (isFollowing)
        {
            transform.position = new Vector3(lider.position.x + xOffset, lider.position.y + yOffset, lider.position.z);
        }
    }

    public void StartFollow()
    {
        isFollowing = true;
    }
}
