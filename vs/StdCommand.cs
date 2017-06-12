using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace VS
{
    /// <summary>
    /// Command handler
    /// </summary>
    sealed class StdCommand
    {
        public const int ID = 0x0100;

        public static readonly Guid SetGuid = 
            new Guid("03647b07-e8ca-4d6e-9aae-5b89bbd3276d");

        private readonly Package package;


        /// <summary>
        /// Initializes a new instance of the <see cref="StdCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private StdCommand(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if 
                ( this.ServiceProvider.GetService(typeof(IMenuCommandService)) 
                    is OleMenuCommandService commandService
                )
            {
                commandService.AddCommand
                    ( new MenuCommand(null, new CommandID(SetGuid, ID))
                    );
            }
        }


        public static StdCommand Instance { get; private set; }


        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private System.IServiceProvider ServiceProvider
        {
            get { return this.package; }
        }


        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new StdCommand(package);
        }

    }
}
