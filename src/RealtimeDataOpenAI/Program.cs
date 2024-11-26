using Azure.AI.OpenAI;
using OpenAI.Chat;
using RealtimeDataOpenAI.Utils;
using System.ClientModel;
using System.Text;


// SHOW HEADER
ConsoleHelper.ShowHeader();

// GET HOST
string host = ConsoleHelper.SelectFromOptions(
    [Statics.AzureOpenAIKey, Statics.OpenAIKey]);

// CREATE CLIENT
ChatClient? client = null;

switch (host)
{
    case Statics.AzureOpenAIKey:

        string azureEndpoint =
            ConsoleHelper.GetUrl(
                "Please insert your [yellow]Azure OpenAI endpoint[/]:");

        string azureOpenAIKey =
            ConsoleHelper.GetString(
                "Please insert your [yellow]Azure OpenAI[/] API key:");

        string azureDeploymentName =
            ConsoleHelper.GetString(
                "Please insert the [yellow]deployment name[/] of the model:");

        AzureOpenAIClient azureClient = new(
            new Uri(azureEndpoint),
            new ApiKeyCredential(azureOpenAIKey));

        client = azureClient.GetChatClient(azureDeploymentName);

        break;

    case Statics.OpenAIKey:

        string openAIKey =
            ConsoleHelper.GetString(
                "Please insert your [yellow]OpenAI[/] API key:");

        string openAIDeploymentName =
            ConsoleHelper.SelectFromOptions(
                [Statics.GPT4oKey, Statics.GPT4oMiniKey,
                Statics.GPT4TurboKey, Statics.GPT4Key]);

        client = new ChatClient(
            openAIDeploymentName,
            new ApiKeyCredential(openAIKey));

        break;
}

// CHECK CLIENT
if (client is null)
{
    ConsoleHelper.WriteToConsole("[red]Client creation failed.[/]");
    return;
}

// GET WEBSITE
string websiteUrl =
    ConsoleHelper.GetUrl(
        "Please insert the [yellow]URL[/] of the website, you want to chat with:");

// GET WEBSITE CONTENT
var htmlContent =
    await WebsiteHelper.GetHtmlBodyAsync(websiteUrl);

// CREATE LIST OF CHAT MESSAGES
List<ChatMessage> chatMessages = [];

if (!string.IsNullOrEmpty(htmlContent))
{
    chatMessages.Add(
        ChatMessage.CreateSystemMessage(
            ChatMessageContentPart.CreateTextPart(
                $"You are a helpful AI assistant. " +
                $"Please use the following as your additional data source " +
                $"to answer the related questions: {htmlContent}")));
}

// SHOW HEADER
ConsoleHelper.ShowHeader();

while (true)
{
    // GET USER INPUT
    string message =
        ConsoleHelper.GetString("USER: ", false);

    // ADD USER MESSAGE TO CHAT
    chatMessages.Add(
        ChatMessage.CreateUserMessage(
            ChatMessageContentPart.CreateTextPart(message)));

    // WRITE OUTPUT
    ConsoleHelper.WriteToConsole("[green]OUTPUT:[/]");

    // CREATE ASSISTANT MESSAGE STRING BUILDER
    StringBuilder assistantMessageStringBuilder = new();

    // COMPLETE CHAT
    await foreach (StreamingChatCompletionUpdate? chatUpdate
        in client.CompleteChatStreamingAsync(chatMessages))
    {
        foreach (ChatMessageContentPart? contentPart
            in chatUpdate.ContentUpdate)
        {
            ConsoleHelper.WriteToConsole(contentPart.Text, false);
            assistantMessageStringBuilder.Append(contentPart.Text);
        }
    }

    // WRITE NEW LINE
    ConsoleHelper.WriteToConsole(Environment.NewLine);

    // ADD ASSISTANT MESSAGE TO CHAT
    chatMessages.Add(
        new AssistantChatMessage(
            ChatMessageContentPart.CreateTextPart(
                assistantMessageStringBuilder.ToString())));
}