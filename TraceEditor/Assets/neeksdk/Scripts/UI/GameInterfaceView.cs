using System;
using UnityEngine;
using UnityEngine.UI;

namespace neeksdk.Scripts.UI
{
    public class GameInterfaceView : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;

        public Action OnCloseClick;
        
        public void SetupView()
        {
            _closeButton.onClick.AddListener(CloseButtonClick);
        }

        public void ClearView()
        {
            _closeButton.onClick.RemoveListener(CloseButtonClick);
        }

        private void CloseButtonClick() =>
            OnCloseClick?.Invoke();
    }
}
