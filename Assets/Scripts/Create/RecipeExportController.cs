using System;
using Geidai.Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Geidai.Create
{
    /// <summary>WAVE 書き出しの確認トリガ。</summary>
    public class RecipeExportController : MonoBehaviour
    {
        [SerializeField] private Button exportButton;
        [SerializeField] private ConfirmDialog confirmDialog;

        public event Action ExportConfirmed;

        private void Awake()
        {
            if (exportButton != null) exportButton.onClick.AddListener(OnExportClicked);
        }

        private void OnExportClicked()
        {
            if (confirmDialog != null)
            {
                confirmDialog.Show("かきだし", "WAVE に かきだす？", () => ExportConfirmed?.Invoke());
            }
            else
            {
                ExportConfirmed?.Invoke();
            }
        }
    }
}
