﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WordsBlocksContainer : MonoBehaviour
{
    public LetterBlockItem LetterBlock;
    public float LetterSpacing = 10f;
    public Transform Centerer;

    List<RenderedWordBlock> Words = new List<RenderedWordBlock>();

    public IEnumerator RenderWords(string[] words)
    {
        Words = new List<RenderedWordBlock>();

        // clear the parent
        foreach (Transform child in Centerer.transform)
            Destroy(child.gameObject);

        yield return new WaitForSeconds(0.25f);

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

                var letters = new List<RenderedLetterBlock>();

                // TODO: Render the word
                for (var k = 0; k < word.Length; k++)
                {
                    if (k != connection.connectionIndex)
                    {
                        var lblock = Instantiate(LetterBlock, Vector3.zero, Quaternion.Euler(0, 0, 0), Centerer.transform);
                        lblock.Letter = word[k].ToString();
                        lblock.Visible = true; // DEV PURPOSES
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

    private void Start()
    {
        StartCoroutine(RenderWords(new string[] {
            "BIGGER", "BLACK", "KHAKI", "TRY", "ANT", "LIGMA", "AS", "MANGO"
        }));
    }

    [TextArea]
    public string WordsDEV;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(RenderWords(WordsDEV.Split(',')));
        }
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

public enum Direction { Horizontal, Vertical }

public static class DirectionSwitcher
{
    public static Direction Switch(Direction current)
    {
        return (current == Direction.Horizontal) ? Direction.Vertical : Direction.Horizontal;
    }
}