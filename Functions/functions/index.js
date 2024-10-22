const functions = require('firebase-functions');
const admin = require('firebase-admin');
const randomstring = require("randomstring");
var words = require('./data/words.json')

admin.initializeApp(functions.config().firebase);

var database = admin.database();

const MATCHMAKING_PLACEHOLDER = "MATCHMAKING";

exports.clearMatches = functions.pubsub.schedule("every 5 minutes").onRun((context) => {

    database.ref("match/").once("value").then((snapshot) => {
        var value = snapshot.val();
        
        if (value != null)
        {
            Object.keys(value).forEach(function(key) {
                var duration = Date.now() - value[key]["timestamp"];

                // If the match is older than an hour
                if (duration > (1000 * 60 * 60))
                {
                    // delete the database
                    database.ref("match/" + key).set(null);
                    console.log("Match (" + key + ") is old, so deleted to save RTDB space");
                }
            });
        }
    });
    
    return null;
});

// ON BOTH PLAYERS ARE READY FOR RESTART
exports.resetMatch = functions.database.ref("match/{matchid}/ready").onUpdate((snap, context) => 
{
    var matchid = context.params.matchid;

    database.ref("match/" + matchid).once("value").then((snap) => 
    {
        var matchdata = snap.val();

        var players = matchdata["players"];
        var readyPlayers = matchdata["ready"];

        // Both players are ready for the next round
        if (readyPlayers[players[0]] && readyPlayers[players[1]])
            resetMatch(matchid);
    });

});

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

            // If no one has answered, the match is a draw
            if (Object.keys(answers).length == 0)
                draw = true;
    
            database.ref("match/" + matchid + "/status").set({
                draw : draw,
                winner : (draw) ? null : winner
            });

            // MATCH CONCLUSION SAVING
            database.ref("match/" + matchid + "/players").once("value").then((snapshot) => 
            {
                var players = snapshot.val();
                var loser = players.find((x) => x != winner);

                // set player statistics
                if (!draw)
                {
                    database.ref("user/" + winner + "/statistics/wins").once("value").then((snap) => 
                    {
                        database.ref("user/" + winner + "/statistics/wins").set(snap.val() + 1)
                    });

                    // Gain 150 XP per winning match
                    database.ref("user/" + winner + "/xp").once("value").then((snap) => 
                    {
                        var currentXP = snap.val();
                        database.ref("user/" + winner + "/xp").set(currentXP + 150);
                    });

                    database.ref("user/" + loser + "/statistics/losses").once("value").then((snap) => 
                    {
                        database.ref("user/" + loser + "/statistics/losses").set(snap.val() + 1)
                    });

                    // Gain 25 XP per finishing match
                    database.ref("user/" + loser + "/xp").once("value").then((snap) => 
                    {
                        var currentXP = snap.val();
                        database.ref("user/" + loser + "/xp").set(currentXP + 25);
                    });
                } else {
                    // on draw give both players 50 XP
                    database.ref("user/" + players[0] + "/xp").once("value").then((snap) => 
                    {
                        var currentXP = snap.val();
                        database.ref("user/" + players[0] + "/xp").set(currentXP + 50);
                    });

                    database.ref("user/" + players[1] + "/xp").once("value").then((snap) => 
                    {
                        var currentXP = snap.val();
                        database.ref("user/" + players[1] + "/xp").set(currentXP + 50);
                    });
                }
            });
           
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
                
                database.ref("match/" + matchid + "/status").set({
                    draw : draw,
                    winner : (draw) ? null : winner
                });
                
                // MATCH CONCLUSION SAVING
                database.ref("match/" + matchid + "/players").once("value").then((snapshot) => 
                {
                    var players = snapshot.val();
                    var loser = players.find((x) => x != winner);

                    // set player statistics
                    if (!draw)
                    {
                        database.ref("user/" + winner + "/statistics/wins").once("value").then((snap) => 
                        {
                            database.ref("user/" + winner + "/statistics/wins").set(snap.val() + 1)
                        });
    
                        // Gain 150 XP per winning match
                        database.ref("user/" + winner + "/xp").once("value").then((snap) => 
                        {
                            var currentXP = snap.val();
                            database.ref("user/" + winner + "/xp").set(currentXP + 150);
                        });
    
                        database.ref("user/" + loser + "/statistics/losses").once("value").then((snap) => 
                        {
                            database.ref("user/" + loser + "/statistics/losses").set(snap.val() + 1)
                        });
    
                        // Gain 25 XP per finishing match
                        database.ref("user/" + loser + "/xp").once("value").then((snap) => 
                        {
                            var currentXP = snap.val();
                            database.ref("user/" + loser + "/xp").set(currentXP + 25);
                        });
                    } else {
                        // on draw give both players 50 XP
                        database.ref("user/" + players[0] + "/xp").once("value").then((snap) => 
                        {
                            var currentXP = snap.val();
                            database.ref("user/" + players[0] + "/xp").set(currentXP + 50);
                        });

                        database.ref("user/" + players[1] + "/xp").once("value").then((snap) => 
                        {
                            var currentXP = snap.val();
                            database.ref("user/" + players[1] + "/xp").set(currentXP + 50);
                        });
                    }
                });
               

            }

            return true;
        });

        return true;
    })

});

function resetMatch(matchid)
{
    // Reset match status
    database.ref("match/" + matchid + "/status").set(null);
    // Reset current matches
    database.ref("match/" + matchid + "/matches").set(null);
    // Neither players should be ready for round two ending
    database.ref("match/" + matchid + "/ready").set(null);
    // Reset the words for the match (new match);
    set = generateWords();
    database.ref("match/" + matchid + "/content/words").set(set);
}

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
                },
                timestamp: Date.now()
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