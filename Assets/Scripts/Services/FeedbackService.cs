using UnityEngine;
using Blobs.Interfaces;
using Blobs.Core;
using Blobs.Presenters;

namespace Blobs.Services
{
    /// <summary>
    /// Feedback service implementation.
    /// SRP: Only handles displaying feedback to the user.
    /// Wraps UIManager to decouple it from game logic.
    /// </summary>
    public class FeedbackService : MonoBehaviour, IFeedbackService
    {
        private void Awake()
        {
            // Self-register to ServiceLocator
            ServiceLocator.RegisterFeedback(this);
        }
        
        public void ShowFeedback(string message)
        {
            UIManager.Instance?.ShowFeedback(message);
        }
        
        public void ShowCannotSelectFeedback(IBlobPresenter blob)
        {
            UIManager.Instance?.ShowCannotSelectFeedback();
            blob?.PlayInvalidMoveEffect();
        }
        
        public void ShowSameColorFeedback(IBlobPresenter blob)
        {
            UIManager.Instance?.ShowSameColorFeedback();
            blob?.PlayInvalidMoveEffect();
        }
        
        public void ShowCannotMergeFeedback(IBlobPresenter blob)
        {
            UIManager.Instance?.ShowCannotMergeFeedback();
            blob?.PlayInvalidMoveEffect();
        }
        
        public void ShowNoTargetFeedback(IBlobPresenter blob)
        {
            UIManager.Instance?.ShowNoMoveFeedback();
            blob?.PlayInvalidMoveEffect();
        }
        
        public void ShowPathBlockedFeedback(IBlobPresenter blob)
        {
            UIManager.Instance?.ShowBlockedFeedback();
            blob?.PlayInvalidMoveEffect();
        }
        
        public void ShowMoveValidationFeedback(IBlobPresenter blob, MoveValidationResult result)
        {
            switch (result)
            {
                case MoveValidationResult.NoTarget:
                    ShowNoTargetFeedback(blob);
                    break;
                case MoveValidationResult.SameColor:
                    ShowSameColorFeedback(blob);
                    break;
                case MoveValidationResult.IncompatibleType:
                    ShowCannotMergeFeedback(blob);
                    break;
                case MoveValidationResult.NotAligned:
                    ShowCannotMergeFeedback(blob);
                    break;
                case MoveValidationResult.PathBlocked:
                    ShowPathBlockedFeedback(blob);
                    break;
            }
        }
    }
}
