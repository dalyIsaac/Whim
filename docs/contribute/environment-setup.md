# Environment Setup

1. Clone this repo
2. Install tools for the Windows App SDK:
   1. Go to the [Install tools for the Windows App SDK](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment) page
   2. Follow the instructions for [winget for C# developers](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b#for-c-developers), or [installing Visual Studio](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment?tabs=cs-vs-community%2Ccpp-vs-community%2Cvs-2022-17-1-a%2Cvs-2022-17-1-b#install-visual-studio)
3. Ensure that the dotnet version specified in the [global.json](https://github.com/dalyIsaac/Whim/blob/main/global.json) file is installed - if it isn't, install it from the [dotnet download page](https://dotnet.microsoft.com/en-us/download).

After cloning, make sure to run the following in a shell in the root Whim directory:

```shell
git config core.autocrlf true
```

If you've already made changes with `core.autocrlf` set to `false`, you can fix the line endings with:

```shell
git add . --renormalize
```

Before making a pull request, please install the tools specified in [`.config/dotnet-tools.json`](https://github.com/dalyIsaac/Whim/blob/main/.config/dotnet-tools.json), and run the formatters:

```shell
# In the repo root, install the tools
cd Whim
dotnet tool restore

# Run the formatters
dotnet tool run csharpier format .
dotnet tool run xstyler --recursive --d . --config ./.xamlstylerrc
```
