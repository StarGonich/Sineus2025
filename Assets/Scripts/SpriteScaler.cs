using UnityEngine;

public class SpriteScaler : MonoBehaviour
{
    [Range(0.5f, 5f)]
    public float scale = 2.0f;

    void Start()
    {
        ScaleSprite();
    }

    void OnValidate()
    {
        if (Application.isPlaying)
            ScaleSprite();
    }

    void ScaleSprite()
    {
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}