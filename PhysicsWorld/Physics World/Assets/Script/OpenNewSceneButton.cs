namespace VRTK.NewScripts
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System;
    using System.Collections.Generic;

    public class OpenNewSceneButton : VRTK_InteractableObject
    {
        public String toOpen;

        public override void StartUsing(GameObject usingObject)
        {
        base.StartUsing(usingObject);
            PushButton();
        }

        private void PushButton()
        {
            if (toOpen != null)
            {
                SceneManager.LoadScene(toOpen);
            }
        }
    }
}