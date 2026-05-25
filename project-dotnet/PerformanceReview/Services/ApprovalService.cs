using PerformanceReview.Models;
using PerformanceReview.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PerformanceReview.Services
{
    public class ApprovalService
    {
        private static ApprovalService? _instance;

        private ApprovalService() { }

        public static ApprovalService GetInstance()
        {
            if (_instance == null) _instance = new ApprovalService();
            return _instance;
        }

        public PendingAction? RequestSuspension(User requester, User target)
        {
            if (!IsManagerOf(requester, target)) return null;
            if (HasDirectReports(target)) return null;
            if (!HasPeer(target)) return null;

            var action = new PendingAction(requester, target, PendingAction.ACTION_SUSPEND, null);
            DataStore.GetInstance().AddPendingAction(action);
            return action;
        }

        public PendingAction? RequestTierChange(User requester, User target, int newTier)
        {
            if (!IsManagerOf(requester, target)) return null;
            if (HasDirectReports(target)) return null;
            if (newTier < 1 || newTier >= requester.TierLevel) return null;

            var action = new PendingAction(requester, target, PendingAction.ACTION_CHANGE_TIER, newTier);
            DataStore.GetInstance().AddPendingAction(action);
            return action;
        }

        public PendingAction? RequestDeletion(User requester, User target)
        {
            if (!IsManagerOf(requester, target)) return null;
            if (HasDirectReports(target)) return null;
            if (!HasPeer(target)) return null;

            var action = new PendingAction(requester, target, PendingAction.ACTION_DELETE, null);
            DataStore.GetInstance().AddPendingAction(action);
            return action;
        }

        public List<PendingAction> GetPendingRequests() =>
            DataStore.GetInstance().PendingActions.Where(a => a.IsPending()).ToList();

        public List<PendingAction> GetAllRequests() =>
            new List<PendingAction>(DataStore.GetInstance().PendingActions);

        public bool ApproveRequest(PendingAction action, User admin)
        {
            if (action == null || !action.IsPending()) return false;
            if (admin is not Admin) return false;

            var target = action.TargetUser;
            if (target == null) return false;

            switch (action.ActionType)
            {
                case PendingAction.ACTION_SUSPEND:
                    target.SetStatus(UserStatus.SUSPENDED);
                    break;
                case PendingAction.ACTION_CHANGE_TIER:
                    target.TierLevel = (int)action.NewValue!;
                    UserService.GetInstance().UpdateUserRole(target);
                    break;
                case PendingAction.ACTION_DELETE:
                    UserService.GetInstance().DeleteUser(target);
                    break;
                default:
                    return false;
            }

            action.Approve(admin);
            DataStore.GetInstance().SaveData();
            return true;
        }

        public bool RejectRequest(PendingAction action, User admin, string reason)
        {
            if (action == null || !action.IsPending()) return false;
            if (admin is not Admin) return false;

            action.Reject(admin, reason);
            DataStore.GetInstance().SaveData();
            return true;
        }

        public List<PendingAction> GetRequestsByUser(User user) =>
            DataStore.GetInstance().PendingActions.Where(a => a.RequestedBy?.Username == user.Username).ToList();

        private bool IsManagerOf(User requester, User target)
        {
            if (requester == null || target == null) return false;
            var manager = target.Manager;
            while (manager != null)
            {
                if (manager.Username == requester.Username) return true;
                manager = manager.Manager;
            }
            return false;
        }

        private bool HasPeer(User target)
        {
            if (target.Manager == null) return false;
            var siblings = target.Manager.DirectReports;
            return siblings != null && siblings.Count > 1;
        }

        private bool HasDirectReports(User target)
        {
            if (target == null) return false;
            return target.DirectReports?.Count > 0;
        }
    }
}
