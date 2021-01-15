const functions = require('firebase-functions');
const admin = require('firebase-admin');
const randomstring = require("randomstring");
const { object } = require('firebase-functions/lib/providers/storage');

admin.initializeApp(functions.config().firebase);

var database = admin.database();

const MATCHMAKING_PLACEHOLDER = "MATCHMAKING";

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
                // calculate the match stuff
                var tempMax = -1;
                var winner = null;
                var draw = false;
                
                Object.keys(answers).forEach((playerId) => 
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
            }

            return true;
        });

        return true;
    })

});

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
            var match = 
            {
                players: [context.params.playerId, secondPlayer.key],
                content: {
                    words: [
                        "TEACHER",
                        "TEACH",
                        "CREATE",
                        "TEAR"
                    ]
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

function generateGameId()
{
    return randomstring.generate(15);
}