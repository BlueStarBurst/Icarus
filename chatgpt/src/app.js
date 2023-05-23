
import { render } from "react-dom";
import React from "react";
import { useEffect } from "react";
import { Button } from "react-bootstrap";

let SpeechRecognition = window.webkitSpeechRecognition || window.SpeechRecognition
const recognition = new SpeechRecognition();

// speech to text
function App() {
    const [text, setText] = React.useState([]);
    const [isListening, setIsListening] = React.useState(false);

    useEffect(() => {
        recognition.continuous = true;
        recognition.interimResults = true;
        recognition.lang = 'en-US';
        recognition.onstart = function () {
            setIsListening(true);
            console.log("We are listening. Try speaking into the microphone.");
        };

        recognition.onspeechend = function () {
            // when user is done speaking
            setIsListening(false);
            recognition.stop();

            // recognition.start(); // restart recognition
        }

        // This runs when the speech recognition service returns result
        recognition.onresult = function (event) {
            // var transcript = event.results[0][0].transcript;
            var confidence = event.results[0][0].confidence;
            console.log(event.results, confidence);
            // get the array of results and set it to text
            var temp = [];
            for (let i = 0; i < event.results.length; i++) {
                temp.push(event.results[i]);
            }
            setText(temp);
        };

        // start recognition
        // recognition.start();

    }, []);

    return (
        <div>
            <h1>Chat GPT</h1>
            <p>
                {text.map((result, index) => (
                    <p key={index}>{result[0].transcript}</p>
                ))}
            </p>
            <Button variant={isListening ? "danger" : "primary"}
                onClick={(e) => {
                    if (!isListening) {
                        recognition.start();
                    } else {
                        recognition.stop();
                    }
                }} >
                {isListening ? "Stop" : "Start"}
            </Button>
        </div>
    );
}

render(<App />, document.getElementById("root"));