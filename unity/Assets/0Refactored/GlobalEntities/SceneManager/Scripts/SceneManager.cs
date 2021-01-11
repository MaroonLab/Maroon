﻿using UnityEngine;
using System.Collections.Generic;

namespace Maroon
{
    /// <summary>
    ///     Handles tasks related to scenes in Maroon, including finding scene categories, scene assets and loading
    ///     scenes.
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Fields

        private static SceneManager instance = null;

        /// TODO
        [SerializeField] private Maroon.CustomSceneAsset sceneMainMenuPC = null;

        [SerializeField] private Maroon.CustomSceneAsset sceneMainMenuVR = null;

        [SerializeField] private Maroon.SceneCategory[] sceneCategories = null;

        private Maroon.SceneCategory activeSceneCategory;

        private Stack<Maroon.CustomSceneAsset> sceneHistory = new Stack<Maroon.CustomSceneAsset>();

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Properties, Getters and Setters

        // -------------------------------------------------------------------------------------------------------------
        // Singleton

        /// <summary>
        ///     The SceneManager instance
        /// </summary>
        public static SceneManager Instance
        {
            get
            {
                return SceneManager.instance;
            }
        }

        // -------------------------------------------------------------------------------------------------------------
        // Categories

        /// <summary>
        ///     The SceneCategory that is currently active.
        //      For example if the player is in the physics lab/category, the physics category should be set to active.
        /// </summary>
        public Maroon.SceneCategory ActiveSceneCategory
        {
            get
            {
                return this.activeSceneCategory;
            }

            set
            {
                // Only set if it exists in categories array
                if(System.Array.Exists(this.sceneCategories, element => element == value))
                {
                    this.activeSceneCategory = value;
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------------
        // Active Scene

        /// <summary>
        ///     The name of the scene that is currently active.
        /// </summary>
        public string ActiveSceneName
        {
            get
            {
                return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }
        }

        /// <summary>
        ///     The name of the scene that is currently active, without the platform-specific extension (.vr or .pc).
        /// </summary>
        public string ActiveSceneNameWithoutPlatformExtension
        {
            get
            {
                return this.ActiveSceneName.Substring(0, this.ActiveSceneName.LastIndexOf('.'));
            }
        }

        /// <summary>
        ///     Maroon.PlatformManager has the properties CurrentPlatformIsVR and SceneTypeBasedOnPlatform. You might
        ///     want to use that to get info about the current build platform and VR state. This is only valid for the
        ///     active scene. This method returns true if the currently active scene is a virtual reality scene and has
        ///     the .vr extension. Returns false otherwise.
        /// </summary>
        public bool activeSceneIsVR()
        {
            if(this.ActiveSceneName.Contains(".vr"))
            {
                return true;
            }
            return false;
        }

        // -------------------------------------------------------------------------------------------------------------
        // Categories

        /// <summary>
        ///     Returns a list of all scene categories registered in the SceneManager prefab and returns the category
        ///     if the name matches. Returns empty list if no category was found.
        /// </summary>
        /// <param name="sceneType">
        ///     Mixed allows any scene category type, others limit returned categories to the given type.
        /// </param>
        /// <param name="includeHidden">
        ///     Hidden categories are excluded by default. Set to true to include hidden categories.
        /// </param>
        public List<Maroon.SceneCategory> getSceneCategories(Maroon.SceneType sceneType = Maroon.SceneType.Mixed,
                                                             bool includeHidden = false)
        {
            // Create list
            List<Maroon.SceneCategory> categories = new List<Maroon.SceneCategory>();

            // Add categories
            for(int iCategories = 0; iCategories < this.sceneCategories.Length; iCategories++)
            {
                // Extract current category
                Maroon.SceneCategory current_category = this.sceneCategories[iCategories];

                // Skip hidden categories based on setting
                if((!includeHidden) && (current_category.HiddenCategory))
                {
                    continue;
                }

                // Select if all types allowed or if desired type eqal to current type
                if((sceneType == Maroon.SceneType.Mixed) || (sceneType == current_category.SceneTypeInThisCategory))
                {
                    categories.Add(current_category);
                }
            }

            // Return categories
            return categories;
        }

        /// <summary>
        ///     Searches all scene categories registered in the SceneManager prefab and returns the category if the
        ///     name matches. Returns null if no category was found.
        /// </summary>
        /// <param name="categoryName">
        ///     The name of the category to be looked for.
        /// </param>
        /// <param name="sceneType">
        ///     Mixed allows any scene category type, others limit returned categories to the given type.
        /// </param>
        public Maroon.SceneCategory getSceneCategoryByName(string categoryName, Maroon.SceneType sceneType =
                                                           Maroon.SceneType.Mixed)
        {
            // Check if category exists
            Maroon.SceneCategory categoryFound = null;
            for(int iCategories = 0; iCategories < this.sceneCategories.Length; iCategories++)
            {
                if(categoryName == this.sceneCategories[iCategories].Name)
                {
                    // Extract current category
                    Maroon.SceneCategory current_category = this.sceneCategories[iCategories];

                    // Select if all types allowed or if desired type eqal to current type
                    if((sceneType == Maroon.SceneType.Mixed) || (sceneType == current_category.SceneTypeInThisCategory))
                    {
                        categoryFound = current_category;
                        break;
                    }
                }
            }

            // Return category or null
            return categoryFound;
        }

        // -------------------------------------------------------------------------------------------------------------
        // Scenes

        /// <summary>
        ///     Aggregates all scenes registered to any category in the SceneManager prefab and returns them as an
        ///     Maroon.CustomSceneAsset list.
        /// </summary>
        /// <param name="sceneType">
        ///     Mixed allows any scene type, others limit returned scenes to the given type.
        /// </param>
        private List<Maroon.CustomSceneAsset> getScenesFromAllCategories(Maroon.SceneType sceneType =
                                                                         Maroon.SceneType.Mixed)
        {
            // Create list
            List<Maroon.CustomSceneAsset> scenesFromAllCategories = new List<Maroon.CustomSceneAsset>();

            // Aggregate all scenes from all categories
            for(int iCategories = 0; iCategories < this.sceneCategories.Length; iCategories++)
            {
                for(int iScenes = 0; iScenes < this.sceneCategories[iCategories].Scenes.Length; iScenes++)
                {
                    // Extract current scene
                    Maroon.CustomSceneAsset current_scene = this.sceneCategories[iCategories].Scenes[iScenes];

                    // Add if all types allowed or if desired type eqal to current type
                    if((sceneType == Maroon.SceneType.Mixed) || (sceneType == current_scene.SceneType))
                    {
                        scenesFromAllCategories.Add(current_scene);
                    }
                }
            }
        
            // Return result
            return scenesFromAllCategories;
        }

        /// <summary>
        ///     Aggregates all scenes registered to any category in the SceneManager prefab and returns their names as
        ///     a string array.
        /// </summary>
        /// <param name="sceneType">
        ///     Mixed allows any scene type, others limit returned scenes to the given type.
        /// </param>
        public string[] getSceneNamesFromAllCategories(Maroon.SceneType sceneType = Maroon.SceneType.Mixed)
        {
            // Get Maroon.CustomSceneAsset list containing all registered scenes
            List<Maroon.CustomSceneAsset> scenesFromAllCategories = this.getScenesFromAllCategories(sceneType);

            // Create array
            string[] sceneNamesFromAllCategories = new string[scenesFromAllCategories.Count];

            // Get scene names
            for(int iScenes = 0; iScenes < scenesFromAllCategories.Count; iScenes++)
            {
                sceneNamesFromAllCategories[iScenes] = scenesFromAllCategories[iScenes].SceneName;
            }

            // Return result
            return sceneNamesFromAllCategories;
        }

        /// <summary>
        ///     Aggregates all scenes registered to any category in the SceneManager prefab and returns their names as
        ///     a string array. The platform-specific extension .vr or .pc is removed from the name.
        /// </summary>
        /// <param name="sceneType">
        ///     Mixed allows any scene type, others limit returned scenes to the given type.
        /// </param>
        public string[] getSceneNamesWithoutPlatformExtensionFromAllCategories(Maroon.SceneType sceneType =
                                                                               Maroon.SceneType.Mixed)
        {
            // Get Maroon.CustomSceneAsset list containing all registered scenes
            List<Maroon.CustomSceneAsset> scenesFromAllCategories = this.getScenesFromAllCategories(sceneType);

            // Create array
            string[] sceneNamesFromAllCategories = new string[scenesFromAllCategories.Count];

            // Get scene names without platform-specific extensions
            for(int iScenes = 0; iScenes < scenesFromAllCategories.Count; iScenes++)
            {
                sceneNamesFromAllCategories[iScenes] = scenesFromAllCategories[iScenes].SceneNameWithoutPlatformExtension;
            }

            // Return result
            return sceneNamesFromAllCategories;
        }

        // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // Methods

        // -------------------------------------------------------------------------------------------------------------
        // Initialization

        /// <summary>
        ///     Called by Unity. Initializes singleton instance and DontDestroyOnLoad (stays active on new scene load).
        ///     Checks if only VR scenes are in VR categories and only standard scenes are in standard categories.
        ///     Throws an exception if this is not the case. 
        /// </summary>
        private void Awake()
        {
            // Singleton
            if(SceneManager.instance == null)
            {
                SceneManager.instance = this;
            }
            else if(SceneManager.instance != this)
            {
                DestroyImmediate(this.gameObject);
                return;
            }

            // Keep alive
            DontDestroyOnLoad(this.gameObject);

            // Check if only VR scenes are in VR categories and only standard scenes are in standard categories
            for(int iCategories = 0; iCategories < this.sceneCategories.Length; iCategories++)
            {
                // Extract current category
                Maroon.SceneCategory current_category = this.sceneCategories[iCategories];

                // Check if all scenes types match
                for(int iScenes = 0; iScenes < current_category.Scenes.Length; iScenes++)
                {
                    if(current_category.SceneTypeInThisCategory != current_category.Scenes[iScenes].SceneType)
                    {
                        throw new System.Exception("Category >" + current_category.Name +
                                                   "< contains a scene with mismatched type.");
                    }                    
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------------
        // Scenes

        /// <summary>
        ///     Returns a Maroon.CustomSceneAsset based on a scene name or a scene path. The scene must be registered
        ///     in one of the categories in the SceneManager to be able to convert it with this method. Returns null
        ///     if no Maroon.CustomSceneAsset is found
        /// </summary>
        /// <param name="scene">
        ///     The full path to the scene or the name of the scene.
        /// </param>
        public Maroon.CustomSceneAsset GetSceneAssetBySceneName(string sceneNameOrPath)
        {
            // Convert full path to scene name
            string sceneName = System.IO.Path.GetFileName(sceneNameOrPath);

            // Find and return Maroon.CustomSceneAsset
            Maroon.CustomSceneAsset sceneAsset;
            sceneAsset = this.getScenesFromAllCategories().Find(element => sceneName == element.SceneName);
            return sceneAsset;
        }

        // -------------------------------------------------------------------------------------------------------------
        // Scene Navigation

        /// <summary>
        ///     Loades a scene based on a Maroon.CustomSceneAsset. The scene must be registered in one of the
        ///     categories in the SceneManager to be able to load it with this method. Always use this method for 
        ///     loading a new scene, it updates the scene history and checks if the platform is correct.
        ///
        ///     TODO: Pipe all scene changes through this method so that this method can notify the NetworkManager
        ///     consistently.
        /// </summary>
        /// <param name="scene">
        ///     A Maroon.CustomSceneAsset to be loaded.
        /// </param>
        /// <param name="showLoadingScreen">
        ///     Enables a loading screen while loading the scene. TODO: Not implemented. Implement this.
        /// </param>
        public bool LoadSceneIfInAnyCategory(Maroon.CustomSceneAsset scene, bool showLoadingScreen = false)
        {
            // If scene to be loaded doesn't exist in one of the categories
            if(!System.Array.Exists(this.getScenesFromAllCategories().ToArray(), element => element.SceneName == scene.SceneName))
            {
                // TODO: Convert to warning after fixing warnings
                Debug.Log("WARNING: Tried to load scene that does not exist in categories."); 
                return false;
            }

            // If scene to be loaded has wrong platform
            if(scene.IsVirtualRealityScene != Maroon.PlatformManager.Instance.CurrentPlatformIsVR)
            {
                // TODO: Convert to warning after fixing warnings
                Debug.Log("WARNING: Tried to load scene that does not match platform type."); 
                return false;
            }

            // If valid, load scene and return true
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
            this.sceneHistory.Push(scene);
            return true;
        }

        /// <summary>
        ///     Loades the Main Menu scene according to the current platform.
        /// </summary>
        /// <param name="showLoadingScreen">
        ///     Enables a loading screen while loading the scene.
        /// </param>
        public void LoadMainMenu(bool showLoadingScreen = false)
        {
            // Return VR main menu for VR platform
            if(Maroon.PlatformManager.Instance.CurrentPlatformIsVR)
            {
                this.LoadSceneIfInAnyCategory(this.sceneMainMenuVR);
            }

            // Return PC main menu
            this.LoadSceneIfInAnyCategory(this.sceneMainMenuPC);
        }

        /// <summary>
        ///     The name of the scene that was loaded before the currently active scene. If no previous scene
        ///     is available, the Main Menu scene according to the current platform.
        /// </summary>
        public void LoadPreviousScene()
        {
            // If there is no previous scene available, load main menu
            if(this.sceneHistory.Count < 2)
            {
                this.LoadMainMenu();
            }

            // If previous scene available, remove current scene and load previous scene
            this.sceneHistory.Pop();
            this.LoadSceneIfInAnyCategory(this.sceneHistory.Pop());            
        }
    }

    /// <summary>
    ///     Describes types of scenes used in Maroon. Standard is used for PC, Mac and WebGL. VR is used for Virtual
    ///     Reality scenes. Mixed is used for scene categories or functions that work with both types.
    /// </summary>
    public enum SceneType
    {
        Standard,
        VR,
        Mixed
    }
}
