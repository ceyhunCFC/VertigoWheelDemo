using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoWheel.UI
{
    public class GameOverPanelView : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Button giveUpButton;
        [SerializeField, HideInInspector] private Button reviveGoldButton;
        [SerializeField, HideInInspector] private Button reviveVideoButton;
        [SerializeField, HideInInspector] private TMP_Text titleText;
        [SerializeField, HideInInspector] private TMP_Text descriptionText;
        [SerializeField, HideInInspector] private RectTransform deathCardTransform;

        public event System.Action GiveUpClicked;
        public event System.Action ReviveClicked;

        private void Awake()
        {
            AutoWire();
            BindButtons();
        }

        private void OnDestroy()
        {
            UnbindButtons();
            transform.DOKill();
            if (deathCardTransform != null)
            {
                deathCardTransform.DOKill();
            }
        }

        private void OnValidate()
        {
            AutoWire();
        }

        public void Show()
        {
            AutoWire();
            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = "OH NO, A BOMB EXPLODED RIGHT IN YOUR HANDS!";
            }

            if (descriptionText != null)
            {
                descriptionText.text = "Revive yourself to keep your rewards.";
            }

            CanvasGroup canvasGroup = GetOrCreateCanvasGroup();
            canvasGroup.alpha = 0f;
            transform.DOKill();
            canvasGroup.DOFade(1f, 0.18f);

            if (deathCardTransform != null)
            {
                deathCardTransform.DOKill();
                deathCardTransform.localScale = Vector3.one * 0.9f;
                deathCardTransform.DOScale(1f, 0.22f).SetEase(Ease.OutBack);
            }
        }

        public void Hide()
        {
            HideImmediate();
        }

        private void HideImmediate()
        {
            gameObject.SetActive(false);
        }

        private void BindButtons()
        {
            UnbindButtons();

            if (giveUpButton != null)
            {
                giveUpButton.onClick.AddListener(OnGiveUpClicked);
            }

            if (reviveGoldButton != null)
            {
                reviveGoldButton.onClick.AddListener(OnReviveClicked);
            }

            if (reviveVideoButton != null)
            {
                reviveVideoButton.onClick.AddListener(OnReviveClicked);
            }
        }

        private void UnbindButtons()
        {
            if (giveUpButton != null)
            {
                giveUpButton.onClick.RemoveListener(OnGiveUpClicked);
            }

            if (reviveGoldButton != null)
            {
                reviveGoldButton.onClick.RemoveListener(OnReviveClicked);
            }

            if (reviveVideoButton != null)
            {
                reviveVideoButton.onClick.RemoveListener(OnReviveClicked);
            }
        }

        private void OnGiveUpClicked()
        {
            GiveUpClicked?.Invoke();
        }

        private void OnReviveClicked()
        {
            ReviveClicked?.Invoke();
        }

        private CanvasGroup GetOrCreateCanvasGroup()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            return canvasGroup;
        }

        [ContextMenu("Auto Wire")]
        private void AutoWire()
        {
            if (giveUpButton == null)
            {
                giveUpButton = FindButton("ui_button_game_over_give_up");
            }

            if (reviveGoldButton == null)
            {
                reviveGoldButton = FindButton("ui_button_game_over_revive_gold");
            }

            if (reviveVideoButton == null)
            {
                reviveVideoButton = FindButton("ui_button_game_over_revive_video");
            }

            if (titleText == null)
            {
                titleText = FindText("ui_text_game_over_title_value");
            }

            if (descriptionText == null)
            {
                descriptionText = FindText("ui_text_game_over_description_value");
            }

            if (deathCardTransform == null)
            {
                Transform deathCard = FindTransform("ui_image_game_over_death_card_bg");
                deathCardTransform = deathCard as RectTransform;
            }
        }

        private Button FindButton(string objectName)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button.name.Trim() == objectName)
                {
                    return button;
                }
            }

            return null;
        }

        private TMP_Text FindText(string objectName)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (text.name.Trim() == objectName)
                {
                    return text;
                }
            }

            return null;
        }

        private Transform FindTransform(string objectName)
        {
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            foreach (Transform child in transforms)
            {
                if (child.name.Trim() == objectName)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
