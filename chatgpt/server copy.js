import { ChatGPTAPI } from "chatgpt";
import pkg from 'node-rake-v2';
import fs from 'fs';

import player from 'node-wav-player'

// import websocketclient from 'websocket'; 
import WebSocket from 'ws';
import tmi from 'tmi.js'

// import express
import express from 'express';
const app = express();
const port = 3000;

// import say from 'say';
// say.getInstalledVoices(console.log)
// say.speak("Hello, world!", 'Microsoft Haruka Desktop');

import * as googleTTS from 'google-tts-api'; // ES6 or TypeScript
const url = googleTTS.getAudioUrl('Hello World', {
    lang: 'en',
    slow: false,
    host: 'https://translate.google.com',
});
console.log(url);

const { NodeRakeV2 } = pkg;
const rake = new NodeRakeV2();
const stopWords = fs.readFileSync('stopWords.txt', 'utf8').split('\n');
rake.addStopWords(stopWords);
// rake.addStopWords(['for', 'the', 'a', 'stands', 'test']);

// get string from icarus.txt
var icarus = fs.readFileSync('icarus.txt', 'utf8').replace(/\n/g, ' ');
var name = "Icarus";
var ws = null;
var voiceWs = null;

const api = new ChatGPTAPI({
    apiKey: "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",
    completionParams: {
        temperature: 0.5,
        top_p: 0.8,
        max_tokens: 50,
    },
    maxModelTokens: 2048,
    maxResponseTokens: 50,
    systemMessage: icarus,
});

var res = null;

var actions = ["jump", "walk", "run", "turn", "spin"];
var directions = ["left", "right", "forward", "backward"];

var numbers = ["one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten"];

function checkContains(word) {
    for (var i = 0; i < actions.length; i++) {
        if (word.indexOf(actions[i]) != -1) {
            console.log(actions[i]);
            return actions[i];
        }
    }
    for (var i = 0; i < directions.length; i++) {
        if (word.includes(directions[i])) {
            console.log(directions[i]);
            return directions[i];
        }
    }
    // for (var i = 0; i < numbers.length; i++) {
    //     if (word.includes(numbers[i])) {
    //         var temp = numbers.indexOf(word) + 1;
    //         return temp;
    //     }
    // }
    // get number
    var num = parseFloat(word);
    if (num) {
        return num;
    }
    return false;
}

var index = 0;
var foundAction = false;

function attemptAction(actionString) {
    if (!ws) {
        return;
    }
    console.log(actionString);
    var temp = actionString.split(' ');
    var verb = null;
    var direction = null;
    var quantity = null;
    var tempVerb = null;
    var tempDirection = null;
    var tempQuantity = Math.floor(Math.random() * 10) + 1;
    var word;
    for (var i = 0; i < temp.length; i++) {
        word = temp[i];
        var result = checkContains(word);
        tempVerb = null;
        tempDirection = null;
        tempQuantity = Math.floor(Math.random() * 10) + 1;

        if (result) {
            if (typeof result == "string") {
                if (actions.includes(result)) {
                    if (verb) {
                        tempVerb = result;
                    }
                    else {
                        verb = result;
                    }
                } else {
                    if (direction) {
                        tempDirection = result;
                    } else {
                        direction = result;
                    }
                }
            } else {
                quantity = result;
            }

            console.log("verb: " + verb + " direction: " + direction + " quantity: " + quantity);
            console.log("tempVerb: " + tempVerb + " tempDirection: " + tempDirection + " tempQuantity: " + tempQuantity);

            if ((verb && tempVerb) || (direction && tempDirection) || (verb && direction && quantity)) {
                // send the verb, direction, and quantity to the game
                if (quantity == null) {
                    if (verb == "spin") {
                        quantity = 1;
                    } else {
                        quantity = tempQuantity;
                    }
                }
                if (verb == null) {
                    verb = tempVerb || "";
                } else if (direction == null) {
                    direction = tempDirection || "";
                }
                console.log("verb: " + verb + " direction: " + direction + " quantity: " + quantity);
                ws.send(verb + "," + direction + "," + quantity);
                verb = null;
                direction = null;
                quantity = null;
                if (tempVerb) {
                    verb = tempVerb;
                } else {
                    direction = tempDirection;
                }
            }

        }
    }
}

var actionIndexes = [];
var actionsStack = [];

function onPartialResponse(partialResponse) {
    // console.log(partialResponse.text);
    // find the next action in the response denoted by *action*
    for (var i = index; i < partialResponse.text.length; i++) {
        if (partialResponse.text[i] == '*') {
            if (foundAction) {
                // attemptAction(partialResponse.text.substring(index, i))
                foundAction = false;
                actionIndexes.push(i + 1);
            } else {
                foundAction = true;
                index = i + 1;
                actionIndexes.push(i);
            }
        } else if (!foundAction) {
            index = i + 2;
        }
    }
    // if action indexes is odd, then the last action was not completed
    if (actionIndexes.length % 2 == 1) {
        actionIndexes.push(partialResponse.text.length);
    }
}


