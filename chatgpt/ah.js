


var url = 'localhost:5000'
var uri = 'http://' + url + '/api/v1/generate'

var prompt = "How many hours are in a year?"
var request = {
    'prompt': prompt,
    'max_new_tokens': 250,
    'do_sample': true,
    'temperature': 1.3,
    'top_p': 0.1,
    'typical_p': 1,
    'repetition_penalty': 1.18,
    'top_k': 40,
    'min_length': 0,
    'no_repeat_ngram_size': 0,
    'num_beams': 1,
    'penalty_alpha': 0,
    'length_penalty': 1,
    'early_stopping': false,
    'seed': -1,
    'add_bos_token': true,
    'truncation_length': 2048,
    'ban_eos_token': false,
    'skip_special_tokens': true,
    'stopping_strings': []
}

fetch(uri, {
    method: 'POST',
    body: JSON.stringify(request),
    headers: {
        'Content-Type': 'application/json'
    }
}).then(res => res.json())
    .then(json => console.log(json))
    .catch(err => console.log(err));
