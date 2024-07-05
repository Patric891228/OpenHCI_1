# OpenHCI_1

OpenHCI 1's team's Project

# Quick Start

## Connect to OpenAI

### Saving Your Credentials

To make requests to the OpenAI API, you need to use your API key and organization name (if applicable). To avoid exposing your API key in your Unity project, you can save it in your device's local storage.

### To do this, follow these steps:

1. Create a folder called .openai in your home directory (e.g. C:User\UserName\ for Windows or ~\ for Linux or Mac)
2. Create a file called auth.json in the .openai folder
3. Add an api_key field and a organization field (if applicable) to the auth.json file and save it
   Here is an example of what your auth.json file should look like:

   ```
   {
       "api_key": "sk-...W6yi",
       "organization": "org-...L7W"
   }
   ```
