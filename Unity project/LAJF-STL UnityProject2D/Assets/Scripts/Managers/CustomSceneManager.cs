﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using STL2.Events;

//Responsible for sceneloading, both additive and non-additive.
//"ALoad" refers to async loading something in. Remember to only load things in async once.
public class CustomSceneManager : MonoBehaviour
{
    #region INSPECTOR
    [SerializeField]
    private int currentEnvironmentIndex = -1;
    private int currentChoiceSceneIndex = -1;
    [SerializeField]
    public BoolVariable isLoadingScene;
    //private bool isLoadingScene = false;
    private float loadProgress = 0;

    private int environmentBuildIndexToLoad, environmentPreviousBuildIndex, choiceSceneBuildIndexToLoad, choiceScenePreviousBuildIndex;

    public IntEvent loadProgressInt;

    [SerializeField]
    public List<int> currentlyLoadedSceneIndices;

    public TransitionScreen transitionScreen;
    public IntTypeListener transitionScreenIntListener;

    bool firstFade = true;

    #endregion INSPECTOR

    public void Start()
    {
      //  GameObject.DontDestroyOnLoad(this);
        if (isLoadingScene != null)
        {
            isLoadingScene.setBool(false);
        }
    }

    public void LoadMainMenu()
    {
        if (SceneManager.GetActiveScene().name != "Credits")
        {
            try
            {
                Destroy(FindObjectOfType<MusicManager>().gameObject);
            }
            catch
            {
                Debug.Log("there is no music manager");
            }
            Time.timeScale = 1;
        }
        
        SceneManager.LoadScene(0); //Index of main-menu. 
    }

    public void LoadCredits()
    {
        //Should probably not just load this scene, but this is a start.
        //SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
        SceneManager.LoadScene(8);
    }

    public void RequestSceneChange(int _sceneIndex)
    {
        //In case there is logic needed to prevent scenechanges in certain situations:
        if (_sceneIndex > SceneManager.sceneCountInBuildSettings - 1 || _sceneIndex < 0)
        {
            Debug.Log("Scene request denied, index out of range");
            return;
        }
        //Probably need some conditional to figure out how many environments there are, and not just load into those,
        //but rather ensure that the sceneIndex corresponds to a proper scene (mainmenu, gameloop, credits/end).
        SceneManager.LoadScene(_sceneIndex);
    }

    public void UpdateLoadProgress(float _loadProgress)
    {

      //  loadProgressInt.Raise((int)(_loadProgress * 100));
    }

    #region AditiveEnvironments

    public void RequestEnvironmentChange(int environmentIndex)
    {
        //TODO: Check whether or not we can change environment:
        if (isLoadingScene.myBool)
        {
            return;
        }

        if (environmentIndex == -1)
        {
            isLoadingScene.setBool(true);
            StartCoroutine(ALoadEnvironment(environmentIndex));
            return;
        }

        isLoadingScene.setBool(true);
        StartCoroutine(ChangeEnvironment(currentEnvironmentIndex, environmentIndex));
    }

    //ChangeEnvironment combines two other coroutines, to ensure that before loading the next environment, the previous one is always unloaded first.
    IEnumerator ChangeEnvironment(int currentEnvironment, int newEnvironment)
    {
        if (currentEnvironment < 1)
        {
            StartCoroutine(ALoadEnvironment(newEnvironment));
            yield break;
        }
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentEnvironment, UnloadSceneOptions.None);
        asyncUnload.completed += (AsyncOperation) =>
        {
            StartCoroutine(ALoadEnvironment(newEnvironment));
        };

        while (!asyncUnload.isDone)
        {
            Debug.Log("Unloading progress: " + asyncUnload.progress);
            yield return null;
        }

