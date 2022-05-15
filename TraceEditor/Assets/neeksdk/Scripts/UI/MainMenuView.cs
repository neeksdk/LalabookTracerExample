using System.Collections.Generic;
using neeksdk.Scripts.StaticData.LevelData;
using UnityEngine;

namespace neeksdk.Scripts.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Transform _levelLayoutTransform;
        [SerializeField] private LevelButton _levelButtonPrefab;

        private List<LevelButton> _levelButtons = new List<LevelButton>();

        public void SetupView(List<LevelData> levels)
        {
            foreach (LevelData level in levels)
            {
                GameObject go = Instantiate(_levelButtonPrefab.gameObject, _levelLayoutTransform);
                LevelButton lb = go.GetComponent<LevelButton>();
                lb.SetupLevelButton(level);
                _levelButtons.Add(lb);
            }    
        }

        public void ClearView()
        {
            foreach (LevelButton levelButton in _levelButtons)
            {
                Destroy(levelButton);
            }
            
            _levelButtons.Clear();
        }
    }
}
