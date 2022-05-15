using System;
using neeksdk.Scripts.StaticData.LevelData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace neeksdk.Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        private Button _levelButton;
        private LevelData _level;
        
        public static Action<string> OnLevelClicked;

        public void SetupLevelButton(LevelData level)
        {
            _level = level;
            _titleText.text = level.ShortName;
        }
        
        private void Awake()
        {
            _levelButton = GetComponent<Button>();
            _levelButton.onClick.AddListener(ButtonPressed);
        }

        private void ButtonPressed()
        {
            OnLevelClicked?.Invoke(_level.Path);
        }

        private void OnDestroy()
        {
            _levelButton.onClick.RemoveListener(ButtonPressed);
        }
    }
}