async function callGPT(prompt) {
    if (!res) {
        res = await api.sendMessage(prompt, {
            onProgress: onPartialResponse
        });
    } else {
        res = await api.sendMessage(prompt, {
            parentMessageId: res.id,
            onProgress: onPartialResponse
        });
    }


    index = 0;
    foundAction = false;
    actionIndexes = [];
    console.log(res.text);
    // find the next action in the response denoted by *action*
    for (var i = index; i < res.text.length; i++) {
        if (res.text[i] == '*') {
            if (foundAction) {
                // attemptAction(partialResponse.text.substring(index, i))
                foundAction = false;
                actionIndexes.push(i + 1);
            } else {
                foundAction = true;
                index = i + 1;
                actionIndexes.push(i);
            }
        } else if (!foundAction) {
            index = i + 2;
        }
    }
    // if action indexes is odd, then the last action was not completed
    if (actionIndexes.length % 2 == 1) {
        actionIndexes.push(res.text.length);
    }

    var actionsL = [];
    // get rid of the actions in the response and return the rest
    var tempRes = res.text;
    for (var i = 0; i < actionIndexes.length; i += 2) {
        var start = actionIndexes[i];
        var end = actionIndexes[i + 1];
        var temp = res.text.substring(start, end);
        actionsL.push(temp.substring(1, temp.length - 1));
        tempRes = res.text.replace(temp, '');
    }
    res.text = tempRes;

    if (voiceWs) {
        voiceWs.send(res.text.replace(name + ": ", ' '));
        actionsStack.push(actionsL);
    } else {
        // go through the actions and attempt to do them
        runActions(actionsL);
    }

    console.log(res.text.replace(name + ": ", ' '))

    return res.text.replace(name + ": ", ' ');
}

async function onPrompt(prompt) {
    return await callGPT(prompt);
    return "";
}

var msgStack = [];
var client;
var isEmpty = true;
var isBusy = false;
var returnMsgStack = [];

async function processMsg() {
    isBusy = true;
    var msg = msgStack.shift();
    var returnMsg = await onPrompt(msg.tags['display-name'] + ": " + msg.msg);
    // console.log(returnMsg);

    if (!voiceWs) {
        client.say(msg.channel, returnMsg);
    } else {
        returnMsgStack.push([returnMsg, msg.channel]);
    }

    isBusy = false;
    if (msgStack.length > 0) {
        processMsg();
    } else {
        isEmpty = true;
    }
}

function addMsg(msg, tags, channel) {
    // check if the message contains words from dictionary
    if (msg.length > 100) {
        return;
    }
    msgStack.push({
        'msg': msg,
        'time': Date.now(),
        'tags': tags,
        'channel': channel
    });
    if (msgStack.length > 10) {
        msgStack.shift();
    }
    if (isEmpty && !isBusy) {
        isEmpty = false;
        processMsg();
    }
}

// make a twitch bot that reads chat and responds to questions
async function twitchBot() {
    var channel = "Inkobako";
    const account = "Inkobako";

    const token = "oauth:05m9olo6ieffclwud10hpm9r64jek4"

    client = new tmi.Client({
        options: { debug: true },
        connection: {
            secure: true,
            reconnect: true
        },
        identity: {
            username: account,
            password: token
        },
        channels: [channel]
    });

    client.connect().then(() => {
        console.log("Connected to Twitch");
    }).catch((err) => {
        console.log(err);
        process.exit(1);
    });

    client.on('message', async (channel, tags, message, self) => {
        console.log(message);
        if (self) return;
        addMsg(message, tags, channel);
    });
}

async function createServer() {
    const wss = new WebSocket.Server({
        port: 2567,
        maxPayload: 1024 * 1024 * 50 // 50mb 
    });

    wss.on('connection', function connection(_ws) {
        _ws.on('message', function incoming(message) {
            // console.log('received: %s', message);
            if (message == "voice") {
                console.log('received: %s', message);
                voiceWs = _ws;
                // _ws.send("This is a test of the tts system");
                return;
            } else if (message == "unity") {
                console.log('received: %s', message);
                ws = _ws;
                return;
            }

            var decoded = JSON.parse(message);
            if (decoded.type == "wav") {
                // there is a base64 encoded wav file in the audio field
                var temp = decoded.audio;
                var binary = Buffer.from(temp, 'base64');
                fs.writeFileSync('test.wav', binary, 'binary');
                // play the audio file
                player.play({
                    path: 'test.wav',
                }).then(() => {
                    console.log("The wav file started playing");
                    if (actionsStack.length != 0) {
                        var actions = actionsStack.shift();
                        var returnMsg = returnMsgStack.shift();
                        client.say(returnMsg[1], returnMsg[0]);
                        runActions(actions);
                    }
                }).catch((error) => {
                    console.error(error);
                });
                return;
            }
        });
    });

    // print the url to connect to
    console.log("Connect to ws://localhost:2567");
}

async function runActions(actionsArr) {
    console.log(actionsArr);
    if (actionsArr && actionsArr.length > 0) {
        for (var i = 0; i < actionsArr.length; i++) {
            var action = actionsArr[i];
            attemptAction(action);
            // wait 1 second
            await new Promise(r => setTimeout(r, 1000));
        }
    }
}

createServer();

twitchBot();

