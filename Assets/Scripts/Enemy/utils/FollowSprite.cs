using UnityEngine;

public class FollowSprite : MonoBehaviour
{
    public Transform lider;
    public float xOffset;
    public float yOffset;
    private void Update()
    {
        transform.position = new Vector3(lider.position.x + xOffset, lider.position.y + yOffset, lider.position.z);
    }
}
