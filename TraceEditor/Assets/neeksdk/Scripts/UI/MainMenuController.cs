using System.Collections.Generic;
using System.IO;
using neeksdk.Scripts.Constants;
using neeksdk.Scripts.StaticData.LevelData;
using UnityEngine;

namespace neeksdk.Scripts.UI
{
    public class MainMenuController
    {
        private MainMenuView _mainMenuView;

        public void SetupMainMenu()
        {
            _mainMenuView = GameObject.FindObjectOfType<MainMenuView>();
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, RedactorConstants.LEVEL_PATH));
            FileInfo[] fileInfos = directoryInfo.GetFiles("*.dat");
            List<LevelData> levelDataList = new List<LevelData>();
            foreach (FileInfo fileInfo in fileInfos)
            {
                LevelData levelData = new LevelData()
                {
                    ShortName = fileInfo.Name,
                    Path = fileInfo.FullName
                };
                
                levelDataList.Add(levelData);
            }
            
            _mainMenuView.SetupView(levelDataList);
        }

        public void ClearMainMenu() =>
            _mainMenuView.ClearView();
    }
}