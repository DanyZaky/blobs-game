using UnityEngine;
using System;
using Blobs.Interfaces;
using Blobs.Presenters;
using Blobs.Views;

namespace Blobs.Services
{
    /// <summary>
    /// Selection state management service.
    /// SRP: Only handles blob selection/deselection and hit testing.
    /// Does NOT handle move logic or feedback.
    /// </summary>
    public class SelectionService : MonoBehaviour, ISelectionService
    {
        private IBlobPresenter selectedBlob;
        
        // Service reference
        private IGridPresenter Grid => ServiceLocator.Grid;
        
        private void Awake()
        {
            // Self-register to ServiceLocator
            ServiceLocator.RegisterSelection(this);
        }
        
        public IBlobPresenter SelectedBlob => selectedBlob;
        
        // Events
        public event Action<IBlobPresenter> OnSelectionChanged;
        public event Action<IBlobPresenter> OnInvalidSelection;
        
        public void HandleClickAt(Vector2 worldPosition)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            
            if (hit.collider != null)
            {
                // Try to get BlobView from collider
                BlobView blobView = hit.collider.GetComponent<BlobView>();
                if (blobView != null)
                {
                    IBlobPresenter presenter = GetPresenterForView(blobView);
                    if (presenter != null)
                    {
                        HandleBlobClick(presenter);
                        return;
                    }
                }
            }
            
            // Clicked empty space - deselect if something selected
            if (selectedBlob != null)
            {
                Deselect();
            }
        }
        
        private IBlobPresenter GetPresenterForView(BlobView view)
        {
            if (Grid == null) return null;
            
            foreach (var blob in Grid.GetAllBlobs())
            {
                if (blob.View == view as IBlobView)
                {
                    return blob;
                }
            }
            return null;
        }
        
        private void HandleBlobClick(IBlobPresenter clickedBlob)
        {
            if (selectedBlob == null)
            {
                // Try to select
                if (clickedBlob.Model.CanInitiateMerge)
                {
                    Select(clickedBlob);
                }
                else
                {
                    // Can't select this blob
                    OnInvalidSelection?.Invoke(clickedBlob);
                }
            }
            else if (selectedBlob == clickedBlob)
            {
                // Clicked same blob - deselect
                Deselect();
            }
            else
            {
                // Clicked different blob while selected - this is handled by MoveService
                // We don't change selection here, just emit the click event with context
                // The InputPresenter will coordinate between SelectionService and MoveService
                OnSelectionChanged?.Invoke(clickedBlob);
            }
        }
        
        public void Select(IBlobPresenter blob)
        {
            if (blob == null) return;
            
            // Deselect previous if any
            if (selectedBlob != null)
            {
                selectedBlob.Deselect();
            }
            
            selectedBlob = blob;
            selectedBlob.Select();
            
            OnSelectionChanged?.Invoke(selectedBlob);
        }
        
        public void Deselect()
        {
            if (selectedBlob != null)
            {
                selectedBlob.Deselect();
                selectedBlob = null;
                
                OnSelectionChanged?.Invoke(null);
            }
        }
        
        public void ClearSelection()
        {
            if (selectedBlob != null)
            {
                selectedBlob.Deselect();
                selectedBlob = null;
            }
        }
    }
}