        yield return null;
    }

    //Loads in new environments
    public IEnumerator ALoadEnvironment(int sceneIndex)
    {
        //Unload current environment (if one is present)
        //Load in new one
        //Remove loading-screen (transition)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

        asyncLoad.completed += (AsyncOperation) =>
        {
            currentEnvironmentIndex = sceneIndex;
            isLoadingScene.setBool(false);
            loadProgress = 0;
            UpdateLoadProgress(1);
        };


        while (!asyncLoad.isDone)
        {
            UpdateLoadProgress(asyncLoad.progress);
            loadProgress = asyncLoad.progress;
            yield return null;
        }
    }

    public IEnumerator AUnloadEnvironment(int sceneIndex)
    {
        if (sceneIndex == -1)
        {
            yield break;
        }

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneIndex, UnloadSceneOptions.None);
        asyncUnload.completed += (AsyncOperation) =>
        {
            currentEnvironmentIndex = -1;
        };

        while (!asyncUnload.isDone)
        {
            Debug.Log("Unloading progress: " + asyncUnload.progress);
            loadProgress = asyncUnload.progress;
            yield return null;
        }
    }

    public void LoadChosenEnvironmentScene()
    {
        if (SceneManager.sceneCount > 2)
        {
            StartCoroutine(AUnloadEnvironment(environmentPreviousBuildIndex));
        }
        if (environmentBuildIndexToLoad != 4)
        {
            StartCoroutine(ALoadEnvironment(environmentBuildIndexToLoad));
            environmentPreviousBuildIndex = environmentBuildIndexToLoad;
        }
    }

    #endregion AditiveEnvironments

    #region AdditiveChoiceScenes
    public void RequestChoiceSceneChange(int choiceSceneIndex)
    {
        if (choiceSceneIndex == -1)
        {
            isLoadingScene.setBool(true);
            StartCoroutine(ALoadChoiceScene(choiceSceneIndex));
            return;
        }
        isLoadingScene.setBool(true);
        StartCoroutine(ChangeChoiceScene(currentChoiceSceneIndex, choiceSceneIndex));
    }

    IEnumerator ChangeChoiceScene(int currentChoiceScene, int newChoiceScene)
    {
        if (currentChoiceScene < 1)
        {
            StartCoroutine(ALoadChoiceScene(newChoiceScene));
            yield break;
        }
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentChoiceScene, UnloadSceneOptions.None);
        asyncUnload.completed += (AsyncOperation) =>
        {
            StartCoroutine(ALoadChoiceScene(newChoiceScene));
        };

        while (!asyncUnload.isDone)
        {
            Debug.Log("Unloading progress: " + asyncUnload.progress);
            yield return null;
        }

        yield return null;
    }

    IEnumerator ALoadChoiceScene(int sceneIndex)
    {
        //Unload current choice scene (if one is present)
        //Load in new one
        //Remove loading-screen (transition)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

        asyncLoad.completed += (AsyncOperation) =>
        {
            currentChoiceSceneIndex = sceneIndex;
            isLoadingScene.setBool(false);
            loadProgress = 0;
            UpdateLoadProgress(1);
            // Debug.Log("Checking how many times this is called");
        };


        while (!asyncLoad.isDone)
        {
            UpdateLoadProgress(asyncLoad.progress);
            //   Debug.Log("Loading progress: " + asyncLoad.progress);
            loadProgress = asyncLoad.progress;
            yield return null;
        }
    }

    IEnumerator AUnLoadChoiceScene(int sceneIndex)
    {
        if (sceneIndex == -1)
        {
            yield break;
        }

        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneIndex, UnloadSceneOptions.None);
        asyncUnload.completed += (AsyncOperation) =>
        {
            currentChoiceSceneIndex = -1;
        };

        while (!asyncUnload.isDone)
        {
            Debug.Log("Unloading progress: " + asyncUnload.progress);
            loadProgress = asyncUnload.progress;
            yield return null;
        }
    }

    #endregion AdditiveChoiceScenes

    #region SceneTransitions

    public void ChooseEnvironmentSceneToLoad(int buildIndex)
    {
        environmentBuildIndexToLoad = buildIndex;
    }

    public void ChooseChoiceSceneToLoad(int buildIndex)
    {
        choiceSceneBuildIndexToLoad = buildIndex;
    }


    public void LoadChosenChoiceScene()
    {
        if (SceneManager.sceneCount == 2)
        {
            // transitionScreen.StopAllCoroutines();
            transitionScreen.enabled = false;
            transitionScreenIntListener.enabled = false;
        }
        else if (SceneManager.sceneCount > 2)
        {
            StartCoroutine(AUnLoadChoiceScene(choiceScenePreviousBuildIndex));
        }
        if(choiceSceneBuildIndexToLoad != 4)
        {
            StartCoroutine(ALoadChoiceScene(choiceSceneBuildIndexToLoad));
            choiceScenePreviousBuildIndex = choiceSceneBuildIndexToLoad;
        }
        else
        {
            transitionScreen.enabled = true;
            transitionScreenIntListener.enabled = true;
        }
    }

    public void OnLoadedAdditiveScene()
    {
        transitionScreen.GetComponent<CanvasGroup>().alpha = 0;
    }
    #endregion






}
