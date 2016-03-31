// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Helpmebot Development Team">
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
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Client
{
    using System;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    using Helpmebot.Startup;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        #region Methods

        /// <summary>
        /// The main.
        /// </summary>
        private static void Main()
        {
            Console.Write("Initialising container...");

            var container = new WindsorContainer();
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            container.Install(FromAssembly.Containing<WindsorServiceLocator>());

            container.Register(Component.For<IApplication>().ImplementedBy<ClientApplication>());

            var application = container.Resolve<IApplication>();

            application.Run();

            container.Dispose();
        }

        #endregion
    }
}