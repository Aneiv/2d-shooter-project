using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public SpriteRenderer textureRender;

    public void DrawTexture(Texture2D texture) {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
        
        //sprite from texture
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        //asign sprite to SpriteRenderer
        textureRender.sprite = sprite;

        //scale
        textureRender.transform.localScale = new Vector3(texture.width, texture.height, 1);

    }

}
