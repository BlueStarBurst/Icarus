import asyncio
import json
import sys

try:
    import websockets
except ImportError:
    print("Websockets package not found. Make sure it's installed.")

# For local streaming, the websockets are hosted without ssl - ws://

URI = f'wss://treating-navigation-section-without.trycloudflare.com/api/v1/stream'

# For reverse-proxied streaming, the remote will likely host with ssl - wss://
# URI = 'wss://your-uri-here.trycloudflare.com/api/v1/stream'

history = []
name = "Icarus"
context = "chat: Your name is Icarus. You are a world renowned hacker and programmer. You are currently a live streamer on Twitch and you are talking to your viewers."

async def run(_context):
    # Note: the selected defaults change from time to time.
    prompt = context
    for i in range(len(history)):
        prompt += history[i]

    request = {
        'prompt': prompt,
        'max_new_tokens': 50,
        'do_sample': True,
        'temperature': 0.7,
        'top_p': 0.5,
        'typical_p': 1,
        'repetition_penalty': 1.2,
        'encoder_repetition_penalty': 1,
        'top_k': 40,
        'min_length': 0,
        'no_repeat_ngram_size': 0,
        'num_beams': 1,
        'penalty_alpha': 0,
        'length_penalty': 1,
        'early_stopping': True,
        'seed': -1,
        'add_bos_token': True,
        'truncation_length': 2048,
        'ban_eos_token': False,
        'skip_special_tokens': True,
        'stopping_strings': ["\nchat: ", "\n" + name + ": "]
    }

    async with websockets.connect(URI, ping_interval=None) as websocket:
        await websocket.send(json.dumps(request))

        # yield context  # Remove this if you just want to see the reply

        while True:
            incoming_data = await websocket.recv()
            incoming_data = json.loads(incoming_data)

            match incoming_data['event']:
                case 'text_stream':
                    yield incoming_data['text']
                case 'stream_end':
                    return


async def print_response_stream(prompt):
    responseStr = ''
    # print(response, end='')
    async for response in run(prompt):
        print(response, end='')
        sys.stdout.flush()  # If we don't flush, we won't see tokens in realtime.
        responseStr += response.replace("\n", " ")
    # history.append(f"\n{name}: " + responseStr)
    history.append("\n" + responseStr)



prompt = "what is your favorite thing to do on the weekend?"
history.append("\nchat: " + prompt)
asyncio.run(print_response_stream(prompt))
print(history)