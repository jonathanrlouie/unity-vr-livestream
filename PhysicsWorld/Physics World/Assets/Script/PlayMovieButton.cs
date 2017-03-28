namespace VRTK.NewScripts
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System;
    using System.Collections.Generic;

    public class PlayMovieButton : VRTK_InteractableObject
    {
        public GameObject screen;
        private Renderer r;
        private MovieTexture movieTex;
        private int isPlaying = 0;
        public Material black;
        public Material movie;
        private int firstPlay = 0;

        private void Awake()
        {
            r = screen.GetComponent<Renderer>();
            r.material = black;
        }

        public override void StartUsing(GameObject usingObject)
        {
            base.StartUsing(usingObject);
            if (firstPlay == 0)
            {
                firstPlay = 1;
                r.material = movie;
                movieTex = (MovieTexture)r.material.mainTexture;
            }
            PlayMovie();
        }

        private void PlayMovie()
        {
            if (movieTex != null && isPlaying == 0)
            {
                movieTex.Play();
                isPlaying = 1;
            } else if (movieTex != null)
            {
                movieTex.Stop();
                isPlaying = 0;
            }
        }
    }
}