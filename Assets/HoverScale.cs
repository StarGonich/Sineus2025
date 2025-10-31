using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverScale : MonoBehaviour
{
    public void OnHoverEnterEffect(GameObject go)
    {
        go.transform.localScale = new Vector3(1.2f, 1.2f, 1);
    }

    public void OnHoverExitEffect(GameObject go)
    {
        go.transform.localScale = Vector3.one;
    }
}