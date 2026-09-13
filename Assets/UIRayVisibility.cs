using UnityEngine;

public class UIRayVisibility : MonoBehaviour
{
    [SerializeField]
    private GameObject rayVisual;

    private int uiHoverCount = 0;

    private void Start()
    {
        if (rayVisual != null)
            rayVisual.SetActive(false);
    }

    public void UIHoverEntered()
    {
        uiHoverCount++;

        if (rayVisual != null)
            rayVisual.SetActive(true);
    }

    public void UIHoverExited()
    {
        uiHoverCount--;

        if (uiHoverCount <= 0)
        {
            uiHoverCount = 0;

            if (rayVisual != null)
                rayVisual.SetActive(false);
        }
    }
}