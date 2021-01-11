﻿using UnityEngine;

namespace Maroon
{
    public class BootstrappingManager : MonoBehaviour
    {
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Fields

        // Singleton instance
        private static BootstrappingManager instance = null;

        // Settings
        [SerializeField] private bool webGLEnableSceneLoadingViaURLParameter = true;
        [SerializeField] private string webGLSceneURLParameterName = "LoadScene";
        [SerializeField] private Maroon.CustomSceneAsset firstStandardScene = null;
        [SerializeField] private Maroon.CustomSceneAsset firstVRScene = null;

        // State
        private bool bootstrappingFinished = false;

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Getters and Properties

        public static BootstrappingManager Instance
        {
            get { return BootstrappingManager.instance; }
        }

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Methods

        private void Awake()
        {
            // Singleton
            if(BootstrappingManager.instance == null)
            {
                BootstrappingManager.instance = this;
            }
            else if(BootstrappingManager.instance != this)
            {
                DestroyImmediate(this.gameObject);
                return;
            }

            // Keep alive
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            if(!this.bootstrappingFinished)        
            {
                // Update platform VR state
                Maroon.PlatformManager.Instance.UpdatePlatformVRStateBasedOnScene();

                // Redirects: Only enable if on bootstrapping scene, if standalone scene, don't redirect somewhere else
                if(Maroon.SceneManager.Instance.ActiveSceneNameWithoutPlatformExtension == "Bootstrapping")
                {
                    // Webgl redirect
                    if((Maroon.PlatformManager.Instance.CurrentPlatform == Maroon.Platform.WebGL) &&
                    (this.webGLEnableSceneLoadingViaURLParameter))
                    {
                        string parameter = Maroon.WebGLUrlParameterReader.GetUrlParameter(this.webGLSceneURLParameterName);
                        Maroon.SceneManager.Instance.LoadSceneIfInAnyCategory(
                            Maroon.SceneManager.Instance.GetSceneAssetBySceneName(parameter + ".pc"));
                    }

                    // First Scene Redirect
                    else
                    {
                        if(Maroon.PlatformManager.Instance.CurrentPlatformIsVR)
                        {
                            Maroon.SceneManager.Instance.LoadSceneIfInAnyCategory(this.firstVRScene);
                        }
                        else
                        {
                            Maroon.SceneManager.Instance.LoadSceneIfInAnyCategory(this.firstStandardScene);
                        }
                    }
                }

                // Bootstrapping finished
                this.bootstrappingFinished = true;
            }
        }
    }
}

