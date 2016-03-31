// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationStartup.cs" company="Helpmebot Development Team">
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
//   The application startup.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Startup
{
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The application startup.
    /// </summary>
    public class ApplicationStartup
    {
        #region Public Methods and Operators

        /// <summary>
        /// The main.
        /// </summary>
        public static void Main()
        {
            var container = new WindsorContainer();

            container.Install(FromAssembly.This());

            // ReSharper disable once AccessToDisposedClosure
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            var application = container.Resolve<Application>();

            application.Run();

            container.Dispose();
        }

        #endregion
    }
}