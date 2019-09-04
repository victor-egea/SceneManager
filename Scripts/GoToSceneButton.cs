using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TGS.Lightning.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToSceneButton : MonoBehaviour
{
    [SerializeField] private string _SceneToLoad;
    [SerializeField] private bool _IsPopup;
    [SerializeField] private bool ClearHistory = false;

    public void OnButtonClicked()
    {
        if (_IsPopup)
        {
            TGSSceneManager.Instance.OpenSceneAdditive(_SceneToLoad);
        }
        else
        {
            TGSSceneManager.Instance.GoToScene(_SceneToLoad, ClearHistory);
        }
    }
}