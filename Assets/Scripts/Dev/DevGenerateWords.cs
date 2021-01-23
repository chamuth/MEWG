using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public class DevGenerateWords : MonoBehaviour
{
    WordsBlocksContainer container;
    string[] words;

    void Start()
    {
        container = GetComponent<WordsBlocksContainer>();

        string path = Application.streamingAssetsPath + "/dev_common_words.json";
        string contents = File.ReadAllText(path);
        words = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(contents);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateWords();
        }
    }

    void GenerateWords()
    {
        var selectedWords = new List<string>();

        // Select first word
        var compatible = words.Where((x) => (x.Length > 2 && x.Length < 7));

        // Select first word randomly
        var firstWord = compatible.ElementAt(Random.Range(0, compatible.Count()));
        selectedWords.Add(firstWord);

        var count = Random.Range(4, 7);

        // Connect word to the words
        for (var i = 0; i < count; i++)
        {
            var letters = GetAllLetters(selectedWords);
            var letter = letters[Random.Range(0, letters.Length)];

            var wordsWithLetter = compatible.Where((x) =>
            {
                if (!selectedWords.Contains(x))
                {
                    var prevLetters = GetAllLetters(new List<string>(new string[] { selectedWords[i] })); // letters in previous words
                    return (prevLetters.Count(y => x.Contains(y)) > 3); // if there's more than x letters that are in the previous words
                }
                else
                {
                    return false;
                }
            });

            if (wordsWithLetter.Count() > 0)
            {
                var connectionword = wordsWithLetter.ElementAt(Random.Range(0, wordsWithLetter.Count()));
                selectedWords.Add(connectionword);
            }
            else
            {
                print("No word matching");
                
                // If there's less than 4 words
                if (i < 3)
                    GenerateWords();

                break;
            }
        }

        var lettersCount = GetAllLetters(selectedWords).Count();
        print(lettersCount + " Letters in each word");
        if (lettersCount < 8)
        {
            container.WordsDEV = string.Join(",", selectedWords.Select(x => x.ToUpper()).ToArray());
        }
        else
        {
            // too much letters regenerate
            print("TOO MUCH LETTERS");
            GenerateWords();
        }
    }

    string[] GetAllLetters(List<string> words)
    {
        var l = new List<string>();
        
        foreach(var word in words)
        {
            foreach (var letter in word)
            {
                if (!l.Contains(letter.ToString()))
                    l.Add(letter.ToString());
            }
        }

        return l.ToArray();
    }
}
