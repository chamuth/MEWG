using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Firebase.Database;
using Firebase.Auth;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

public class WordsBlocksContainer : MonoBehaviour
{
    public LetterBlockItem LetterBlock;
    public float LetterSpacing = 10f;
    public Transform Centerer;
    public Animator HintAnimator;
    public SelectionCircle selectionCircle;

    public string[] _Words { get; set; }

    List<RenderedWordBlock> Words = new List<RenderedWordBlock>();
    public List<ProcessedWordMatch> WordMatches = new List<ProcessedWordMatch>();
    public List<WordWordConnection> Connections = new List<WordWordConnection>();

    public CanvasGroup ZeroHintsRemainingNotification;
    public CanvasGroup AllHintsUsedNotification;

    public const int USABLE_HINT_LIMIT_PER_MATCH = 3;

    private int UsedHints = 0;
    private Dictionary<string, bool> PreviousAnimatedWords = null;

    public int GetUsedHints()
    {
        return UsedHints;
    }

    public IEnumerator RenderWords(string[] words, bool dev = false)
    {
        Words = new List<RenderedWordBlock>();
        Connections = new List<WordWordConnection>();

        // clear the parent
        foreach (Transform child in Centerer.transform)
            Destroy(child.gameObject);

        yield return new WaitForSeconds(0.05f);

        // First word goes brrr
        var fword = new RenderedWordBlock
        {
            Word = words[0],
            Letters = new List<RenderedLetterBlock>(),
            _Direction = Direction.Horizontal // First word is always horizontal
        };

        for (var i = 0; i < words[0].Length; i ++)
        {
            var lblock = Instantiate(LetterBlock, Vector3.zero, Quaternion.Euler(0, 0, 0), Centerer.transform);
            lblock.Letter = words[0][i].ToString();
            lblock.Visible = dev;
            lblock.UpdateValues();

            var lblockrect = lblock.gameObject.GetComponent<RectTransform>();

            var blockWidth = lblockrect.rect.width;

            var letterOffset = (words[0].Length % 2 == 0) ? (i + 0.5f - words[0].Length / 2) : i - (words[0].Length / 2);
            lblockrect.anchoredPosition = new Vector2(letterOffset * (blockWidth + LetterSpacing), 0);

            fword.Letters.Add(new RenderedLetterBlock
            {
                Index = i,
                Object = lblockrect,
                CrosswordUsed = false
            });
        }

        // Add first word to the collection
        Words.Add(fword);

        Direction currentDirection = Direction.Vertical;
        
        // Render the other words
        for (var j = 1; j < words.Length; j ++)
        {
            var word = words[j];

            var connection = FindMatchingConnection(word,  Words.GetRange(0, j).ToArray());
            Connections.Add(new WordWordConnection
            {
                _2ndWordIndex = j,
                _1stWordIndex = connection.wordIndex,
                _1stWordLetterIndex = connection.letterIndex
            });

            if (connection.set)
            {
                currentDirection = DirectionSwitcher.Switch(Words[connection.wordIndex]._Direction);

                // Update the crossword params on the end word
                Words[connection.wordIndex].Letters[connection.letterIndex].CrosswordUsed = true;

                var letters = new List<RenderedLetterBlock>();

                // TODO: Render the word
                for (var k = 0; k < word.Length; k++)
                {
                    if (k != connection.connectionIndex)
                    {
                        var lblock = Instantiate(LetterBlock, Vector3.zero, Quaternion.Euler(0, 0, 0), Centerer.transform);
                        lblock.Letter = word[k].ToString();
                        lblock.Visible = dev;
                        lblock.UpdateValues();

                        var lblockrect = lblock.gameObject.GetComponent<RectTransform>();
                        var blockWidth = lblockrect.rect.width;
                        var blockHeight = lblockrect.rect.height;

                        var ci = k - connection.connectionIndex;

                        var connectedLetterAnchoredPosition = Words[connection.wordIndex].Letters[connection.letterIndex].Object.GetComponent<RectTransform>().anchoredPosition;
                        var currentLetterOffset = (((currentDirection == Direction.Vertical) ? new Vector2(0, blockHeight + LetterSpacing) : new Vector2(-blockWidth + -LetterSpacing, 0)) * -ci);
                        lblockrect.anchoredPosition = connectedLetterAnchoredPosition + currentLetterOffset;

                        letters.Add(new RenderedLetterBlock
                        {
                            CrosswordUsed = false,
                            Index = k,
                            Object = lblockrect
                        });
                    }
                    else
                    {
                        letters.Add(new RenderedLetterBlock
                        {
                            CrosswordUsed = true,
                            Index = k,
                            Object = null
                        });
                    }
                }

                // Add the word the list
                Words.Add(new RenderedWordBlock
                {
                    Word = word,
                    Letters = letters,
                    _Direction = currentDirection
                });
            }
            else
            {
                print("Connection has not been made");
            }
        }

        // Center the rendered blocks
        var center = GetCenterOfObjects();
        Centerer.GetComponent<RectTransform>().anchoredPosition = -center;

        if (PreviousAnimatedWords == null)
        {
            PreviousAnimatedWords = new Dictionary<string, bool>();

            foreach (var word in words)
                PreviousAnimatedWords.Add(word, false);
        }

        yield return new WaitForSeconds(5f);

        StartCoroutine(FadeInBlocks());
    }

