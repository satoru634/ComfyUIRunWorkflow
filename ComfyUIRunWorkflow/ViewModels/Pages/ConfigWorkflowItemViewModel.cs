using ComfyUILibs.Models;
using System.Collections.ObjectModel;

namespace ComfyUIRunWorkflow.ViewModels.Pages
{
    /// <summary>
    /// ConfigEditorPage で編集するワークフロー設定1件分（workflow_config.json の workflows[name]）。
    /// </summary>
    public partial class ConfigWorkflowItemViewModel : ObservableObject
    {
        /// <summary>ワークフロー名（workflows の辞書キー）。</summary>
        [ObservableProperty]
        private string _name = "";

        /// <summary>
        /// ワークフロー一覧でインライン編集（名前テキストのダブルクリック編集）中かどうか。
        /// セッション限りの UI 状態であり、保存対象には含めない。
        /// </summary>
        [ObservableProperty]
        private bool _isEditingName = false;

        /// <summary>default_image_size の幅。</summary>
        [ObservableProperty]
        private int _defaultWidth = 1024;

        /// <summary>default_image_size の高さ。</summary>
        [ObservableProperty]
        private int _defaultHeight = 1024;

        /// <summary>image_size.vertical の幅。</summary>
        [ObservableProperty]
        private int _verticalWidth = 1024;

        /// <summary>image_size.vertical の高さ。</summary>
        [ObservableProperty]
        private int _verticalHeight = 1024;

        /// <summary>image_size.horizontal の幅。</summary>
        [ObservableProperty]
        private int _horizontalWidth = 1024;

        /// <summary>image_size.horizontal の高さ。</summary>
        [ObservableProperty]
        private int _horizontalHeight = 1024;

        /// <summary>image_size.square の幅。</summary>
        [ObservableProperty]
        private int _squareWidth = 1024;

        /// <summary>image_size.square の高さ。</summary>
        [ObservableProperty]
        private int _squareHeight = 1024;

        /// <summary>
        /// 画面上で現在編集対象としている画像サイズ種別（"default"/"vertical"/"horizontal"/"square"）。
        /// <see cref="Width"/>/<see cref="Height"/> はこの値に応じて4種の幅・高さのいずれかへ委譲する。
        /// </summary>
        [ObservableProperty]
        private string _selectedSizeKind = "default";

