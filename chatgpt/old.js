const { Configuration, OpenAIApi } = require("openai");
const configuration = new Configuration({
    apiKey: "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",
});
const openai = new OpenAIApi(configuration);

async function callGPT(prompt) {
    const response = await openai.createCompletion({
        model: "text-davinci-003",
        prompt: prompt,
        temperature: 0,
        max_tokens: 100,
        conversation_id: "test",
    });
    console.log(response.data);
}

const api2 = new ChatGPTAPI({
    apiKey: "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",
    // completionParams: {
    //     model: 'curie',
    //     temperature: 0.5,
    //     top_p: 0.8
    // }
})

callGPT("What was the last thing i asked you?");