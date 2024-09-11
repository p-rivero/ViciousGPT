# ViciousGPT

Play Baldur's Gate or any other DnD game with an infinite number of Vicious Mockery voice lines!

## Usage

Keep this program running while you play. The window can be minimized.  
Whenever you want your character to come up with a Vicious Mockery line, press `Ctrl+Enter` and talk to them, explaining the situation you're in and which enemy you are attacking. Then, press `Ctrl+Enter` again to confirm, or `Ctrl+Alt+Enter` to cancel.  
Wait a few seconds for the response to be generated.

> For controller users: Press down both analog sticks to talk/confirm, and press both triggers to cancel.

## Set up

### OpenAI

The audio transcription (Speech-to-Text) is done using OpenAI's WhisperV1 model. The text generation of the Vicious Mockery lines is done using GPT-4o Mini.

You will need to set up an OpenAI developer account and get an API key. Then, when you run the program, you will need to input this key.

### Google Cloud

This project uses Google Cloud's [Text-to-Speech](https://cloud.google.com/text-to-speech) API. In order to use it, follow these steps:

1. Go to the [Google Cloud Console](https://console.cloud.google.com/).
1. Create a new project.
1. Go to the API Library and enable the Text-to-Speech API.
1. Install the [Google Cloud CLI](https://cloud.google.com/sdk/docs/install).
1. Open a terminal and run `gcloud init`. Follow the instructions to authenticate.
1. Run `gcloud auth application-default login` and follow the instructions to authenticate.

After those steps, launch the program again and check that the Google Cloud credentials show up as "Found".
