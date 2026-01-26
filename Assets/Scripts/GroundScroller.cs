using UnityEngine;
using UnityEngine.UI;

public class GroundScroller : MonoBehaviour
{
    public float scrollSpeed = 100f;
    private RawImage rawImage;
    private Rect uvRect;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        if (rawImage != null)
        {
            uvRect = rawImage.uvRect;
        }
    }

    void Update()
    {
        if (DinoGameManager.Instance != null && DinoGameManager.Instance.IsGameOver())
            return;

        if (rawImage != null)
        {
            float speed = DinoGameManager.Instance != null ? DinoGameManager.Instance.GetCurrentSpeed() : scrollSpeed;
            uvRect.x += speed * Time.deltaTime * 0.01f;
            rawImage.uvRect = uvRect;
        }
    }
}
