using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace neeksdk.Scripts.UI
{
    public class GameInterfaceView : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _informText;

        public Action OnCloseClick;
        
        public void SetupView()
        {
            _closeButton.onClick.AddListener(CloseButtonClick);
           EmptyInformText();
        }

        public void ClearView()
        {
            _closeButton.onClick.RemoveListener(CloseButtonClick);
            EmptyInformText();
        }

        public void SetInformText(string text) => _informText.text = text;
        public void EmptyInformText() => _informText.text = "";

        private void CloseButtonClick() =>
            OnCloseClick?.Invoke();
    }
}
