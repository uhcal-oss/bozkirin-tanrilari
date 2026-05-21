#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PuzzleSetupHelper : MonoBehaviour
{
    [MenuItem("Tools/Create Word Puzzle UI")]
    public static void CreatePuzzleUI()
    {
        // 1. Find or Create Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MiniGame Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. Create Background Panel
        GameObject panelObj = new GameObject("Bulmaca Panel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Dark blue-ish gray

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelRect, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Çengel Bulmaca";
        titleText.fontSize = 48;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Container for items
        GameObject containerObj = new GameObject("Items Container");
        containerObj.transform.SetParent(panelRect, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.05f, 0.05f);
        containerRect.anchorMax = new Vector2(0.95f, 0.85f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 15;

        // Data
        string[] words = { "Dağ", "Zafer", "Umut", "Cesaret", "Gökbörü" };
        string[] hints = { 
            "Ne kadar büyük olsa da bir gün aşılır.", 
            "Savaşın ya da mücadelenin kazanılması.", 
            "İnsanı hayatta tutan iç güç.", 
            "Korkuya rağmen devam etme gücü.", 
            "Yolu kaybolanlara rehberlik eden kutsal kurt." 
        };

        WordPuzzleMiniGame puzzleGame = panelObj.AddComponent<WordPuzzleMiniGame>();
        puzzleGame.puzzleWords = new List<WordPuzzleMiniGame.PuzzleWord>();

        for (int i = 0; i < words.Length; i++)
        {
            GameObject rowObj = new GameObject($"Row_{words[i]}");
            rowObj.transform.SetParent(containerRect, false);
            HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.spacing = 10;
            LayoutElement rowLe = rowObj.AddComponent<LayoutElement>();
            rowLe.minHeight = 80;

            // Hint Text
            GameObject hintObj = new GameObject("Hint Text");
            hintObj.transform.SetParent(rowObj.transform, false);
            TextMeshProUGUI hintTextComp = hintObj.AddComponent<TextMeshProUGUI>();
            hintTextComp.text = hints[i];
            hintTextComp.fontSize = 24;
            hintTextComp.color = Color.white;
            hintTextComp.alignment = TextAlignmentOptions.MidlineLeft;
            hintTextComp.enableWordWrapping = true;
            LayoutElement hintLe = hintObj.AddComponent<LayoutElement>();
            hintLe.flexibleWidth = 2f; // Takes twice the width of input

            // Input Field Background
            GameObject inputBgObj = new GameObject("Input Field");
            inputBgObj.transform.SetParent(rowObj.transform, false);
            Image inputBgImage = inputBgObj.AddComponent<Image>();
            inputBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            LayoutElement inputLe = inputBgObj.AddComponent<LayoutElement>();
            inputLe.flexibleWidth = 1f;

            // Input Text Area
            GameObject textAreaObj = new GameObject("Text Area");
            textAreaObj.transform.SetParent(inputBgObj.transform, false);
            RectTransform textAreaRect = textAreaObj.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 5);
            textAreaRect.offsetMax = new Vector2(-10, -5);
            RectMask2D mask = textAreaObj.AddComponent<RectMask2D>();

            // Input Text
            GameObject inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(textAreaObj.transform, false);
            RectTransform inputTextRect = inputTextObj.AddComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI inputTextComp = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputTextComp.fontSize = 32;
            inputTextComp.color = Color.white;
            inputTextComp.alignment = TextAlignmentOptions.MidlineLeft;

            TMP_InputField inputField = inputBgObj.AddComponent<TMP_InputField>();
            inputField.textComponent = inputTextComp;
            inputField.textViewport = textAreaRect;

            WordPuzzleMiniGame.PuzzleWord pWord = new WordPuzzleMiniGame.PuzzleWord();
            pWord.word = words[i];
            pWord.hint = hints[i];
            pWord.hintText = hintTextComp;
            pWord.inputField = inputField;
            puzzleGame.puzzleWords.Add(pWord);
        }

        Selection.activeGameObject = panelObj;
        Debug.Log("Created Word Puzzle UI Successfully!");
    }
}
#endif