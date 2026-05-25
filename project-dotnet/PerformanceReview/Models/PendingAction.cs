using System;

namespace PerformanceReview.Models
{
    public class PendingAction
    {
        public const string ACTION_SUSPEND = "SUSPEND";
        public const string ACTION_CHANGE_TIER = "CHANGE_TIER";
        public const string ACTION_DELETE = "DELETE";

        public const string STATUS_PENDING = "PENDING";
        public const string STATUS_APPROVED = "APPROVED";
        public const string STATUS_REJECTED = "REJECTED";

        public string Id { get; set; }
        public User RequestedBy { get; set; }
        public User TargetUser { get; set; }
        public string ActionType { get; set; }
        public object? NewValue { get; set; }
        public DateTime RequestedDate { get; set; }
        public string Status { get; set; }
        public string? RejectionReason { get; set; }
        public User? ProcessedBy { get; set; }
        public DateTime? ProcessedDate { get; set; }

        public PendingAction(User requestedBy, User targetUser, string actionType, object? newValue)
        {
            Id = Guid.NewGuid().ToString();
            RequestedBy = requestedBy;
            TargetUser = targetUser;
            ActionType = actionType;
            NewValue = newValue;
            RequestedDate = DateTime.Now;
            Status = STATUS_PENDING;
        }

        public bool IsPending() => STATUS_PENDING == Status;

        public void Approve(User admin)
        {
            Status = STATUS_APPROVED;
            ProcessedBy = admin;
            ProcessedDate = DateTime.Now;
        }

        public void Reject(User admin, string reason)
        {
            Status = STATUS_REJECTED;
            ProcessedBy = admin;
            ProcessedDate = DateTime.Now;
            RejectionReason = reason;
        }

        public string GetDescription()
        {
            string targetName = TargetUser?.Name ?? "Unknown";
            return ActionType switch
            {
                ACTION_SUSPEND => $"Suspend {targetName}",
                ACTION_CHANGE_TIER => $"Change {targetName} to Tier {NewValue}",
                ACTION_DELETE => $"Delete {targetName}",
                _ => $"{ActionType} on {targetName}"
            };
        }

        public override string ToString()
        {
            return $"{GetDescription()} [{Status}]";
        }
    }
}
