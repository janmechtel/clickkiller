# The Clickkiller BTTN

The BTTN is a giant “HELP” buzzer for whenever software tools are in your way instead of being helpful. 

## How to use it

1. Press the button
2. Take some notes 
3. Get back to work 

## Examples

- “I’m googling my bank account website several times a month, I should research a better way to handle my bookmarks.”
- “Maybe I should add this programm to auto-run on each startup, I wonder how i do that. But I shouldn’t do it right now, I’m in the middle of something urgent”
- “I’m repeating these steps over and over, maybe my colleague can help me automate it.”

## Benefits

- Avoid the distraction - Make a note instead of distracting yourself by acting right now
- Don’t forget about it - allow yourself to prioritze it later
- Get help - from your colleagues, the community or Artificial Intelligence

The BTTN provides a fast way to get the nuisance out of your head and sidesteps the dillema of acting either “now” or “never”.

## Download

- Linux: [Clickkiller.AppImage](https://github.com/janmechtel/clickkiller/releases/latest/download/Clickkiller.AppImage)
- Windows: [Clickkiller-win-stable-Setup.exe](https://github.com/janmechtel/clickkiller/releases/latest/download/Clickkiller-win-stable-Setup.exe)

## FAQ
- Why not use my TODO App? 
    - If that works for you - keep going! Click Killer is dedicated to software problems. It's a fast way to tap into a community and your backlog is not mixed with all your real work.
- Is it safe to use? 
    - Yes, we only share the data on your screen.
- Will it slow down my computer? 
    - No
- I can’t install it?
    - Please get in touch [jmechtel@gmail.com](mailto:jmechtel@gmail.com)
- Why did you launch this project?
    - I love automation and helping people being more productive. 
    - I want my clients & friends to have a faster way to reach out with their problems 💚

# Test

```
dotnet build
dotnet run
bash clickkiller.Linux/build-linux.sh 0.0.xx
bash clickkiller.Windows/build-win.sh 0.0.xx
```

## Publishing a release

Releases are distributed via **GitHub Releases** only (GCS is no longer used).

```bash
# 1. Build the Linux release
bash clickkiller.Linux/build-linux.sh 0.0.xx

# 2. Publish to GitHub Releases (get token from: gh auth token)
vpk upload github --repoUrl https://github.com/janmechtel/clickkiller/ --publish --releaseName "ClickKiller 0.0.xx" --tag v0.0.xx -o releases/ --token <gh-token>
```

The in-app update check (`UpdateManager`) also points to GitHub, so users will be prompted to update automatically.

## Local Linux setup (niri / F1 hotkey)

For desktop integration outside the repo (autostart, F1 toggle, niri bind), see
[setup/README.md](setup/README.md).

Quick version:

```bash
dotnet publish clickkiller.root.csproj -c Release -r linux-x64 --self-contained -o publish
./setup/install.sh
```

# Goals:

1. I use it and I find it useful
2. A friend of mine also finds it useful (regular use + shares problems with me + refers other users)
3. I do more interviews

Who are my ideal users? 
- PowerUsers = demanding
- Less frequent = less demanding, but also they use it less

# Why
> creates a dialog, a stream of annoyances
> it's a basis from where i can offer more things / experiment
> I can sollicit feedack

# Roadmap

- [X] Simple Backend - sqlite
- [X] Frontend - Avalonia
- [X] Installer - Velopack
- [ ] Share issues to a friend
- [ ] Detect the current app