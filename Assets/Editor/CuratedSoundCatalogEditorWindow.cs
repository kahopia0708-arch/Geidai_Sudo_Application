using System.Collections.Generic;
using Geidai.Common.Library;
using UnityEditor;
using UnityEngine;

namespace Geidai.EditorTools
{
    /// <summary>
    /// 制作側音の登録ウィンドウ（US-LIB-04 / IMGUI）。
    /// コンテンツ担当向けのため、ラベルは通常の日本語表記。
    /// </summary>
    public class CuratedSoundCatalogEditorWindow : EditorWindow
    {
        private CuratedSoundCatalog _catalog;
        private TimbreTagCatalog _timbre;
        private Vector2 _listScroll;
        private Vector2 _formScroll;
        private string _selectedId;
        private bool _isNew;
        private string _status = string.Empty;

        private string _id = string.Empty;
        private int _encyclopediaNumber = 1;
        private string _displayName = string.Empty;
        private string _reading = string.Empty;
        private string _description = string.Empty;
        private string _timbreTagId = "bell";
        private int _basePitchMidi = CuratedSoundDefinition.UnsetPitchMidi;
        private LoudnessBand _loudness = LoudnessBand.None;
        private DurationBand _duration = DurationBand.None;
        private string _pairKey = string.Empty;
        private bool _allowPitchShift = true;
        private string _difficultyTagsCsv = string.Empty;
        private string _category = string.Empty;
        private bool _initiallyUnlocked;
        private AudioClip _clip;
        private Sprite _image;
        private string _wavPath = string.Empty;

        private string _tagId = string.Empty;
        private string _tagDisplayName = string.Empty;
        private int _tagSortOrder;
        private string _tagReplaceId;

        private const string DefaultStatusHelp =
            "試聴は中央の「試聴」ボタン、または Project で AudioClip を選び Inspector の再生でも聞けます。必須項目（*）を埋めて「保存」してください。";

        [MenuItem("Geidai/Library/Curated Sound Catalog")]
        public static void Open()
        {
            var window = GetWindow<CuratedSoundCatalogEditorWindow>("音図鑑 登録");
            window.minSize = new Vector2(880f, 560f);
            window.LoadDefaults();
        }

        private void OnEnable() => LoadDefaults();

        private void OnDisable() => EditorAudioPreview.Stop();

