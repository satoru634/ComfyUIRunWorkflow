# Usage

✨ [日本語](usage.md)

A detailed guide to each page of ComfyUIRunWorkflow.
For setup instructions, see the [Quick Start](README_english.md) section of the English README.

## Table of Contents

- [Settings Page](#settings-page)
- [Home Page (Running Workflows)](#home-page-running-workflows)
- [Data Page (Results / Tag History)](#data-page-results--tag-history)
- [Tagger Page (WD14 Tagger)](#tagger-page-wd14-tagger)

---

## Settings Page

![Settings page](images/settings_page.png)

Open this page first after launching the app and configure the following.

| Item | Description |
|---|---|
| ComfyUI URL | The ComfyUI server URL (default: `http://127.0.0.1:8188`) |
| workflow_config.json path | The JSON file defining workflows, LoRA, and WD14 Tagger settings |
| Results folder | Where execution results (`result_*.json`), tag history (`tag_result_*.json`), and the preview image cache (`preview_cache/`) are stored |
| Theme | Switch between light and dark |
| Language | Switch the display language between Japanese and English (default: Japanese; applies immediately, no restart required) |

Settings persist across app restarts.

---

## Home Page (Running Workflows)

![Home page](images/dashboard_page.png)

### Steps

1. Select a workflow (`sdxl`, `anima`, `anima_rapid`, etc. — as defined under `workflows` in `workflow_config.json`)
2. Enter the positive and negative prompts
3. Choose an image size — a preset (vertical / horizontal / square) or a custom size
4. Add LoRAs (optional, up to 4)
5. Set the **batch count** if needed (1–10, default 1)
6. Click **Run**

### Batch Count

Setting the batch count to 2 or more runs the same content (only the seed is auto-incremented) that many times in sequence.

- Progress ("Running N/M") is shown below the progress bar while running
- All output files are combined and saved as a single execution result
- If a ComfyUI error occurs partway through, execution stops at that point and the outputs succeeded so far are saved as a result with an error

### Result Preview

Once execution finishes, thumbnails of the generated images appear in the right panel. Click a thumbnail to view it at full size.

---

## Data Page (Results / Tag History)

![Data page](images/data_page.png)

Switch views using the "Results" / "Tag History" tabs at the top of the page.

### Results Tab

- Lists execution history (`{results folder}/result_*.json`) newest first, with thumbnails
- Clicking a row opens a detail dialog showing thumbnails of all output files
- Click a thumbnail to view it at full size

Thumbnails are fetched via ComfyUI's `GET /view` API and cached under `{results folder}/preview_cache/` (the same image is not re-fetched from the server after the first time).

### Tag History Tab

- Lists `{results folder}/tag_result_*.json` newest first
- Each card shows only the input filename, timestamp, full tag text, and a copy button (no thumbnail or detail dialog — everything is self-contained in the card)

### Refresh

The **Refresh** button reloads both tabs.

---

## Tagger Page (WD14 Tagger)

![Tagger page](images/tagger_page.png)

A dedicated page for selecting a single image, running the WD14 Tagger workflow, and getting/copying the resulting tag string.

### Steps

1. Select an image via the "Select Image" button or by dragging and dropping it — a preview appears
2. Click **Run Tagging**
3. The tags (comma-separated) appear in the right panel; click **Copy** to copy them to the clipboard

### Model and Thresholds

The model name and thresholds (general/character threshold) come from the `wd14_tagger` section of `workflow_config.json` and cannot be changed from the page. To change them, check the `workflow_config.json` path on the Settings page and edit the file directly.

### Where Results Are Saved

Tagging results are saved to `{results folder}/tag_result_{timestamp}.json` (managed separately from workflow execution results `result_*.json`, and shown in the "Tag History" tab on the Data page).
