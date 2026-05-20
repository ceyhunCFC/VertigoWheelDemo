using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoWheel.UI
{
    public class ExitConfirmPanelView : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Button collectButton;
        [SerializeField, HideInInspector] private Button goBackButton;
        [SerializeField, HideInInspector] private TMP_Text messageText;

        public event System.Action CollectClicked;
        public event System.Action GoBackClicked;

        private void Awake()
        {
            AutoWire();
            BindButtons();
        }

        private void OnDestroy()
        {
            UnbindButtons();
            transform.DOKill();
        }

        private void OnValidate()
        {
            AutoWire();
        }

        public void Show()
        {
            AutoWire();
            gameObject.SetActive(true);

            if (messageText != null)
            {
                messageText.text = "Want to exit and collect your rewards? The best ones are saved for the last!";
            }

            transform.DOKill();
            transform.localScale = Vector3.one * 0.98f;
            transform.DOScale(1f, 0.12f).SetEase(Ease.OutCubic);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void BindButtons()
        {
            UnbindButtons();

            if (collectButton != null)
            {
                collectButton.onClick.AddListener(OnCollectClicked);
            }

            if (goBackButton != null)
            {
                goBackButton.onClick.AddListener(OnGoBackClicked);
            }
        }

        private void UnbindButtons()
        {
            if (collectButton != null)
            {
                collectButton.onClick.RemoveListener(OnCollectClicked);
            }

            if (goBackButton != null)
            {
                goBackButton.onClick.RemoveListener(OnGoBackClicked);
            }
        }

        private void OnCollectClicked()
        {
            CollectClicked?.Invoke();
        }

        private void OnGoBackClicked()
        {
            GoBackClicked?.Invoke();
        }

        [ContextMenu("Auto Wire")]
        private void AutoWire()
        {
            if (collectButton == null)
            {
                collectButton = FindButton("ui_button_exit_confirm_collect");
            }

            if (goBackButton == null)
            {
                goBackButton = FindButton("ui_button_exit_confirm_go_back");
            }

            if (messageText == null)
            {
                messageText = FindText("ui_text_exit_confirm_message_value");
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
    }
}
