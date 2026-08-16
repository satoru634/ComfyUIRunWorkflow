# Generate Page (Bulk-Creating Many Jobs at Once)

✨ [日本語](generate.md)

Use this page when you want to create a large number of similar images that only differ by, say, the character or outfit.
Instead of registering each job by hand on the [Queue page](queue_english.md), this page creates them all for you at once.

![Generate page](../images/generate_page_en.png)

Unlike the other pages, this one requires preparing a few plain text files beforehand. It sounds like more work, but you can get started quickly by copying and editing the sample files that come with the app.

## Preparing Files (Using the Sample Files)

There's a folder called `sample_jobs` in the same location as the app. Copy the files inside it and edit them to create what you need.

| Folder | What it's for |
|---|---|
| `sample_jobs/base_prompts/` | One file per scene/composition you want to generate. Placeholders like `<character>` (text wrapped in `< >`) get replaced later with actual text |
| `sample_jobs/replace_list/` | Defines what each placeholder like `<character>` should actually be replaced with |
| `sample_jobs/job_templates/` | Settings shared by every job — art style, image size, and so on |

### Steps

1. Copy the whole `sample_jobs` folder and give the copy a descriptive name (e.g. `my_jobs`).
2. Open the files inside the `base_prompts` folder with Notepad or a similar text editor. The text after `"positive"` is what you want to see in the image. You can edit it, or copy the file to add more (each file becomes one job). Leave any `<character>`-style placeholders exactly as they are.
3. Open the file inside `replace_list` and set what each placeholder should be replaced with (e.g. `<character>` → `black hair, school uniform`).
4. Open the file inside `job_templates` and set the shared art style, image size, and so on.
5. Go back to the Generate page and use the "Browse" buttons to specify these four items:
   - **Base Prompt Directory**: the folder from step 2
   - **Replacement List File**: the file from step 3
   - **Job Template File**: the file from step 4
   - **Output File**: where to save the generated job list (choose a new file name)
6. Click **Generate**. The log area below shows progress as it runs.
7. Once you see a message saying the output file was written successfully, you're done.

## Using the Generated Jobs on the Queue Page

1. Open the [Queue page](queue_english.md).
2. Click **Import List** at the top and select the output file from step 5.
3. The generated jobs appear in the list. Click **Run All** on the Queue page to start generating them.

## If Something Goes Wrong

- An error saying a keyword is missing from the replacement list means a placeholder like `<...>` in one of your `base_prompts` files isn't defined in the `replace_list` file. Make sure the `< >` placeholders match exactly between the two.
- If an error occurs, no output file is created at all — you'll never end up with a half-finished job list.
