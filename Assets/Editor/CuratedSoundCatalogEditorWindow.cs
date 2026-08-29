using System.Collections.Generic;
using Geidai.Common.Library;
using UnityEditor;
using UnityEngine;

namespace Geidai.EditorTools
{
    /// <summary>
    /// 制作側音の登録ウィンドウ（US-LIB-04 / IMGUI）。
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

        [MenuItem("Geidai/Library/Curated Sound Catalog")]
        public static void Open()
        {
            var window = GetWindow<CuratedSoundCatalogEditorWindow>("おとずかん とうろく");
            window.minSize = new Vector2(880f, 560f);
            window.LoadDefaults();
        }

        private void OnEnable() => LoadDefaults();

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
            _catalog = (CuratedSoundCatalog)EditorGUILayout.ObjectField("Catalog", _catalog, typeof(CuratedSoundCatalog), false);
            _timbre = (TimbreTagCatalog)EditorGUILayout.ObjectField("Timbre", _timbre, typeof(TimbreTagCatalog), false);
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            DrawSoundList();
            DrawSoundForm();
            DrawTimbrePanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSoundList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(220f));
            EditorGUILayout.LabelField("とうろくおん", EditorStyles.boldLabel);
            if (GUILayout.Button("ついか"))
            {
                _isNew = true;
                _selectedId = null;
                ClearForm();
                _status = "あたらしい おとを ついか";
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
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSoundForm()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(_isNew ? "しんき" : "へんしゅう", EditorStyles.boldLabel);
            _formScroll = EditorGUILayout.BeginScrollView(_formScroll);

            _id = EditorGUILayout.TextField("id *", _id);
            _encyclopediaNumber = EditorGUILayout.IntField("図鑑ナンバー *", _encyclopediaNumber);
            _displayName = EditorGUILayout.TextField("表示名 *", _displayName);
            _reading = EditorGUILayout.TextField("読み", _reading);
            _description = EditorGUILayout.TextArea(_description, GUILayout.MinHeight(48f));
            _category = EditorGUILayout.TextField("カテゴリ *", _category);
            DrawTimbrePopup();
            _basePitchMidi = EditorGUILayout.IntField("基準ピッチ MIDI (-1=未設定)", _basePitchMidi);
            _loudness = (LoudnessBand)EditorGUILayout.EnumPopup("強弱帯", _loudness);
            _duration = (DurationBand)EditorGUILayout.EnumPopup("長さ帯", _duration);
            _pairKey = EditorGUILayout.TextField("pairKey", _pairKey);
            _allowPitchShift = EditorGUILayout.Toggle("allowPitchShift", _allowPitchShift);
            _difficultyTagsCsv = EditorGUILayout.TextField("difficultyTags (CSV)", _difficultyTagsCsv);
            _initiallyUnlocked = EditorGUILayout.Toggle("初期解除", _initiallyUnlocked);
            _image = (Sprite)EditorGUILayout.ObjectField("画像", _image, typeof(Sprite), false);
            _clip = (AudioClip)EditorGUILayout.ObjectField("AudioClip *", _clip, typeof(AudioClip), false);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("WAV インポート", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _wavPath = EditorGUILayout.TextField(_wavPath);
            if (GUILayout.Button("選択", GUILayout.Width(64f)))
            {
                string path = EditorUtility.OpenFilePanel("WAV", "", "wav,mp3,ogg,aiff");
                if (!string.IsNullOrEmpty(path)) _wavPath = path;
            }
            if (GUILayout.Button("とりこむ", GUILayout.Width(80f)))
                ImportWav();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("ほぞん", GUILayout.Height(32f)))
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
            EditorGUILayout.LabelField("おんしょくタグ", EditorStyles.boldLabel);
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
                        _status = r.IsSuccess ? "タグを けしたよ" : r.Message;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(6f);
            _tagId = EditorGUILayout.TextField("tag id", _tagId);
            _tagDisplayName = EditorGUILayout.TextField("表示名", _tagDisplayName);
            _tagSortOrder = EditorGUILayout.IntField("sort", _tagSortOrder);
            if (GUILayout.Button("タグを ほぞん"))
            {
                var draft = new TimbreTagDefinition
                {
                    id = _tagId,
                    displayName = _tagDisplayName,
                    sortOrder = _tagSortOrder
                };
                var r = CuratedSoundCatalogEditorOps.SaveTimbreTag(_timbre, draft, _tagReplaceId);
                _status = r.IsSuccess ? "タグを ほぞんしたよ" : r.Message;
                if (r.IsSuccess) _tagReplaceId = draft.id;
            }
            if (GUILayout.Button("タグ しんきフォーム"))
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
                _status = "さきに ID を いれてね";
                return;
            }

            var result = CuratedSoundCatalogEditorOps.ImportWavToLibrary(_wavPath, _id.Trim());
            if (!result.IsSuccess)
            {
                _status = result.Message;
                return;
            }

            _clip = result.Value;
            _status = "WAV を とりこんだよ";
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
            _status = "ほぞんしたよ";
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