        private void LoadDefaults()
        {
            if (_catalog == null)
                _catalog = AssetDatabase.LoadAssetAtPath<CuratedSoundCatalog>(
                    CuratedSoundCatalogEditorOps.DefaultCatalogPath);
            if (_timbre == null)
                _timbre = AssetDatabase.LoadAssetAtPath<TimbreTagCatalog>(
                    CuratedSoundCatalogEditorOps.DefaultTimbrePath);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _catalog = (CuratedSoundCatalog)EditorGUILayout.ObjectField(
                "カタログ", _catalog, typeof(CuratedSoundCatalog), false);
            _timbre = (TimbreTagCatalog)EditorGUILayout.ObjectField(
                "音色語彙", _timbre, typeof(TimbreTagCatalog), false);
            EditorGUILayout.EndHorizontal();

            // 常時同じ高さのステータス欄（空のときヘルプを表示し、出現／消失でレイアウトが跳ねないようにする）
            string statusText = string.IsNullOrEmpty(_status) ? DefaultStatusHelp : _status;
            var statusType = string.IsNullOrEmpty(_status) ? MessageType.None : MessageType.Info;
            EditorGUILayout.HelpBox(statusText, statusType);

            EditorGUILayout.BeginHorizontal();
            DrawSoundList();
            DrawSoundForm();
            DrawTimbrePanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSoundList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(220f));
            EditorGUILayout.LabelField("登録音", EditorStyles.boldLabel);
            if (GUILayout.Button("追加"))
            {
                _isNew = true;
                _selectedId = null;
                ClearForm();
                _status = "新規音を追加します";
            }

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            if (_catalog != null && _catalog.Items != null)
            {
                for (int i = 0; i < _catalog.Items.Count; i++)
                {
                    var item = _catalog.Items[i];
                    if (item == null) continue;
                    string label = $"#{item.encyclopediaNumber} {item.displayName} ({item.id})";
                    if (GUILayout.Button(label, _selectedId == item.id ? EditorStyles.toolbarButton : EditorStyles.miniButton))
                    {
                        _isNew = false;
                        _selectedId = item.id;
                        LoadForm(item);
                        _status = string.Empty;
                        EditorAudioPreview.Stop();
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSoundForm()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(_isNew ? "新規" : "編集", EditorStyles.boldLabel);
            _formScroll = EditorGUILayout.BeginScrollView(_formScroll);

            _id = EditorGUILayout.TextField("id *", _id);
            _encyclopediaNumber = EditorGUILayout.IntField("図鑑ナンバー *", _encyclopediaNumber);
            _displayName = EditorGUILayout.TextField("表示名 *", _displayName);
            _reading = EditorGUILayout.TextField("読み", _reading);
            EditorGUILayout.LabelField("説明");
            _description = EditorGUILayout.TextArea(_description, GUILayout.MinHeight(48f));
            _category = EditorGUILayout.TextField("カテゴリ *", _category);
            DrawTimbrePopup();
            _basePitchMidi = EditorGUILayout.IntField("基準ピッチ MIDI（-1=未設定）", _basePitchMidi);
            _loudness = (LoudnessBand)EditorGUILayout.EnumPopup("強弱帯", _loudness);
            _duration = (DurationBand)EditorGUILayout.EnumPopup("長さ帯", _duration);
            _pairKey = EditorGUILayout.TextField("pairKey", _pairKey);
            _allowPitchShift = EditorGUILayout.Toggle("allowPitchShift", _allowPitchShift);
            _difficultyTagsCsv = EditorGUILayout.TextField("difficultyTags（CSV）", _difficultyTagsCsv);
            _initiallyUnlocked = EditorGUILayout.Toggle("初期解除", _initiallyUnlocked);
            _image = (Sprite)EditorGUILayout.ObjectField("画像", _image, typeof(Sprite), false);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("音声 *", EditorStyles.boldLabel);
            _clip = (AudioClip)EditorGUILayout.ObjectField("AudioClip", _clip, typeof(AudioClip), false);
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_clip == null))
            {
                if (GUILayout.Button("試聴", GUILayout.Height(28f)))
                {
                    EditorAudioPreview.Play(_clip);
                    _status = "試聴を再生中（もう一度押すか「停止」で止められます）";
                }
                if (GUILayout.Button("停止", GUILayout.Width(72f), GUILayout.Height(28f)))
                {
                    EditorAudioPreview.Stop();
                    _status = "試聴を停止しました";
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("WAV インポート", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _wavPath = EditorGUILayout.TextField(_wavPath);
            if (GUILayout.Button("選択", GUILayout.Width(64f)))
            {
                string path = EditorUtility.OpenFilePanel("音声ファイル", "", "wav,mp3,ogg,aiff");
                if (!string.IsNullOrEmpty(path)) _wavPath = path;
            }
            if (GUILayout.Button("取り込み", GUILayout.Width(80f)))
                ImportWav();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("保存", GUILayout.Height(32f)))
                SaveSound();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTimbrePopup()
        {
            var ids = new List<string>();
            var labels = new List<string>();
            if (_timbre != null)
            {
                var tags = _timbre.ValidTags();
                for (int i = 0; i < tags.Count; i++)
                {
                    ids.Add(tags[i].id);
                    labels.Add($"{tags[i].displayName} ({tags[i].id})");
                }
            }

            int index = Mathf.Max(0, ids.IndexOf(_timbreTagId));
            if (labels.Count == 0)
            {
                _timbreTagId = EditorGUILayout.TextField("音色タグ *", _timbreTagId);
                return;
            }

            index = EditorGUILayout.Popup("音色タグ *", index, labels.ToArray());
            if (index >= 0 && index < ids.Count) _timbreTagId = ids[index];
        }

        private void DrawTimbrePanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(240f));
            EditorGUILayout.LabelField("音色タグ", EditorStyles.boldLabel);
            if (_timbre != null && _timbre.Tags != null)
            {
                for (int i = 0; i < _timbre.Tags.Count; i++)
                {
                    var t = _timbre.Tags[i];
                    if (t == null) continue;
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button($"{t.displayName} ({t.id})", EditorStyles.miniButton))
                    {
                        _tagId = t.id;
                        _tagDisplayName = t.displayName;
                        _tagSortOrder = t.sortOrder;
                        _tagReplaceId = t.id;
                    }
                    if (GUILayout.Button("×", GUILayout.Width(24f)))
                    {
                        var r = CuratedSoundCatalogEditorOps.RemoveTimbreTag(_timbre, _catalog, t.id);
                        _status = r.IsSuccess ? "タグを削除しました" : r.Message;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(6f);
            _tagId = EditorGUILayout.TextField("tag id", _tagId);
            _tagDisplayName = EditorGUILayout.TextField("表示名", _tagDisplayName);
            _tagSortOrder = EditorGUILayout.IntField("sort", _tagSortOrder);
            if (GUILayout.Button("タグを保存"))
            {
                var draft = new TimbreTagDefinition
                {
                    id = _tagId,
                    displayName = _tagDisplayName,
                    sortOrder = _tagSortOrder
                };
                var r = CuratedSoundCatalogEditorOps.SaveTimbreTag(_timbre, draft, _tagReplaceId);
                _status = r.IsSuccess ? "タグを保存しました" : r.Message;
                if (r.IsSuccess) _tagReplaceId = draft.id;
            }
            if (GUILayout.Button("タグ新規フォーム"))
            {
                _tagId = string.Empty;
                _tagDisplayName = string.Empty;
                _tagSortOrder = 0;
                _tagReplaceId = null;
            }

            EditorGUILayout.EndVertical();
        }

        private void ImportWav()
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                _status = "先に ID を入力してください";
                return;
            }

            var result = CuratedSoundCatalogEditorOps.ImportWavToLibrary(_wavPath, _id.Trim());
            if (!result.IsSuccess)
            {
                _status = result.Message;
                return;
            }

            _clip = result.Value;
            _status = "WAV を取り込みました";
        }

        private void SaveSound()
        {
            var draft = BuildDraft();
            string replaceId = _isNew ? null : _selectedId;
            var result = CuratedSoundCatalogEditorOps.SaveSound(_catalog, _timbre, draft, replaceId);
            if (!result.IsSuccess)
            {
                _status = result.Message;
                return;
            }

            _isNew = false;
            _selectedId = draft.id;
            _status = "保存しました";
            GUI.FocusControl(null);
        }

        private CuratedSoundDefinition BuildDraft()
        {
            string[] tags = null;
            if (!string.IsNullOrWhiteSpace(_difficultyTagsCsv))
            {
                var parts = _difficultyTagsCsv.Split(',');
                var list = new List<string>();
                for (int i = 0; i < parts.Length; i++)
                {
                    var p = parts[i].Trim();
                    if (p.Length > 0) list.Add(p);
                }
                tags = list.ToArray();
            }

            return new CuratedSoundDefinition
            {
                id = _id != null ? _id.Trim() : string.Empty,
                encyclopediaNumber = _encyclopediaNumber,
                displayName = _displayName,
                reading = _reading,
                description = _description,
                imageRef = _image,
                timbreTagId = _timbreTagId,
                basePitchMidi = _basePitchMidi,
                loudnessBand = _loudness,
                durationBand = _duration,
                pairKey = _pairKey,
                allowPitchShift = _allowPitchShift,
                difficultyTags = tags,
                category = _category,
                clipRef = _clip,
                initiallyUnlocked = _initiallyUnlocked
            };
        }

        private void LoadForm(CuratedSoundDefinition item)
        {
            _id = item.id ?? string.Empty;
            _encyclopediaNumber = item.encyclopediaNumber;
            _displayName = item.displayName ?? string.Empty;
            _reading = item.reading ?? string.Empty;
            _description = item.description ?? string.Empty;
            _timbreTagId = item.timbreTagId ?? string.Empty;
            _basePitchMidi = item.basePitchMidi;
            _loudness = item.loudnessBand;
            _duration = item.durationBand;
            _pairKey = item.pairKey ?? string.Empty;
            _allowPitchShift = item.allowPitchShift;
            _difficultyTagsCsv = item.difficultyTags != null ? string.Join(",", item.difficultyTags) : string.Empty;
            _category = item.category ?? string.Empty;
            _initiallyUnlocked = item.initiallyUnlocked;
            _clip = item.clipRef;
            _image = item.imageRef;
            _wavPath = string.Empty;
        }

        private void ClearForm()
        {
            _id = string.Empty;
            _encyclopediaNumber = 1;
            _displayName = string.Empty;
            _reading = string.Empty;
            _description = string.Empty;
            _timbreTagId = _timbre != null && _timbre.ValidTags().Count > 0
                ? _timbre.ValidTags()[0].id
                : "bell";
            _basePitchMidi = CuratedSoundDefinition.UnsetPitchMidi;
            _loudness = LoudnessBand.None;
            _duration = DurationBand.None;
            _pairKey = string.Empty;
            _allowPitchShift = true;
            _difficultyTagsCsv = string.Empty;
            _category = string.Empty;
            _initiallyUnlocked = false;
            _clip = null;
            _image = null;
            _wavPath = string.Empty;
        }
    }
}
