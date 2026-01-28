using UnityEngine;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private GameObject aboutPanel;
    [SerializeField] private GameObject menuRoot;

    public void ShowAbout()
    {
        if (menuRoot != null) menuRoot.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(true);
    }

    public void HideAbout()
    {
        if (aboutPanel != null) aboutPanel.SetActive(false);
        if (menuRoot != null) menuRoot.SetActive(true);
    }
}
