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
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.IdleBot
{
    using System;
    using System.Collections.Generic;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    using Helpmebot.Startup;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        static void Main(string[] args)
        {
            var container = new WindsorContainer();
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
            container.Install(FromAssembly.Containing<Program>());

            container.Register(
                Component.For<IApplication>().ImplementedBy<Application>(),
                Component.For<HashSet<string>>()
                    .Named("channels")
                    .Instance(
                        new HashSet<string>
                            {
                                "##stwalkerster",
                                "##stwalkerster-development",
                                "##helpmebot",
                                "#wikipedia-en-accounts-devs"
                            }));

            var application = container.Resolve<IApplication>();

            application.Run();

            container.Dispose();
        }
    }
}