        /// <summary>
        /// <see cref="SelectedSizeKind"/> が指す画像サイズ種別の幅。NumberBox 1つで4種の幅を切り替え編集するための委譲プロパティ。
        /// </summary>
        public int Width
        {
            get => SelectedSizeKind switch
            {
                "vertical" => VerticalWidth,
                "horizontal" => HorizontalWidth,
                "square" => SquareWidth,
                _ => DefaultWidth,
            };
            set
            {
                switch (SelectedSizeKind)
                {
                    case "vertical": VerticalWidth = value; break;
                    case "horizontal": HorizontalWidth = value; break;
                    case "square": SquareWidth = value; break;
                    default: DefaultWidth = value; break;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <see cref="SelectedSizeKind"/> が指す画像サイズ種別の高さ。NumberBox 1つで4種の高さを切り替え編集するための委譲プロパティ。
        /// </summary>
        public int Height
        {
            get => SelectedSizeKind switch
            {
                "vertical" => VerticalHeight,
                "horizontal" => HorizontalHeight,
                "square" => SquareHeight,
                _ => DefaultHeight,
            };
            set
            {
                switch (SelectedSizeKind)
                {
                    case "vertical": VerticalHeight = value; break;
                    case "horizontal": HorizontalHeight = value; break;
                    case "square": SquareHeight = value; break;
                    default: DefaultHeight = value; break;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>SelectedSizeKind が変わったとき、委譲プロパティ Width/Height を再表示させる。</summary>
        partial void OnSelectedSizeKindChanged(string value)
        {
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
        }

        /// <summary>このワークフローの loras 一覧。</summary>
        public ObservableCollection<ConfigLoraItemViewModel> Loras { get; } = new();

        /// <summary>
        /// インライン編集を開始した時点の名前。無効な名前（空文字・重複）で確定しようとした場合、
        /// この値へ復元する。
        /// </summary>
        internal string NameBeforeEdit { get; set; } = "";

        /// <summary>LoRA エントリを1件追加する。</summary>
        [RelayCommand]
        private void AddLora() => Loras.Add(new ConfigLoraItemViewModel());

        /// <summary>指定した LoRA エントリを削除する。</summary>
        [RelayCommand]
        private void RemoveLora(ConfigLoraItemViewModel item) => Loras.Remove(item);

        /// <summary>LoRA 一覧を論理名（Name）の昇順に並び替える。</summary>
        [RelayCommand]
        private void SortLorasAscending() => SortLoras(ascending: true);

        /// <summary>LoRA 一覧を論理名（Name）の降順に並び替える。</summary>
        [RelayCommand]
        private void SortLorasDescending() => SortLoras(ascending: false);

        /// <summary>Loras を Name（大文字小文字を区別しない）で並び替え、既存のコレクションを差し替えずに順序のみ更新する。</summary>
        private void SortLoras(bool ascending)
        {
            var sorted = ascending
                ? Loras.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase).ToList()
                : Loras.OrderByDescending(l => l.Name, StringComparer.OrdinalIgnoreCase).ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                var currentIndex = Loras.IndexOf(sorted[i]);
                if (currentIndex != i)
                    Loras.Move(currentIndex, i);
            }
        }

        /// <summary>workflow_config.json から読み込んだ <see cref="WorkflowSettings"/> を編集用データに変換する。</summary>
        public static ConfigWorkflowItemViewModel FromSettings(string name, WorkflowSettings settings)
        {
            var item = new ConfigWorkflowItemViewModel { Name = name };

            if (settings.DefaultImageSize != null)
            {
                item.DefaultWidth = settings.DefaultImageSize.Width;
                item.DefaultHeight = settings.DefaultImageSize.Height;
            }

            if (settings.ImageSize != null)
            {
                if (settings.ImageSize.TryGetValue("vertical", out var vertical))
                {
                    item.VerticalWidth = vertical.Width;
                    item.VerticalHeight = vertical.Height;
                }
                if (settings.ImageSize.TryGetValue("horizontal", out var horizontal))
                {
                    item.HorizontalWidth = horizontal.Width;
                    item.HorizontalHeight = horizontal.Height;
                }
                if (settings.ImageSize.TryGetValue("square", out var square))
                {
                    item.SquareWidth = square.Width;
                    item.SquareHeight = square.Height;
                }
            }

            if (settings.Loras != null)
            {
                foreach (var (loraName, entry) in settings.Loras)
                {
                    item.Loras.Add(new ConfigLoraItemViewModel
                    {
                        Name = loraName,
                        File = entry.File ?? "",
                        Strength = entry.Strength ?? 0.8,
                    });
                }
            }

            return item;
        }

        /// <summary>新規ワークフロー追加時の初期値（画像サイズ全方向 1024×1024、loras 空）を持つインスタンスを作成する。</summary>
        public static ConfigWorkflowItemViewModel CreateDefault(string name) => new() { Name = name };

        /// <summary>編集内容を保存用の <see cref="WorkflowSettings"/> に変換する。</summary>
        public WorkflowSettings ToSettings() => new()
        {
            DefaultImageSize = new ImageSize { Width = DefaultWidth, Height = DefaultHeight },
            ImageSize = new Dictionary<string, ImageSize>
            {
                ["vertical"] = new ImageSize { Width = VerticalWidth, Height = VerticalHeight },
                ["horizontal"] = new ImageSize { Width = HorizontalWidth, Height = HorizontalHeight },
                ["square"] = new ImageSize { Width = SquareWidth, Height = SquareHeight },
            },
            Loras = Loras.ToDictionary(l => l.Name, l => new LoraEntry { File = l.File, Strength = l.Strength }),
        };
    }
}
