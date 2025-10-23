using UnityEngine;

[System.Serializable]
public struct BackgroundData
{
    public Renderer background;
}


public class PallaxBackGround : MonoBehaviour
{
    [SerializeField]
    private Transform targetCamera;
    [SerializeField]
    private BackgroundData[] backgrounds;
    [SerializeField]
    private BackgroundData backgroundObj;

    private float targetStartX;

    private void Awake()
    {
        targetStartX = targetCamera.position.x;
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            return;
        }

        float x = targetCamera.position.x - targetStartX;
        
    }
}
