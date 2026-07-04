# ComfyUIRunWorkflow

✨ [日本語](../README.md)

A tool for running ComfyUI workflows from a GUI. A C# WPF port of [comfyui_tools](https://github.com/satoru634/comfyui_tools).

## Features

- Run workflows (specify prompts, LoRA, and image size from the GUI)
- Batch count (1–10, runs the workflow repeatedly via the batch count field next to the run button)
- List and inspect execution results
- Preview generated images (right after execution, in the list, and in the detail dialog — click to enlarge)
- WD14 Tagger image tagging (select an image, get and copy a tag string)
- Tag history display on the Data page (switch between it and results via tabs)
- Theme switching and persisted connection settings

## Quick Start

### Requirements

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (Visual Studio 2022 or later)
- A running [ComfyUI](https://github.com/comfyanonymous/ComfyUI) server

※The following custom nodes, used by the workflow templates bundled with this project (must be installed on the ComfyUI side beforehand)

- [ComfyUI-Impact-Pack](https://github.com/ltdrdata/ComfyUI-Impact-Pack)
- [ComfyUI-Impact-Subpack](https://github.com/ltdrdata/ComfyUI-Impact-Subpack)
- [ComfyUI-WD-Timm-Tagger](https://github.com/bedovyy/ComfyUI-WD-Timm-Tagger)

### Build & Run

```bash
git clone --recursive https://github.com/satoru634/ComfyUIRunWorkflow.git
cd ComfyUIRunWorkflow
dotnet run --project ComfyUIRunWorkflow
```

### Initial Setup

1. Open the **Settings** page
2. Configure the **ComfyUI URL** (default: `http://127.0.0.1:8188`), the **workflow_config.json path**, and the **results folder**

A sample [`workflow_config.json`](../workflow_config.json) is included at the repository root. Edit it to match your environment (e.g. LoRA filenames), then point the **workflow_config.json path** setting at this file.

![Settings page](images/settings_page.png)

### Running Your First Workflow

1. On the **Home** page, choose a workflow, prompts, and image size
2. Click **Run**
3. Check the results and preview images on the **Data** page

![Home page](images/dashboard_page.png)

For detailed, page-by-page usage (LoRA, batch count, WD14 Tagger, tag history tab, etc.), see [doc/usage_english.md](usage_english.md).

## Tech Stack

| Item | Detail |
|---|---|
| Runtime | .NET 8 / WPF |
| UI framework | Wpf.Ui v4.3.0 |
| MVVM | CommunityToolkit.Mvvm v8.4.2 |
| DI | Microsoft.Extensions.Hosting |
| Shared library | ComfyUILibs (submodule) |

## Project Structure

```
ComfyUIRunWorkflow/   ← solution root
  ComfyUILibs/        ← shared library (submodule)
  ComfyUILibsTests/   ← ComfyUILibs tests (156)
  ComfyUIRunWorkflow/ ← WPF GUI project
  ComfyUIRunWorkflowTests/ ← GUI tests (157)
  doc/                ← documentation (usage, English versions, class diagram)
```

## Documentation

- [Usage (detailed)](usage_english.md)
- [Class diagram](class_diagram.md)

## License

See [LICENSE](../LICENSE).