    public void RenderHint()
    {
        // If user actually has remaining hints and hasn't used more than 3 hints in a single match
        if (User.CurrentUser.hints.count > 0 && UsedHints < USABLE_HINT_LIMIT_PER_MATCH)
        {
            // Reduce available hints count
            var newHints = User.CurrentUser.hints.count - 1;
            FirebaseDatabase.DefaultInstance.RootReference.Child("user").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).Child("hints").SetRawJsonValueAsync(JsonUtility.ToJson(new Hints { count = newHints }));

            var foundWords = new List<string>();
            WordMatches.ForEach((word) => { foundWords.Add(word.word); });

            var unfoundWords = Words.Where((word) => !foundWords.Contains(word.Word));

            // Use hint
            UsedHints++;

            Hint(unfoundWords.ToArray());
        }
        else
        {
            if (User.CurrentUser.hints.count == 0)
            {
                StartCoroutine(ShowNotification(ZeroHintsRemainingNotification));
                return;
            }

            if (UsedHints == USABLE_HINT_LIMIT_PER_MATCH)
            {
                StartCoroutine(ShowNotification(AllHintsUsedNotification));
            }
        }
    }

    IEnumerator ShowNotification(CanvasGroup group)
    {
        while (group.alpha < 1)
        {
            group.alpha = Mathf.MoveTowards(group.alpha, 1, Time.deltaTime * 4f);
            yield return null;
        }

        yield return new WaitForSeconds(4);

        while(group.alpha > 0)
        {
            group.alpha = Mathf.MoveTowards(group.alpha, 0, Time.deltaTime * 5f);
            yield return null;
        }
    }

    void Hint(RenderedWordBlock[] words)
    {
        // Get letters that aren't used in crosswords and aren't revealed before using a hint
        var crossWordNotUsedLetters = (words[UnityEngine.Random.Range(0, words.Length)].Letters).Where((x) => {
            return !x.CrosswordUsed && !x.Object.gameObject.GetComponent<LetterBlockItem>().Visible;
        }).ToArray();

        var letter = crossWordNotUsedLetters[UnityEngine.Random.Range(0, crossWordNotUsedLetters.Length)].Object.GetComponent<LetterBlockItem>();
        letter.Visible = true;
        letter._Ownership = Ownership.Neutral;
        letter.UpdateValues();

        // Play hint animation
        HintAnimator.Play("Used");

        // Play the hint sound
        SoundManager.Instance.PlayClip("HINT");
    }

    IEnumerator FadeInBlocks()
    {
        var cg = Centerer.gameObject.GetComponent<CanvasGroup>();

        while (cg.alpha < 1)
        {
            cg.alpha += Time.deltaTime * 2f;
            yield return null;
        }
    }

    public void MarkCorrectWords()
    {
        foreach (var wordMatch in WordMatches)
        {
            var onceAnimated = false;

            if (PreviousAnimatedWords != null)
            {
                if (PreviousAnimatedWords.ContainsKey(wordMatch.word))
                {
                    onceAnimated = PreviousAnimatedWords[wordMatch.word];
                    PreviousAnimatedWords[wordMatch.word] = true; // Set previous animated to true
                }
            }

            var wItem = Words.Find(x => x.Word == wordMatch.word);
            
            if (wItem != null)
            {
                int index = -1;
                index = Words.FindIndex(x => x.Word == wordMatch.word);
                WordWordConnection connection = null;
                connection = Connections.Find(x => x._2ndWordIndex == index);

                if (index != -1 && connection != null)
                    if (Words[connection._1stWordIndex].Letters[connection._1stWordLetterIndex].Object != null)
                    {
                        ShowLetter(
                            Words[connection._1stWordIndex].Letters[connection._1stWordLetterIndex].Object.GetComponent<LetterBlockItem>(),
                            wordMatch.owner,
                            connection._1stWordLetterIndex,
                            onceAnimated
                        );
                    }

                for (var k = 0; k < wItem.Letters.Count; k ++)
                {
                    var letter = wItem.Letters[k];

                    if (letter.Object != null)
                    {
                        var block = letter.Object.GetComponent<LetterBlockItem>();
                        ShowLetter(block, wordMatch.owner, k, onceAnimated);
                    }
                }
            }
        }
    }

    void ShowLetter(LetterBlockItem block, Ownership owner, int index = 0, bool onceAnimated = false)
    {
        block._Ownership = (owner);
        block.Visible = true;
        block.WordOffset = index;
        block.OnceAnimated = onceAnimated;
        block.UpdateValues();
    }

    Vector3 GetCenterOfObjects()
    {
        float minX = 0;
        float minY = 0;
        float maxX = 0;
        float maxY = 0;

        foreach (Transform child in Centerer)
        {
            var anchor = child.GetComponent<RectTransform>().anchoredPosition;

            if (anchor.x < minX)
                minX = anchor.x;
            if (anchor.y < minY)
                minY = anchor.y;

            if (anchor.x > maxX)
                maxX = anchor.x;
            if (anchor.y > maxY)
                maxY = anchor.y;
        }

        return new Vector3((maxX + minX) / 2, (maxY + minY) / 2, 0);
    }

    WordLetterConnection FindMatchingConnection(string word, RenderedWordBlock[] previouslyAddedWords)
    {
        var foundConnectionIndex = -1; // index of the letter connection is being made to
        var foundWordIndex = -1; // index of the previously added word
        var foundLetterIndex = -1; // index of the letter of the previously added word

        // Check every letter in the word
        for (int i = 0; i < word.Length; i++)
        {
            var letter = word[i];

            // check every word for the letter
            for (var j = 0; j < previouslyAddedWords.Length; j++)
            {
                var w = previouslyAddedWords[j];

                // Check if the word contains the letter
                if (w.Word.Contains(letter.ToString()))
                {
                    // Find the first letter equivalence
                    var index = w.Word.IndexOf(letter);

                    var CONSEQUETIVE_INVALIDANCE = false;

                    // consequtive ones aren't used
                    if (index > 0)
                        if (w.Letters[index - 1].CrosswordUsed)
                            CONSEQUETIVE_INVALIDANCE = true;
                    if (w.Letters.Count > index + 1)
                        if (w.Letters[index + 1].CrosswordUsed)
                            CONSEQUETIVE_INVALIDANCE = true;

                    // COMPLEXITY CALCULATION
                    var complex = CalculateComplexity(w);
                    var GRAPH_COMPLEXITY = (complex < (w.Word.Length / 2f));

                    if (!w.Letters[index].CrosswordUsed && !CONSEQUETIVE_INVALIDANCE && GRAPH_COMPLEXITY)
                    {
                        foundConnectionIndex = i;
                        foundWordIndex = j;
                        foundLetterIndex = index;
                        break;
                    }
                }
            }

            // already found something break the loop
            if (foundWordIndex != -1 && foundLetterIndex != -1)
                break;
        }

        return new WordLetterConnection { set = (foundLetterIndex != -1 && foundWordIndex != -1), letterIndex = foundLetterIndex, wordIndex = foundWordIndex, connectionIndex = foundConnectionIndex };
    }

    float CalculateComplexity(RenderedWordBlock w)
    {
        var crosswordedCount = w.Letters.Count(x => x.CrosswordUsed);
        return ((float)crosswordedCount);
    }

    public struct WordLetterConnection
    {
        public bool set;
        public int connectionIndex;
        public int wordIndex;
        public int letterIndex;
    }

    bool RenderedOnce = false;

    /// <summary>
    /// Renders the given words
    /// </summary>
    public void Render()
    {
        if (!RenderedOnce)
        {
            StartCoroutine(RenderWords(_Words));
            RenderedOnce = true;
        }
    }

    [TextArea]
    public string WordsDEV;

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(RenderWords(WordsDEV.Split(','), true));
            selectionCircle.Render(WordsDEV.Split(','), true);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            string path = Application.streamingAssetsPath + "/generated.json";
            string contents = File.ReadAllText(path);
            
            GeneratedWordsClass gen = JsonConvert.DeserializeObject<GeneratedWordsClass>(contents);

            if (!gen.words.Exists(x => x == WordsDEV.Split(',')))
                gen.words.Add(WordsDEV.Split(','));

            File.WriteAllText(path, JsonConvert.SerializeObject(gen));
        }
#endif
    }

    public Vector3 Offset;
}

public class RenderedWordBlock
{
    public string Word;
    public List<RenderedLetterBlock> Letters;
    public Direction _Direction;
}

public class RenderedLetterBlock
{
    public int Index;
    public RectTransform Object;
    public bool CrosswordUsed = false;
}

public class WordWordConnection
{
    public int _2ndWordIndex;
    public int _1stWordIndex;
    public int _1stWordLetterIndex;
}

public enum Direction { Horizontal, Vertical }

public static class DirectionSwitcher
{
    public static Direction Switch(Direction current)
    {
        return (current == Direction.Horizontal) ? Direction.Vertical : Direction.Horizontal;
    }
}

[Serializable]
public class GeneratedWordsClass
{
    public List<string[]> words;
}