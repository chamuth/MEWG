const functions = require('firebase-functions');
const admin = require('firebase-admin');
const randomstring = require("randomstring");
var words = require('./data/words.json')

admin.initializeApp(functions.config().firebase);

var database = admin.database();

const MATCHMAKING_PLACEHOLDER = "MATCHMAKING";

// ON GAME TIMER IS OUT
exports.timeout = functions.database.ref("match/{matchid}/time").onCreate((snap, context) => 
{
    // Game timer is out
    var matchid = context.params.matchid;
    var answers = { }
    var answersCount = 0;

    database.ref("match/" + matchid + "/matches").once("value").then(matches => 
    {
        matches.forEach((match) => 
        {
            var uid = match.val()["uid"];

            if (answers[uid])
                answers[uid] = answers[uid] + 1;
            else
                answers[uid] = 1; 

            answersCount++;
        });
        
        database.ref("match/" + matchid + "/content/words").once("value").then(snap => 
        {
            var totalWords = snap.numChildren();

            // calculate the winner and loser
            var tempMax = -1;
            var winner = null;
            var draw = false;
            
            var _players = Object.keys(answers);

            _players.forEach((playerId) => 
            {
                if (answers[playerId] == tempMax && tempMax != -1)
                    draw = true;

                if (answers[playerId] > tempMax)
                {
                    tempMax = answers[playerId];
                    winner = playerId;
                }
            });

            var loser = _players.find((x,i,o) => x != winner);
    
            database.ref("match/" + matchid + "/status").set({
                draw : draw,
                winner : (draw) ? null : winner
            });

            // set player statistics
            if (!draw)
            {
                database.ref("user/" + winner + "/statistics/wins").once("value").then((snap) => 
                {
                    database.ref("user/" + winner + "/statistics/wins").set(snap.val() + 1)
                });

                database.ref("user/" + loser + "/statistics/losses").once("value").then((snap) => 
                {
                    database.ref("user/" + loser + "/statistics/losses").set(snap.val() + 1)
                });
            }

            return true;
        });

        return true;
    })
});

// ON MATCH DATA UPDATED / WORD MATCHED
exports.matchUpdated = functions.database.ref("match/{matchid}/matches").onUpdate((snap, context) => 
{
    // Check if the game is over and the players are won
    var matchid = context.params.matchid;
    var answers = { }
    var answersCount = 0;

    database.ref("match/" + matchid + "/matches").once("value").then(matches => 
    {
        matches.forEach((match) => 
        {
            var uid = match.val()["uid"];

            if (answers[uid])
                answers[uid] = answers[uid] + 1;
            else
                answers[uid] = 1; 

            answersCount++;        
        });
        
        database.ref("match/" + matchid + "/content/words").once("value").then(snap => 
        {
            var totalWords = snap.numChildren();

            if (totalWords == answersCount)
            {
                // Match finished
                // calculate the match stuff
                var tempMax = -1;
                var winner = null;
                var draw = false;
                
                var _players = Object.keys(answers);

                _players.forEach((playerId) => 
                {
                    if (answers[playerId] == tempMax && tempMax != -1)
                        draw = true;

                    if (answers[playerId] > tempMax)
                    {
                        tempMax = answers[playerId];
                        winner = playerId;
                    }
                });

                var loser = _players.find((x,i,o) => x != winner);
        
                database.ref("match/" + matchid + "/status").set({
                    draw : draw,
                    winner : (draw) ? null : winner
                });

                // set player statistics
                if (!draw)
                {
                    database.ref("user/" + winner + "/statistics/wins").once("value").then((snap) => 
                    {
                        database.ref("user/" + winner + "/statistics/wins").set(snap.val() + 1)
                    });

                    // Gain 100 XP per winning match
                    database.ref("user/" + winner + "/xp").once("value").then((snap) => 
                    {
                        var currentXP = snap.val();
                        database.ref("user/" + winner + "/xp").set(currentXP + 100);
                    });

                    database.ref("user/" + loser + "/statistics/losses").once("value").then((snap) => 
                    {
                        database.ref("user/" + loser + "/statistics/losses").set(snap.val() + 1)
                    });
                }
            }

            return true;
        });

        return true;
    })

});

// ON MATCHMAKING QUEUE UPDATED
exports.matchmaker = functions.database.ref('matchmaking/{playerId}').onCreate((snap, context) => {
    
    var gameId = generateGameId();

    database.ref('matchmaking').once('value').then(players => 
    {        
        var secondPlayer = null;

        players.forEach(player => 
        {
            // If the player is actually matchmaking and not the first player, set the second player
            if (player.val() === MATCHMAKING_PLACEHOLDER && player.key !== context.params.playerId) {
                secondPlayer = player;
            }
        });

        // End if no match found, he'll be matched once a second player enters matchmaking
        if (secondPlayer === null) return null;

        database.ref("matchmaking").transaction(function (matchmaking) {

            // If one of the players aren't matchmaking currently
            if (matchmaking === null || matchmaking[context.params.playerId] !== MATCHMAKING_PLACEHOLDER || matchmaking[secondPlayer.key] !== MATCHMAKING_PLACEHOLDER) 
                return matchmaking;

            // Set GameID for the players
            matchmaking[context.params.playerId] = gameId;
            matchmaking[secondPlayer.key] = gameId;

            return matchmaking;

        }).then(result => {

            // If the match hasn't been set stop the match creation
            if (result.snapshot.child(context.params.playerId).val() !== gameId) return;
            
            // Start the match
            set = generateWords();

            var match = 
            {
                players: [context.params.playerId, secondPlayer.key],
                content: {
                    words: set
                }
            }
            
            database.ref("match/" + gameId).set(match).then(snapshot => {
                console.log("Game created successfully!")
                return null;
            }).catch(error => {
                console.log(error);
            });

            return null;

        }).catch(error => {
            console.log(error);
        });

        return null;

    }).catch(error => 
    {
        console.log(error);
    });

});

function generateWords()
{
    var wordSet = words.sets[Math.floor(Math.random() * words.sets.length)];
    return wordSet;
}

function generateGameId()
{
    return randomstring.generate(15);
}