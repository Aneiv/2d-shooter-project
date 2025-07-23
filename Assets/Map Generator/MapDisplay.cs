using UnityEngine;
using System.Collections;
using static UnityEngine.Mesh;

public class MapDisplay : MonoBehaviour
{

    public GameObject textureRender;
    public void DrawTexture(Texture2D texture)
    {
        MeshRenderer renderer = textureRender.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
            renderer.sharedMaterial.mainTexture = texture;
        }

        //scale adjustment
        textureRender.transform.localScale = new Vector3(10f,10f, 1f);//new Vector3(texture.width*textureScale, texture.height*textureScale, 1);
    }

}