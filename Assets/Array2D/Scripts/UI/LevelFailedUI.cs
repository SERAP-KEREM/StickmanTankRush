using DG.Tweening;
using SerapKeremGameTools._Game._AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailedUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Animation Settings")]
    [SerializeField] private float showDelay = 0.5f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float buttonAnimDelay = 0.2f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        InitializeButtons();
        Hide(true);
    }

    private void InitializeButtons()
    {
        retryButton.onClick.AddListener(OnRetryClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show()
    {
        Debug.Log("[LevelFailedUI] Show called");
        gameObject.SetActive(true);

        // Reset everything
        ResetUI();

        // Start animation sequence
        DOVirtual.DelayedCall(showDelay, () => {
            // Panel fade in
            canvasGroup.DOFade(1f, fadeInDuration);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Title bounce
            titleText.transform.DOScale(1f, fadeInDuration)
                .SetEase(Ease.OutBack);

            // Buttons sequence
            DOVirtual.DelayedCall(buttonAnimDelay, () => {
                retryButton.transform.DOScale(1f, fadeInDuration)
                    .SetEase(Ease.OutBack);
            });

            DOVirtual.DelayedCall(buttonAnimDelay * 2, () => {
                mainMenuButton.transform.DOScale(1f, fadeInDuration)
                    .SetEase(Ease.OutBack);
            });

            // Sound effect
            AudioManager.Instance?.PlayAudio(AudioKeys.LEVEL_FAIL);
        });
    }

    private void ResetUI()
    {
        canvasGroup.alpha = 0f;
        titleText.transform.localScale = Vector3.zero;
        retryButton.transform.localScale = Vector3.zero;
        mainMenuButton.transform.localScale = Vector3.zero;
    }

    public void Hide(bool instant = false)
    {
        if (instant)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
        else
        {
            canvasGroup.DOFade(0f, fadeInDuration)
                .OnComplete(() => {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    gameObject.SetActive(false);
                });
        }
    }

    private void OnRetryClicked()
    {
        Debug.Log("[LevelFailedUI] Retry clicked");
        AudioManager.Instance?.PlayAudio(AudioKeys.UI_CLICK);
        Hide();
        LevelManager.Instance.RestartLevel();
    }

    private void OnMainMenuClicked()
    {
        Debug.Log("[LevelFailedUI] Main Menu clicked");
        AudioManager.Instance?.PlayAudio(AudioKeys.UI_CLICK);
        Hide();
        // TODO: Ana menüye dön
        // SceneManager.LoadScene("MainMenu");
    }
}