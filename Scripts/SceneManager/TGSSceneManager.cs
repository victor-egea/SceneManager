using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TGS.Lightning.UI
{
    public class TGSSceneManager : MonoBehaviour
    {
        public enum SCENE_MANAGER_STATE
        {
            IDLE = 0,
            LOADING_SCENE = 1,
            INITIALIZING_SCENE = 2,
            UNLOADING_SCENE = 3
        }

        public enum LOADING_TYPE
        {
            NONE = 0,
            LOADING_SCREEN = 1
        }

        private Stack<Scene> _SceneStack;
        private Stack<WindowConfig> _WindowHistory;
        private IWindowController _ActiveWindowController = null;
        private WindowConfig _transitionWindowConfig = null;

        private static TGSSceneManager _instance;

        public static TGSSceneManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject parentGameObject = new GameObject("SceneManager");
                    _instance = parentGameObject.AddComponent<TGSSceneManager>();
                }

                return _instance;
            }
        }

        public SCENE_MANAGER_STATE _SceneManagerState = SCENE_MANAGER_STATE.IDLE;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            _WindowHistory = new Stack<WindowConfig>();
            _SceneStack = new Stack<Scene>();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name == "TopBar")
            {
                return;
            }

            SceneManager.SetActiveScene(scene);

            IWindowController windowController = GetWindowControllerFromScene(scene);
            if (windowController != null)
            {
                _ActiveWindowController = windowController;
                _WindowHistory.Push(_transitionWindowConfig);

                if (!_transitionWindowConfig._IsAdditive)
                {
                    _SceneStack.Push(scene);
                }
            }

            _SceneManagerState = SCENE_MANAGER_STATE.INITIALIZING_SCENE;
            
            // Initialize the Top Bar
            StartCoroutine(ShowTopBar(windowController));
            StartCoroutine(InitializeScene(windowController));
        }

        private IWindowController GetWindowControllerFromScene(Scene scene)
        {
            IWindowController windowController = null;
            var rootGameObjects = scene.GetRootGameObjects();
            if (rootGameObjects != null)
            {
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    windowController = rootGameObjects[i].GetComponent<IWindowController>();
                    if (windowController != null)
                    {
                        break;
                    }
                }
            }

            return windowController;
        }

        private IEnumerator InitializeScene(IWindowController windowController)
        {
            if (windowController != null)
            {
                // Initialize the data.
                yield return StartCoroutine(windowController.Initialize(_transitionWindowConfig));
                _transitionWindowConfig = null;

                // Play the animation.
                yield return StartCoroutine(windowController.TransitionIn());
            }

            _SceneManagerState = SCENE_MANAGER_STATE.IDLE;
        }

        #region Unload Scene

        public void CloseScene()
        {
            if (_SceneManagerState == SCENE_MANAGER_STATE.IDLE)
            {
                StartCoroutine(CloseInternal());
            }
        }

        public IEnumerator CloseInternal()
        {
            yield return StartCoroutine(UnloadScene());
            if (_WindowHistory.Count > 1)
            {
                WindowConfig currentWindowConfig = _WindowHistory.Pop();
                if (currentWindowConfig._IsAdditive)
                {
                    StartCoroutine(UnloadPopup(currentWindowConfig));
                }
                else
                {
                    WindowConfig windowConfig = _WindowHistory.Pop();
                    GoToScene(windowConfig._SceneName, false, windowConfig);
                }
            }
            else
            {
                GoToHome();
            }
        }

        private IEnumerator UnloadPopup(WindowConfig currentWindowConfig)
        {
            Scene sceneByName = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneByName);
            yield return asyncOperation;

            sceneByName = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(sceneByName);
            _ActiveWindowController = GetWindowControllerFromScene(sceneByName);
        }

        private IEnumerator UnloadScene()
        {
            if (_ActiveWindowController != null)
            {
                _SceneManagerState = SCENE_MANAGER_STATE.UNLOADING_SCENE;
                yield return StartCoroutine(_ActiveWindowController.Unload());
                yield return StartCoroutine(_ActiveWindowController.TransitionOut());
                _ActiveWindowController = null;
                _SceneManagerState = SCENE_MANAGER_STATE.IDLE;
            }
        }

        #endregion

        #region Go To Scene

        public void GoToHome()
        {
            // TODO(victor): We should have a better way of defining which scene is Home
            GoToScene("Test_1", false);
        }

        public void OpenSceneAdditive(string sceneName, WindowConfig windowWindowConfig = null)
        {
            if (_SceneManagerState == SCENE_MANAGER_STATE.IDLE && Application.CanStreamedLevelBeLoaded(sceneName))
            {
                if (windowWindowConfig == null)
                {
                    windowWindowConfig = new WindowConfig();
                }

                windowWindowConfig._IsAdditive = true;
                windowWindowConfig._SceneName = sceneName;
                _transitionWindowConfig = windowWindowConfig;

                StartCoroutine(GoToSceneCoroutine(sceneName, false, true, false));
            }
            else
            {
                Debug.LogError("Level cannot be loaded");
            }
        }

        public void GoToScene(string sceneName, bool clearHistory, WindowConfig windowWindowConfig = null)
        {
            if (_SceneManagerState == SCENE_MANAGER_STATE.IDLE && Application.CanStreamedLevelBeLoaded(sceneName))
            {
                if (windowWindowConfig == null)
                {
                    windowWindowConfig = new WindowConfig();
                }

                windowWindowConfig._IsAdditive = false;
                windowWindowConfig._SceneName = sceneName;
                _transitionWindowConfig = windowWindowConfig;

                StartCoroutine(GoToSceneCoroutine(sceneName, clearHistory, false, true));
            }
            else
            {
                Debug.LogError("Level cannot be loaded");
            }
        }

        private IEnumerator GoToSceneCoroutine(string sceneName, bool clearHistory, bool isAdditive,
            bool unloadPreviousScene)
        {
            Scene previousScene = default(Scene);
            if (unloadPreviousScene)
            {
                if (_SceneStack.Count > 0)
                {
                    previousScene = _SceneStack.Pop();
                }

                if (_ActiveWindowController != null)
                {
                    yield return StartCoroutine(UnloadScene());
                }
            }

            if (clearHistory)
            {
                _WindowHistory.Clear();
            }

            _SceneManagerState = SCENE_MANAGER_STATE.LOADING_SCENE;

            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncOperation.isDone)
            {
                yield return new WaitForEndOfFrame();
            }

            if (previousScene != default(Scene))
            {
                SceneManager.UnloadSceneAsync(previousScene);
            }
        }

        #endregion

        #region Top Bar

        private IWindowController topBarController = null;

        public IEnumerator ShowTopBar(IWindowController windowController)
        {
            if (windowController == null || windowController.TopBarConfig == null)
            {
                // Hide the Top Bar
                UnityEngine.Debug.LogError("Hide the Top Bar");
                if (topBarController != null)
                {
                    topBarController.CanvasGroup.alpha = 0;
                    topBarController.CanvasGroup.interactable = false;
                }
            }
            else
            {
                // Show and Update the top bar
                UnityEngine.Debug.LogError("Show the Top Bar");
                if (topBarController == null)
                {
                    LoadSceneParameters sceneParameters = new LoadSceneParameters(LoadSceneMode.Additive);
                    AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("TopBar", sceneParameters);
                    while (!asyncOperation.isDone)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    
                    Scene scene =  SceneManager.GetSceneByName("TopBar");

//                    Scene scene = SceneManager.LoadScene("TopBar", sceneParameters);
                    GameObject[] rootGos = scene.GetRootGameObjects();
                    foreach (var rootGameObject in rootGos)
                    {
                        var topBarWindowController = rootGameObject.GetComponent<IWindowController>();
                        if (topBarWindowController != null)
                        {
                            topBarController = topBarWindowController;
                            StartCoroutine(InitializeScene(topBarController));
                        }
                    }
                }

                if (topBarController != null)
                {
                    topBarController.CanvasGroup.alpha = 1.0f;
                    topBarController.CanvasGroup.interactable = true;
//                    topBarController.Initialize()
                }
            }
        }

        #endregion
    }
}