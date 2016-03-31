// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupChannel.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   //   it under the terms of the GNU General Public License as published by
//   //   the Free Software Foundation, either version 3 of the License, or
//   //   (at your option) any later version.
//   //   Helpmebot is distributed in the hope that it will be useful,
//   //   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   //   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   //   GNU General Public License for more details.
//   //   You should have received a copy of the GNU General Public License
//   //   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   The flag group channel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Model
{
    using Helpmebot.Persistence;

    /// <summary>
    /// The flag group channel.
    /// </summary>
    public class FlagGroupChannel : GuidEntityBase
    {
        /// <summary>
        /// Gets or sets the flag group.
        /// </summary>
        public virtual FlagGroup FlagGroup { get; set; }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        public virtual Channel Channel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is protected.
        /// </summary>
        public virtual bool IsProtected { get; set; }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool Equals(FlagGroupChannel other)
        {
            return base.Equals(other) && Equals(this.FlagGroup, other.FlagGroup) && Equals(this.Channel, other.Channel);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">
        /// The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. 
        /// </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((FlagGroupChannel)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.FlagGroup != null ? this.FlagGroup.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Channel != null ? this.Channel.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format(@"{0}: {1}", this.Channel, this.FlagGroup);
        }
    }
}