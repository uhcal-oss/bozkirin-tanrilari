using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class WordPuzzleMiniGame : MonoBehaviour, IMiniGame
{
    [System.Serializable]
    public class PuzzleWord
    {
        public string word;
        [TextArea]
        public string hint;
        public TMP_InputField inputField; // The UI Input to type the guess
        public TextMeshProUGUI hintText;  // The UI Text to display the hint
        public bool isSolved;
    }

    [Header("Puzzle Settings")]
    public List<PuzzleWord> puzzleWords = new List<PuzzleWord>();

    [Header("UI References")]
    public GameObject winScreen; // Optional: A little popup before closing the game

    public void StartGame()
    {
        Debug.Log("[WordPuzzle] MiniGame Started!");
        if (winScreen != null) winScreen.SetActive(false);

        // Initialize UI
        foreach (var p in puzzleWords)
        {
            p.isSolved = false;
            
            if (p.hintText != null)
                p.hintText.text = p.hint;

            if (p.inputField != null)
            {
                p.inputField.text = ""; // clear old inputs
                p.inputField.interactable = true;
                
                // Unsubscribe first to avoid memory leaks
                p.inputField.onValueChanged.RemoveAllListeners(); 
                
                // Check if they typed the right word every time they type a letter
                p.inputField.onValueChanged.AddListener(delegate { CheckWords(); });
            }
        }
    }

    public void CheckWords()
    {
        bool allSolved = true;

        foreach (var p in puzzleWords)
        {
            // Case-insensitive check, trimming whitespaces
            if (p.inputField != null && p.inputField.text.Trim().ToLower() == p.word.ToLower())
            {
                p.isSolved = true;
                p.inputField.interactable = false; // Lock the box once they get it right
                p.inputField.text = p.word.ToUpper(); // Make it look nice
            }
            else
            {
                p.isSolved = false;
            }

            if (!p.isSolved)
            {
                allSolved = false;
            }
        }

        if (allSolved)
        {
            CompleteGame();
        }
    }

    void CompleteGame()
    {
        Debug.Log("[WordPuzzle] All words guessed! Minigame Win!");
        
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            Invoke("EndMiniGame", 2f); // wait 2 seconds, then tell Launcher we won
        }
        else
        {
            EndMiniGame();
        }
    }

    void EndMiniGame()
    {
        // Tell the central Minigame Launcher that we succeeded!
        if (MiniGameLauncher.Instance != null)
        {
            MiniGameLauncher.Instance.WinGame();
        }
    }
}