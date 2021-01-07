using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordsBlocksContainer : MonoBehaviour
{
    public LetterBlockItem LetterBlock;
    public float LetterSpacing = 10f;

    List<RenderedWordBlock> Words = new List<RenderedWordBlock>();

    public void RenderWords(string[] words)
    {
        // First word goes brrr
        var fword = new RenderedWordBlock
        {
            Word = words[0],
            Letters = new List<RenderedLetterBlock>(),
            _Direction = Direction.Horizontal // First word is always horizontal
        };

        for (var i = 0; i < words[0].Length; i ++)
        {
            var lblock = Instantiate(LetterBlock, Vector3.zero, Quaternion.Euler(0, 0, 0), transform);
            lblock.Letter = words[0][i].ToString();
            lblock.Visible = true; // DEV PURPOSES
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

        var currentDirection = Direction.Vertical;
        
        // Render the other words
        for (var j = 1; j < words.Length; j ++)
        {
            var word = words[j];

            // Switch the current direction

            var connection = FindMatchingConnection(word,  Words.GetRange(0, j).ToArray());

            if (connection.set)
            {
                print("Connection has been made " + word[connection.connectionIndex] + " of " + word + " to " + word[connection.connectionIndex] + " in " + Words[connection.wordIndex].Word);

                currentDirection = DirectionSwitcher.Switch(Words[connection.wordIndex]._Direction);

                // Update the crossword params on the end word
                Words[connection.wordIndex].Letters[connection.letterIndex].CrosswordUsed = true;

                // TODO: Render the word
                for (var k = 0; k < word.Length; k++)
                {
                    if (k != connection.connectionIndex)
                    {
                        var lblock = Instantiate(LetterBlock, Vector3.zero, Quaternion.Euler(0, 0, 0), transform);
                        lblock.Letter = word[k].ToString();
                        lblock.Visible = true; // DEV PURPOSES
                        lblock.UpdateValues();

                        var lblockrect = lblock.gameObject.GetComponent<RectTransform>();
                        var blockWidth = lblockrect.rect.width;
                        var blockHeight = lblockrect.rect.height;

                        var ci = k - connection.connectionIndex;

                        var connectedLetterAnchoredPosition = Words[connection.wordIndex].Letters[connection.letterIndex].Object.GetComponent<RectTransform>().anchoredPosition;
                        var currentLetterOffset = (((currentDirection == Direction.Vertical) ? new Vector2(0, blockHeight + LetterSpacing) : new Vector2(blockWidth + LetterSpacing, 0)) * -ci);
                        lblockrect.anchoredPosition = connectedLetterAnchoredPosition + currentLetterOffset;
                    }
                }

                var letters = new List<RenderedLetterBlock>();

                // Add the letter connections
                for (var k = 0; k < words[j].Length; k++)
                {
                    letters.Add(new RenderedLetterBlock
                    {
                        CrosswordUsed = true,
                        Index = k,
                        Object = null
                    });
                }

                // Add the word the list
                Words.Add(new RenderedWordBlock
                {
                    Word = word,
                    Letters = letters
                });
            }
            else
            {
                print("Connection has not been made");
            }
        }
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
            for (var j = 0; j < previouslyAddedWords.Length; j ++)
            {
                var w = previouslyAddedWords[j];

                // Check if the word contains the letter
                if (w.Word.Contains(letter.ToString()))
                {
                    // Find the first letter equivalence
                    var index = w.Word.IndexOf(letter);

                    if (!w.Letters[index].CrosswordUsed)
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

    public struct WordLetterConnection
    {
        public bool set;
        public int connectionIndex;
        public int wordIndex;
        public int letterIndex;
    }

    private void Start()
    {
        RenderWords(new string[] {
            "BIGGER", "MEAN", "BLACK", "KAK"
        });
    }
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

public enum Direction { Horizontal, Vertical }

public static class DirectionSwitcher
{
    public static Direction Switch(Direction current)
    {
        return (current == Direction.Horizontal) ? Direction.Vertical : Direction.Horizontal;
    }
}
