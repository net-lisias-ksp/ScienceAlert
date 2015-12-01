using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.FileSystem;
using strange.extensions.command.impl;

namespace ScienceAlert.Core
{
// ReSharper disable once UnusedMember.Global
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandConfigureAssemblyDirectory : Command
    {
        private readonly IFileSystemFactory _fsFactory;

        public CommandConfigureAssemblyDirectory(IFileSystemFactory fsFactory)
        {
            if (fsFactory == null) throw new ArgumentNullException("fsFactory");
            _fsFactory = fsFactory;
        }


        public override void Execute()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var us = GetAssemblyLocation(assembly).SingleOrDefault();

            if (us == null)
                throw new AssemblyNotFoundException(assembly);

            injectionBinder.Bind<IFile>().ToValue(us).CrossContext();
            injectionBinder.Bind<IDirectory>().ToValue(us.Directory).CrossContext();
        }


        private Maybe<IFile> GetAssemblyLocation(Assembly assembly)
        {
            var results = AssemblyLoader.loadedAssemblies.Where(la => ReferenceEquals(la.assembly, assembly)).ToList();

            if (results.Count > 1) throw new InvalidOperationException("Multiple targets found in assembly loader");
            if (!results.Any()) return Maybe<IFile>.None;

            // oddly, the urls in AssemblyLoader don't specify the filename, only the directory
            var url = new KSPUrlIdentifier(results.First().url + Path.DirectorySeparatorChar + results.First().dllName);

            return _fsFactory.GameData.File(url);
        }
    }
}
