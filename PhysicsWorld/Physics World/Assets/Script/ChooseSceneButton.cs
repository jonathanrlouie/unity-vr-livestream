namespace VRTK.NewScripts
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System;
    using System.Collections.Generic;

    public class ChooseSceneButton : VRTK_InteractableObject
    {
        public GameObject teleButton;
        public static List<String> list = new List<String>();
        private int index = 0;
        private int size;

        private void Start()
        {
            AddScenes();
        }

        public override void StartUsing(GameObject usingObject)
        {
            base.StartUsing(usingObject);
            NextScene();
        }

        private void AddScenes()
        {
            list.Add("Test_Scene_White");
            size++;
            list.Add("Test_Scene_Red");
            size++;
            list.Add("Test_Scene_Blue");
            size++;
        }

        private void NextScene()
        {
            teleButton.GetComponent<OpenNewSceneButton>().toOpen = list[index % size];
            index++;
        }
    }
}