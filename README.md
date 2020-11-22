<h1 align="center">Jellyfin Anime Plugin</h1>
<h3 align="center">Part of the <a href="https://jellyfin.media">Jellyfin Project</a></h3>

<p align="center">
This plugin is built with .NET Core to download metadata for anime.
</p>

## Build Process

1. Clone or download this repository

2. Ensure you have .NET Core SDK setup and installed

3. Build plugin project using either your IDE or the CLI. As an example, the following can be used for a Release build:

```sh
dotnet build --configuration Release
```

On build, the project automatically copies all relevant plugin files to the `plugin` folder in the repository root.

4. Copy the contents of the `plugin` folder (at the repo root) to the `plugins` folder of your Jellyfin installation, under the program data directory or inside the portable install directory

### Copy automation for development

Step 4 can be automated by adding a `Jellyfin.Plugin.Anime.csproj.user` file to the project folder, to copy the published dlls directly to the correct folder for your local Jellyfin server.

```xml
<?xml version="1.0"?> 
<Project>
    <PropertyGroup>
        <!-- NOTE The following property will deploy the plugin to the config folder of your running Jellyfin install, for easier testing. This way you don't have to copy files manually, and a simple server restart is sufficient. -->
        <BinaryPluginOutputFolder>PATH_TO_YOUR_JELLYFIN_CONFIG_FOLDER\plugins</BinaryPluginOutputFolder>-->
    </PropertyGroup>
</Project>
```