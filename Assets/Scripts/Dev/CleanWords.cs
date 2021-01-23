using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

public class CleanWords : MonoBehaviour
{
    private void Start()
    {
        string path = Application.streamingAssetsPath + "/words.json";
        string contents = File.ReadAllText(path);
        var w = JsonConvert.DeserializeObject<WordSetClass>(contents);
        
        var qualified = w.sets.Where((words) =>
        {
            var _letters = new List<string>();
            var _wordLetters = new Dictionary<string, int>();

            foreach (var word in words)
            {
                foreach (var letter in word)
                {
                    var amount = word.Count(x => x.ToString() == letter.ToString());

                    if (_wordLetters.ContainsKey(letter.ToString()) && _wordLetters[letter.ToString()] < amount)
                        _wordLetters[letter.ToString()] = amount;

                    if (!_wordLetters.ContainsKey(letter.ToString()))
                        _wordLetters.Add(letter.ToString(), amount);
                }
            }

            foreach (var l in _wordLetters)
                for (var i = 0; i < l.Value; i++)
                    _letters.Add(l.Key);

            print(_letters.Count + " letters");

            if (_letters.Count > 8)
                return false;

            return true;
        });

        print(JsonConvert.SerializeObject(qualified.ToArray()));
    }
}

public class WordSetClass
{
    public List<string[]> sets;
}
