using System;
using Blobs.Interfaces;

namespace Blobs.Services
{
    /// <summary>
    /// Selection state management service.
    /// SRP: Only handles blob selection/deselection and hit testing.
    /// </summary>
    public interface ISelectionService
    {
        /// <summary>Currently selected blob (null if none)</summary>
        IBlobPresenter SelectedBlob { get; }

        /// <summary>Fired when selection changes</summary>
        event Action<IBlobPresenter> OnSelectionChanged;

        /// <summary>Fired when user tries to select a non-selectable blob</summary>
        event Action<IBlobPresenter> OnInvalidSelection;

        /// <summary>Select a blob</summary>
        void Select(IBlobPresenter blob);

        /// <summary>Deselect current blob</summary>
        void Deselect();

        /// <summary>Handle click at world position - performs hit testing</summary>
        void HandleClickAt(UnityEngine.Vector2 worldPosition);

        /// <summary>Clear selection without notifying</summary>
        void ClearSelection();
    }
}
