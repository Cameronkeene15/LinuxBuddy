# Linux Buddy
Wish you had a buddy to help with Linux commands? Linux Buddy assists with common Linux tasks, can generate bash commands, and accepts piped input for context-aware help. It connects to your local Ollama server, so you can use the LLM of your choice.

## Features
- Connects to your Ollama server so you can run whichever LLM you prefer
- Get help with common Linux commands
- Pipe command results into Linux Buddy for context-aware assistance
- User-friendly command-line interface
- Lightweight and easy to install
- Supports “thinking” models with a spinner that hides think-tags

## Requirements
- Ollama installed and running (e.g., `ollama serve`)
- A local model pulled in Ollama (e.g., `ollama pull deepseek-r1:1.5b` or any other model you prefer)
- .NET SDK targeting .NET 10 (run `dotnet --info` to verify)

## Quick Start
1. Clone the repository:
```bash
    git clone https://github.com/Cameronkeene15/LinuxBuddy.git cd LinuxBuddy
```

2. Configure Ollama URL and default model:
- Edit `LinuxBuddy/Services/SettingsService.cs`
  - `_ollamaUrl` defaults to `http://192.168.25.26:11434` — change to your Ollama server (often `http://127.0.0.1:11434`)
  - `_defaultModel` defaults to `deepseek-r1:1.5b` — change if you want a different default
- Alternatively, you can set the model at runtime via the `model` verb (see Usage).

3. Ensure your model exists in Ollama:
```bash
    ollama pull deepseek-r1:1.5b
```
or a model of your choosing, e.g.
```bash
    ollama pull llama3.1
```

4. Build and publish (Linux x64, single-file):
```bash
    dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained=false
```

Resulting binary (example):
```
    LinuxBuddy/bin/Release/net10.0/linux-x64/publish/LinuxBuddy
```

5. Install to your PATH:
```bach
    mkdir -p ~/.local/bin
    cp LinuxBuddy/bin/Release/net10.0/linux-x64/publish/LinuxBuddy ~/.local/bin/linuxbuddy
    chmod +x ~/.local/bin/linuxbuddy
    echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc source ~/.bashrc
```

6. Optional: add a short alias:
```bash
    echo "alias lb='linuxbuddy'" >> ~/.bashrc source ~/.bashrc
```

7. Try it:
```bash
    linuxbuddy --help 
```
```bash
    linuxbuddy general "Hello, Linux Buddy!"
```

## Usage
Linux Buddy supports three verbs: `bash`, `general`, and `model`. The `-v|--verbose` flag prints extra details and streams all output (including think segments).

- Ask for a bash command:

```bash
    linuxbuddy bash "Find files larger than 100MB in the current directory"
```

- With verbose output:
```bash
    linuxbuddy bash -v "Show top 5 processes by memory usage"
```

- Ask a general question:
```bash
    linuxbuddy general "What does the /etc/fstab file do?"
```

- Provide context via piped input:
```bash
    history | linuxbuddy general "Based on my bash history, what does it look like I was doing?"
```

- Set the model (persisted per user):
```bash
    linuxbuddy model "deepseek-r1:1.5b"
```

## How it Works
- Connects to your Ollama server and streams responses via Semantic Kernel.
- For thinking models (like DeepSeek R1), Linux Buddy shows a spinner and suppresses content between `<think>...</think>` in non-verbose mode.
- With `-v`, all content is streamed directly to the console, helpful for debugging prompts and context.

## Settings & Persistence
- Ollama URL and default model are defined in:
  - `LinuxBuddy/Services/SettingsService.cs`
- The currently selected model is stored in a per-user settings file:
  - Linux: `~/.config/.linuxBuddySettings`
- Change the model at runtime with:
  - `linuxbuddy model "<model-name>"`

## Troubleshooting
- Connection refused / timeouts:
  - Make sure `ollama serve` is running and the URL in `SettingsService` points to it (commonly `http://127.0.0.1:11434`).
- Model not found:
  - Run `ollama pull <model>` for the model you want to use.
- Spinner never stops:
  - Some models may emit think tags unexpectedly; try `-v` to see raw output.
- No output:
  - Try `-v` to see full streaming output and any errors.

## Acknowledgments
- [Ollama] for local model serving
- [Microsoft Semantic Kernel] for chat completion integration

## Contributing
Feel free to contribute or open issues on GitHub!

## License
Licensed under the Apache License 2.0. See [LICENSE](LICENSE) for details.

---
Built by Cameron Keene with ❤️