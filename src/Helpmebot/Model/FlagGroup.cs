// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroup.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Defines the FlagGroup type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Model
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpmebot.Persistence;

    /// <summary>
    /// The flag group.
    /// </summary>
    public class FlagGroup : GuidEntityBase
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether deny group.
        /// </summary>
        public virtual bool DenyGroup { get; set; }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        public virtual IList<FlagGroupAssoc> Flags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is protected.
        /// </summary>
        public virtual bool IsProtected { get; set; }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        public virtual IList<FlagGroupUser> Users { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public virtual string Name { get; set; }

        #endregion

        #region Public Methods and Operators

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

            return this.Equals((FlagGroup)obj);
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
                hashCode = (hashCode * 397) ^ (this.Flags != null ? this.Flags.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            string flagsInGroup = this.Flags != null
                                      ? string.Join(", ", this.Flags.Select(x => x.Flag).ToArray())
                                      : string.Empty;

            return string.Format(@"{0} {2}{{{1}}}", this.Name, flagsInGroup, this.DenyGroup ? "! " : string.Empty);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        protected bool Equals(FlagGroup other)
        {
            return base.Equals(other) && Equals(this.Flags, other.Flags) && string.Equals(this.Name, other.Name);
        }

        #endregion
    }
}