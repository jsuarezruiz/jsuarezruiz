# Portfolio

My portfolio is a 3D room. On the desk there is a laptop running Suárez XP, a pixel-art desktop
I wrote in [Avalonia](https://avaloniaui.net) and compiled to WebAssembly, and a phone that opens
the live demos of my Avalonia libraries.

Click around. Most things in the room do something.

## Run it

You need the .NET SDK listed in `global.json` and the WebAssembly tools:

```bash
dotnet workload install wasm-tools
```

```bash
dotnet run --project Portfolio.Browser
```

Then open http://localhost:5235.

To run the desktop on its own, without the room:

```bash
dotnet run --project Portfolio.Desktop
```

## How it is organised

- `Portfolio` has the desktop, the phone and the game. Both heads share it.
- `Portfolio.Browser` is the WebAssembly head. The room is in `wwwroot`, written with Three.js.
- `Portfolio.Desktop` is the desktop head.
- `tools/UpdateContent.cs` refreshes the project list and stars from GitHub.

The room and the desktop talk to each other through `window.pixos`; see `wwwroot/main.js` and
`Interop.cs`.

## Update the projects

```bash
dotnet run tools/UpdateContent.cs
```

It rewrites `content.json`. Set `GITHUB_TOKEN` if you hit the API rate limit.

## Publish

```bash
dotnet publish Portfolio.Browser -c Release -o publish
```

`publish/wwwroot` is a static site. It works from any folder, so it can go straight to GitHub Pages.

## License

[MIT](LICENSE)
