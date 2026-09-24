using System;
using System.Linq;
using KeySlaught.SceneGameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ReviewIssues2Scene
{
    public static string FocusGameView()
    {
        var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        var window = EditorWindow.GetWindow(gameViewType);
        window.Focus();
        return "Focused the Unity Game view for a composited UI capture.";
    }

    public static string Audit()
    {
        var canvas = GameObject.Find("/Portrait Gameplay HUD") ?? throw new InvalidOperationException("Portrait HUD missing.");
        var top = canvas.transform.Find("Top 20 Percent")?.GetComponent<RectTransform>() ?? throw new InvalidOperationException("Top 20% region missing.");
        var bottom = canvas.transform.Find("Bottom 20 Percent")?.GetComponent<RectTransform>() ?? throw new InvalidOperationException("Bottom 20% region missing.");
        var context = top.Find("Transparent Context Actions")?.GetComponent<Image>() ?? throw new InvalidOperationException("Transparent context region missing.");
        var target = bottom.GetComponentsInChildren<Text>(true).FirstOrDefault(label => label.fontSize == 33) ?? throw new InvalidOperationException("Bottom enemy-word label missing.");
        var keys = bottom.GetComponentsInChildren<OnScreenLetterButton>(true);
        var activeKeys = keys.Count(key => key.gameObject.activeInHierarchy);
        var prefabbedKeys = keys.Count(key => PrefabUtility.GetCorrespondingObjectFromSource(key.gameObject) != null);
        var cinemachine = GameObject.Find("/KeySlaught Gameplay/Cinemachine Player Camera");
        var zoom = cinemachine?.GetComponent<CameraMotionZoom>();
        var wand = bottom.Find("Wand Icon")?.GetComponent<Image>();
        var reload = bottom.Find("Reload/Reload Icon")?.GetComponent<Image>();
        var firstKeyRect = keys[0].GetComponent<RectTransform>();
        var firstKeyImage = keys[0].GetComponent<Image>();

        if (!Mathf.Approximately(top.anchorMin.y, 0.8f) || !Mathf.Approximately(bottom.anchorMax.y, 0.2f)) throw new InvalidOperationException("HUD ratios are not 20/60/20.");
        if (context.color.a > 0.01f) throw new InvalidOperationException("Context background is not transparent.");
        if (keys.Length != 26 || activeKeys != 26) throw new InvalidOperationException($"Expected 26 active keys, found {keys.Length}/{activeKeys}.");
        if (!EditorApplication.isPlaying && prefabbedKeys != 26) throw new InvalidOperationException($"Expected 26 prefabbed keys, found {prefabbedKeys}.");
        if (cinemachine == null || cinemachine.GetComponent("CinemachineCamera") == null) throw new InvalidOperationException("Authored Cinemachine camera missing.");
        var zoomReference = zoom == null ? null : new SerializedObject(zoom).FindProperty("cinemachineCamera")?.objectReferenceValue;
        if (zoomReference == null) throw new InvalidOperationException("CameraMotionZoom is not wired to Cinemachine.");
        if (wand?.sprite?.name != "UI_Wand_128" || reload?.sprite?.name != "UI_Reload_128") throw new InvalidOperationException("Wand or reload icon is not wired.");

        return $"HUD anchors top={top.anchorMin.y:0.0}, bottom={bottom.anchorMax.y:0.0}; context alpha={context.color.a:0.000}; enemy label parent={target.transform.parent.name}; keys active/prefabbed={activeKeys}/{prefabbedKeys}; first key pos={firstKeyRect.anchoredPosition}, size={firstKeyRect.sizeDelta}, alpha={firstKeyImage.color.a:0.0}, culled={firstKeyImage.canvasRenderer.cull}; Cinemachine camera/zoom=present/wired; icons={wand.sprite.name}/{reload.sprite.name}.";
    }
}
