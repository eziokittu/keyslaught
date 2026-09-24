using System;
using System.Linq;
using KeySlaught.Progression;
using KeySlaught.SceneGameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class ReviewMilestone10Runtime
{
    public static string InspectLaunch()
    {
        var button = GameObject.Find("/Game Shell Canvas/Game Shell/Launch Screen/Launch Actions/Button CONTINUE")?.GetComponent<Button>()
            ?? throw new InvalidOperationException("Continue button missing.");
        return $"image={button.image.color}; normal={button.colors.normalColor}; text={button.GetComponentInChildren<Text>().color}";
    }

    public static string ShowMain()
    {
        var shell=Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);shell.ContinueFromLaunch();return "Main menu shown.";
    }

    public static string StartTutorial()
    {
        var shell=Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);shell.StartTutorial();return "Tutorial started.";
    }

    public static string ShowPause()
    {
        var tutorial=Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        var guide=GameObject.Find("/Portrait Gameplay HUD/Tutorial Guidance");if(guide!=null)guide.SetActive(false);
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);pause.TogglePause();return "Pause shown.";
    }

    public static string ShowGameplay()
    {
        var guide=GameObject.Find("/Portrait Gameplay HUD/Tutorial Guidance");if(guide!=null)guide.SetActive(false);
        var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);run.SetGuidanceSuspended(false);
        Time.timeScale=1f;return "Gameplay shown.";
    }

    public static string ShowControls()
    {
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);if(!pause.IsPaused)pause.TogglePause();pause.ToggleControls();return "Controls shown.";
    }

    public static string ShowMenuConfirmation()
    {
        var pause=Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);if(!pause.IsPaused)pause.TogglePause();pause.HideControls();pause.ShowMenuConfirmation();return "Menu confirmation shown.";
    }

    public static string StartAutomatedTutorialDemo()
    {
        var shell=Object.FindFirstObjectByType<GameShellController>(FindObjectsInactive.Include);shell.StartTutorial();
        var tutorial=Object.FindFirstObjectByType<TutorialDirector>(FindObjectsInactive.Include);
        tutorial.ContinueTutorial();tutorial.ContinueTutorial();tutorial.ContinueTutorial();
        return "Automated Library-damage tutorial demo started.";
    }

    public static string InspectTutorialDemo()
    {
        var guide=GameObject.Find("/Portrait Gameplay HUD/Tutorial Guidance");
        var title=guide?.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.gameObject.name.StartsWith("Label "));
        var library=Object.FindFirstObjectByType<LibraryEndpoint>(FindObjectsInactive.Include);
        var run=Object.FindFirstObjectByType<WaveRunController>(FindObjectsInactive.Include);
        return $"guideActive={guide?.activeSelf}; title={title?.text}; library={library?.State?.CurrentHealth}/{library?.State?.MaximumHealth}; phase={run?.Phase}; timeScale={Time.timeScale}";
    }
}
