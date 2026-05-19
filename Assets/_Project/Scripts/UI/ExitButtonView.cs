using UnityEngine;
using UnityEngine.UI;

namespace VertigoWheel.UI
{
    public class ExitButtonView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button exitButton;

        private void OnValidate()
        {
            AutoWire();
        }

        [ContextMenu("Auto Wire")]
        private void AutoWire()
        {
            if (exitButton == null)
            {
                exitButton = FindButton("ui_button_exit");
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            if (exitButton != null)
            {
                exitButton.interactable = isInteractable;
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

            Debug.LogWarning($"Button not found: {objectName}", this);
            return null;
        }
    }
}