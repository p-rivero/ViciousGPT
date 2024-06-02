# ViciousGPT

Play Baldur's Gate or any other DnD game with an infinite number of Vicious Mockery voice lines!

## Set up

### Google Cloud

This project uses Google Cloud's [Speech-to-Text](https://cloud.google.com/speech-to-text) and [Text-to-Speech](https://cloud.google.com/text-to-speech) APIs.
You will need to set up a Google Cloud project and enable these APIs.

Once you have done that, go to Credentials and create a new service account. Give it the following roles:
- Cloud Speech Client
- Cloud Speech-to-Text Service Agent

Edit the service account and go to the Keys tab. Create a new json key and save it as `google-credentials.json` in the `credentials` folder located in the root of this project.
