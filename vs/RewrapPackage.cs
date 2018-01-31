//------------------------------------------------------------------------------
// <copyright file="RewrapCommandPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace VS
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for
    /// Visual Studio is to implement the IVsPackage interface and register
    /// itself with the shell. This package uses the helper classes defined
    /// inside the Managed Package Framework (MPF) to do it: it derives from the
    /// Package class that provides the implementation of the IVsPackage
    /// interface and uses the registration attributes defined in the framework
    /// to register itself and its components with the shell. These attributes
    /// tell the pkgdef creation utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset
    /// Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(RewrapPackage.GuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(Options.OptionsPage), "Rewrap", "Rewrap", 0, 0, true, new string[] { "column", "wrapping" })]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.NoSolution)]
    public sealed class RewrapPackage : Microsoft.VisualStudio.Shell.Package
    {
        /// Guid for the package
        public const string GuidString = "97d91fdc-1781-499d-97f6-31501ae81702";

        /// Guid for the command set
        public static readonly Guid CmdSetGuid = 
            new Guid( "03647b07-e8ca-4d6e-9aae-5b89bbd3276d" );


        // Inside this constructor you can place any initialization code that
        // does not require any Visual Studio service because at this point the
        // package object is created but not sited yet inside Visual Studio
        // environment. The place to do all the other initialization is the
        // Initialize method.
        public RewrapPackage()
        {
        }

        /// Initialization of the package; this method is called right after the
        /// package is sited, so this is the place where you can put all the
        /// initialization code that rely on services provided by VisualStudio.
        protected override void Initialize()
        {
            RewrapCommand.Initialize(this);
            AutoWrapCommand.Initialize(this);
            base.Initialize();
        }
    }
}
