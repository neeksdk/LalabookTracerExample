using UnityEngine;
using UnityEngine.UI;

namespace neeksdk.Scripts.LevelCreator
{
    public class Stage : MonoBehaviour {
        private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        private Color _enabledColor = new Color(0, 0, 0, 1);
        [HideInInspector]
        public Image starImage;

        public Image waitImage;
        [HideInInspector]
        public TMPro.TMP_Text stageNum;

        [SerializeField] public int myStageInfo;
        private static readonly int Press = Animator.StringToHash("Pressed");

        [ExecuteAlways]
        public void SetText(int num) {
            stageNum.text = $"{num:000}";
            myStageInfo = num;
        }

        [ExecuteInEditMode]
        public void SetupFromMemory(int stage) {
            myStageInfo = stage;
            stageNum.text = $"{stage:000}";
        }

        public void StagePressed() {
            if (waitImage.fillAmount > 0) return;
            //GetComponent<Animator>().SetTrigger(Press);
            // GameController.Instance.StartNewStage(myStageInfo, this);
        }

        public void SetTextAndStars(int stars, int txt) {
            SetStars(stars);
            stageNum.text = txt.ToString("00");
        }

        private void Start() {
            //if (myStageInfo.locked) {
            //    GetComponent<Button>().interactable = false;
            //}
        }

        public void ChangeLockedStatus(bool isButtonInteractable) {
            GetComponent<Button>().interactable = isButtonInteractable;
            SetColor();
        }

        private void SetColor() {
            if (GetComponent<Button>().interactable) {
                starImage.color = _enabledColor;
                stageNum.color = Color.black;
            } else {
                starImage.color = _disabledColor;
                stageNum.color = _disabledColor;  
            }
        }

        public void SetStars(int stars) {
            if (stars == -1) {
                GetComponent<Button>().interactable = false;
            } else {
                GetComponent<Button>().interactable = true;
            }
            //_enabledColor = GameController.Instance.StarColor(stars);
            SetColor();
            switch (stars) {
                case 1:
                    starImage.fillAmount = 0.33f;
                    break;
                case 2:
                    starImage.fillAmount = 0.67f;
                    break;        
                case 3:
                    starImage.fillAmount = 1f;
                    break;
                default:
                    starImage.fillAmount = 0f;
                    break;
            }
        }
    }
}
