using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button buttonComp;

    [Header("Scaling Settings")]
    [SerializeField] private float hoverScaleMultiplier = 1.12f;
    [SerializeField] private float transitionSpeed = 18f;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        buttonComp = GetComponent<Button>();
    }

    private void Update()
    {
        // Smoothly lerp towards target scale using unscaled delta time (so pause menus don't freeze it)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Do not scale if the button is disabled/grayed out
        if (buttonComp != null && !buttonComp.interactable) return;

        targetScale = originalScale * hoverScaleMultiplier;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    private void OnDisable()
    {
        // Reset scale immediately if panel closes or button disables
        transform.localScale = originalScale;
        targetScale = originalScale;
    }
}
