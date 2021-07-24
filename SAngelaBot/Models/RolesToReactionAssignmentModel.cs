using System;

namespace SAngelaBot.Models
{
    public class RolesToReactionAssignmentModel
    {
        public ulong MessageId { get; set; }
        public ulong ReactionId { get; set; }
        public ulong RoleId { get; set; }

        public override string ToString()
        {
            return this.MessageId + "|" + this.RoleId + "|" + this.ReactionId;
        }

        public override bool Equals(object obj)
        {
            RolesToReactionAssignmentModel roleAssignment = obj as RolesToReactionAssignmentModel;
            return string.Equals(MessageId, roleAssignment.MessageId)
                && string.Equals(ReactionId, roleAssignment.ReactionId)
                && string.Equals(RoleId, roleAssignment.RoleId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, MessageId) ? MessageId.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, ReactionId) ? ReactionId.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, RoleId) ? RoleId.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
