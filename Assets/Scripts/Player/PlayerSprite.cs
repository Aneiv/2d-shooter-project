using UnityEngine;
using Mirror;
public class PlayerSprite : Mirror.NetworkBehaviour
{
    public Sprite[] playerSprites;

    [SyncVar(hook = nameof(OnSpriteChanged))]
    private int currSpriteIndex;

    public void SetSpriteIndex(int spriteIndex)
    {
        currSpriteIndex = spriteIndex;
    }

    private void OnSpriteChanged(int oldIndex, int newIndex)
    {
        GetComponent<SpriteRenderer>().sprite = playerSprites[newIndex];
    }
}
