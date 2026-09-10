using UnityEngine;

public class DemoEndManager : MonoBehaviour
{
    public void VisitSteamPage()
    {
        Application.OpenURL($"https://store.steampowered.com/");
    }

    public void VisitItchIoPage()
    {
        Application.OpenURL($"https://itch.io/");
    }

    public void BackToMainMenu()
    {
        FindAnyObjectByType<SettingsUI>().BackToMainMenu();
    }
}
