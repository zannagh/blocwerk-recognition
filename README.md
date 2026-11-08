# blocwerk-recognition

Hold recognition for [Blocwerk](https://github.com/zannagh/blocwerk): finding climbing holds on a wall photo, tracing their outlines, reading the ArUco markers on the wall and matching holds across photos. Split out of the Blocwerk repository with its history.

What lives here:

| Path | What it is |
| --- | --- |
| `src/Blocwerk.Recognition.Abstractions` | The contracts: detection, outline, marker and matching interfaces, their result records, the settings, the recognition gate and the pure outline geometry. No OpenCV, no ONNX. **NuGet package.** |
| `src/Blocwerk.HoldDetection` | The implementations: YOLO hold detection over ONNX (tiled), OpenCV outlines and the MobileSAM segmenter, ArUco markers, hold presence, cross-photo hold and texture matching. **NuGet package.** |
| `src/Blocwerk.Recognition.Worker` | The recognition worker: a runner that pulls recognition jobs from a Blocwerk instance over the runner protocol and answers them with the two libraries above. Shipped as the image `ghcr.io/zannagh/blocwerk-recognition`. |
| `test/` | Unit tests of the libraries and the worker. |
| `docker/recognition/Dockerfile` | The worker image, multi-arch (linux/amd64, linux/arm64). |
| `tools/segmenter-export/export_segmenter.py` | Exports MobileSAM to the two ONNX files the segmenter loads. |

## Model files are not in git

The models are large binaries and are gitignored (`*.onnx`). Nothing in this repository, the packages or the image contains them:

* `climbingcrux.onnx`, the hold detector, from [climbingcrux_model](https://github.com/mkurc1/climbingcrux_model).
* `hold-segmenter-encoder.onnx` and `hold-segmenter-decoder.onnx`, the box-prompted segmenter. Make them with `tools/segmenter-export/export_segmenter.py` (its header lists the steps). Without them, outlines are traced by colour.

For local builds and tests, put them in `src/Blocwerk.HoldDetection/models/`; the build copies them next to the binaries. Tests that need a model skip when it is missing.

## Building and testing

Needs the .NET 10 SDK.

```bash
dotnet build Blocwerk.Recognition.slnx
dotnet test Blocwerk.Recognition.slnx
```

OpenCV's native library ships for Linux only (the 4.13 runtimes the package depends on), so the tests that need it are marked `[OpenCvFact]`/`[OpenCvTheory]` and skip on macOS. CI runs everything on Linux. A Mac build still compiles against the older osx_arm64 line (4.8.1), but its native library does not load reliably.

**Local development on a Mac** runs recognition through the worker container, the arm64 image: start Blocwerk with `RECOGNITION__MODE=queue` and an instance key, then the worker as below with `--platform linux/arm64` and the same `RECOGNITION__INSTANCEKEY`. Blocwerk's README has the two commands.

## Running the worker locally

The worker connects to a running Blocwerk instance; it has no inbound API besides `GET /health` on port 8080.

```bash
docker run --rm \
  -e RECOGNITION__SERVER=http://host.docker.internal:5050 \
  -e RECOGNITION__INSTANCEKEY=<the instance's key> \
  -e JOB_MAX_MEMORY_MB=2560 \
  -v /path/to/models:/models:ro \
  ghcr.io/zannagh/blocwerk-recognition:latest
```

Or straight from source, on Linux:

```bash
RECOGNITION__SERVER=http://localhost:5050 \
RECOGNITION__INSTANCEKEYFILE=/path/to/blocwerk/docker/recognition-key/instance.key \
RECOGNITION__MODELSDIR=/path/to/models \
dotnet run --project src/Blocwerk.Recognition.Worker
```

Settings (environment, or `Recognition:*` in configuration):

| Variable | Meaning |
| --- | --- |
| `RECOGNITION__SERVER` | The Blocwerk base URL (default `http://blocwerk:5050`). |
| `RECOGNITION__INSTANCEKEY` | The instance runner key. |
| `RECOGNITION__INSTANCEKEYFILE` | Or the file the app generates that key into; the worker waits until it exists. |
| `RECOGNITION__RUNNERKEY` | Or a `bwr_` key, for a runner on another machine. |
| `RECOGNITION__MODELSDIR` | The read-only models folder (default `/models`). |
| `RECOGNITION__MODELCACHEDIR` | Where models fetched by hash are cached. |
| `JOB_MAX_MEMORY_MB` | Above it a job fails as `resource`; 0 or unset turns the watchdog off. |

Build the image yourself from the repository root:

```bash
docker buildx build -f docker/recognition/Dockerfile --platform linux/amd64,linux/arm64 .
```

## How Blocwerk consumes it

* **Packages.** Blocwerk references `Blocwerk.Recognition.Abstractions` and `Blocwerk.HoldDetection` from GitHub Packages (`https://nuget.pkg.github.com/zannagh/index.json`), with the versions pinned in its `Directory.Packages.props`. The app uses them for the recognition it still runs in-process and for the shared contracts of the job queue.
* **Image.** Blocwerk's compose files run `ghcr.io/zannagh/blocwerk-recognition` as the `recognition` service, next to the app.

### Consuming the packages

`Blocwerk.HoldDetection` depends on `OpenCvSharp4` with both Linux native runtimes (`OpenCvSharp4.runtime.linux-arm64`, `OpenCvSharp4.official.runtime.linux-x64`) and on the CPU ONNX runtime (`Microsoft.ML.OnnxRuntime`). A RID-specific restore and publish (`-a $TARGETARCH`, `-r linux-arm64`) copies only the matching native library; a portable build carries both.

YoloDotNet, which the package uses for detection, depends on the GPU ONNX runtime, which has no arm64 build and would put a second `libonnxruntime` next to the CPU one. Exclude it in the project that references `Blocwerk.HoldDetection`:

```xml
<PackageReference Include="Microsoft.ML.OnnxRuntime.Gpu" ExcludeAssets="all" PrivateAssets="all" />
<PackageReference Include="Microsoft.ML.OnnxRuntime.Gpu.Windows" ExcludeAssets="all" PrivateAssets="all" />
<PackageReference Include="Microsoft.ML.OnnxRuntime.Gpu.Linux" ExcludeAssets="all" PrivateAssets="all" />
```

GitHub Packages needs authentication even for reads: a `nuget.config` that names the source, and credentials from the environment (a token with `read:packages`), never committed.

## Versioning and releases

Versions come from [GitVersion](https://gitversion.net/) (`GitVersion.yml`). Publishing a GitHub release runs `.github/workflows/release.yml`: tests, packs both packages and pushes them to GitHub Packages, and builds and pushes the multi-arch image as `ghcr.io/zannagh/blocwerk-recognition:<version>` and `:latest`. `.github/workflows/ci.yml` builds, tests and packs every pull request and push to `main`.

## License

AGPL-3.0-or-later, like Blocwerk. See [LICENSE](LICENSE).
