# DVLib

DVLib is a personal .NET 6 class library that collects utilities and experimental code for math, image processing, physics, optics, machine learning, data parsing, and small simulation/game helpers.

This repository is being prepared for open-source release. Some APIs are still experimental and may change as the library is cleaned up.

## Features

- Math primitives and data structures, including complex numbers, vectors, matrices, maps, cubes, interpolation helpers, and numerical integration.
- Image processing helpers for bitmap data, filters, histograms, color conversion, Haar features, face scanning experiments, and rendering utilities.
- Physics and optics experiments, including ray tracing objects, simple physics worlds, optical surfaces, and light-field helpers.
- Machine learning utilities, including data vectors, KNN-related structures, decision stumps, and AdaBoost helpers.
- Lab data and expression/script helpers for parsing, managing, and evaluating math-like objects.
- Stream, socket, XML, and file helper utilities used by the library's experimental systems.

## Project Structure

```text
DVLib.sln
DVLib/
  DVLib.csproj
  MathStuff/          Math primitives, maps, matrices, integration, geometry
  LabDataHelper/      Data sets, math object parsing, DVScript helpers
  NewPhysics/         Physics objects, rendering, ray tracing experiments
  DVOSDOTNET/         Stream, map, user, and interface helpers
  ZeminHelper/        ZBF file and spot helper utilities
  Games/              Small image/game-related helper classes
```

The main namespaces currently include `MathBase`, `Images`, `DVOSLib`, `MachineLearning`, `NewPhysics`, `Physics`, `Optic3D`, and `DVLib.LabDataHelper`.

## Requirements

- .NET SDK 6.0 or later
- Windows, Linux, or macOS with a compatible .NET SDK

The project targets `net6.0` and enables unsafe code blocks.

## Build

Clone the repository and build the solution:

```bash
git clone https://github.com/davidlinc/DVLib.git
cd DVLib
dotnet build DVLib.sln
```

For a release build:

```bash
dotnet build DVLib.sln --configuration Release
```

## Usage

Reference `DVLib/DVLib.csproj` from another .NET project:

```bash
dotnet add reference path/to/DVLib/DVLib/DVLib.csproj
```

Then import the namespace that contains the APIs you need:

```csharp
using MathBase;
using Images;
using MachineLearning;
```

Example:

```csharp
var a = new Vector2(1, 2);
var b = new Vector2(3, 4);
var c = a + b;
```

## Development Notes

- This is an experimental library, so public APIs are not yet guaranteed to be stable.
- The current solution builds successfully, but there are existing compiler warnings that should be reviewed as part of the open-source cleanup.
- There is no NuGet package published yet. Use a project reference until packaging is added.
- Consider adding tests, API documentation, and a license before announcing the project broadly.

## Contributing

Issues and pull requests are welcome. For larger changes, please open an issue first so the design and scope can be discussed.

## License

No license file has been added yet. Until a license is chosen and committed, usage rights are not explicitly granted.
