using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Fuse;
using Mono.Unix.Native;

namespace ImgTree {
	class ImageTreeFileSystem: FileSystem {

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public ImageTreeFileSystem() {
		}

		protected override Errno OnGetPathStatus(string path, out Stat buf) {
			return FileSystemProcessorFactory.Create(path).GetPathStatus(path, out buf);
		}

		protected override Errno OnAccessPath(string path, AccessModes mask) {
			return FileSystemProcessorFactory.Create(path).AccessPath(path, mask);
		}

		protected override Errno OnReadSymbolicLink(string path, out string target) {
			return FileSystemProcessorFactory.Create(path).ReadSymbolicLink(path, out target);
		}

		protected override Errno OnReadDirectory(string path, OpenedPathInfo fi,
				out IEnumerable<DirectoryEntry> paths) {
			return FileSystemProcessorFactory.Create(path).ReadDirectory(path, fi, out paths);
		}

		protected override Errno OnCreateSpecialFile(string path, FilePermissions mode, ulong rdev) {
			return FileSystemProcessorFactory.Create(path).CreateSpecialFile(path, mode, rdev);
		}

		protected override Errno OnCreateDirectory(string path, FilePermissions mode) {
			return FileSystemProcessorFactory.Create(path).CreateDirectory(path, mode);
		}

		protected override Errno OnRemoveFile(string path) {
			return FileSystemProcessorFactory.Create(path).RemoveFile(path);
		}

		protected override Errno OnRemoveDirectory(string path) {
			return FileSystemProcessorFactory.Create(path).RemoveDirectory(path);
		}

		protected override Errno OnCreateSymbolicLink(string from, string to) {
			return FileSystemProcessorFactory.Create(to).CreateSymbolicLink(from, to);
		}

		protected override Errno OnRenamePath(string from, string to) {
			return FileSystemProcessorFactory.Create(from).RenamePath(from, to);
		}

		protected override Errno OnCreateHardLink(string from, string to) {
			return FileSystemProcessorFactory.Create(to).CreateHardLink(from, to);
		}

		protected override Errno OnChangePathPermissions(string path, FilePermissions mode) {
			return FileSystemProcessorFactory.Create(path).ChangePathPermissions(path, mode);
		}

		protected override Errno OnChangePathOwner(string path, long uid, long gid) {
			return FileSystemProcessorFactory.Create(path).ChangePathOwner(path, uid, gid);
		}

		protected override Errno OnTruncateFile(string path, long size) {
			return FileSystemProcessorFactory.Create(path).TruncateFile(path, size);
		}

		protected override Errno OnChangePathTimes(string path, ref Utimbuf buf) {
			return FileSystemProcessorFactory.Create(path).ChangePathTimes(path, ref buf);
		}

		protected override Errno OnOpenHandle(string path, OpenedPathInfo info) {
			return FileSystemProcessorFactory.Create(path).OpenHandle(path, info);
		}

		protected override unsafe Errno OnReadHandle(string path, OpenedPathInfo info, byte[] buf,
				long offset, out int bytesRead) {
			return FileSystemProcessorFactory.Create(path).ReadHandle(path, info, buf, offset, out bytesRead);
		}

		protected override unsafe Errno OnWriteHandle(string path, OpenedPathInfo info,
				byte[] buf, long offset, out int bytesWritten) {
			return FileSystemProcessorFactory.Create(path).WriteHandle(path, info, buf, offset, out bytesWritten);
		}

		protected override Errno OnGetFileSystemStatus(string path, out Statvfs stbuf) {
			return FileSystemProcessorFactory.Create(path).GetFileSystemStatus(path, out stbuf);
		}

		protected override Errno OnReleaseHandle(string path, OpenedPathInfo info) {
			return FileSystemProcessorFactory.Create(path).ReleaseHandle(path, info);
		}

		protected override Errno OnSynchronizeHandle(string path, OpenedPathInfo info, bool onlyUserData) {
			return FileSystemProcessorFactory.Create(path).SynchronizeHandle(path, info, onlyUserData);
		}

		protected override Errno OnSetPathExtendedAttribute(string path, string name, byte[] value, XattrFlags flags) {
			return FileSystemProcessorFactory.Create(path).SetPathExtendedAttribute(path, name, value, flags);
		}

		protected override Errno OnGetPathExtendedAttribute(string path, string name, byte[] value, out int bytesWritten) {
			return FileSystemProcessorFactory.Create(path).GetPathExtendedAttribute(path, name, value, out bytesWritten);
		}

		protected override Errno OnListPathExtendedAttributes(string path, out string[] names) {
			return FileSystemProcessorFactory.Create(path).ListPathExtendedAttributes(path, out names);
		}

		protected override Errno OnRemovePathExtendedAttribute(string path, string name) {
			return FileSystemProcessorFactory.Create(path).RemovePathExtendedAttribute(path, name);
		}

		//protected override Errno OnLockHandle(string file, OpenedPathInfo info, FcntlCommand cmd, ref Flock @lock) {
		//	Flock _lock = @lock;
		//	Errno e = ProcessFile(basedir + file, info.OpenFlags, fd => Syscall.fcntl(fd, cmd, ref _lock));
		//	@lock = _lock;
		//	return e;
		//}

		private bool ParseArguments(string[] args) {
			for(int i = 0; i < args.Length; ++i) {
				switch(args[i]) {
					case "-h":
					case "--help":
						ShowHelp();
						return false;
					default:
						if(base.MountPoint == null)
							base.MountPoint = args[i].TrimEnd(new char[] { '/' });
						else
							ProgramConfiguration.BaseDir = args[i].TrimEnd(new char[] { '/' });
							logger.Debug("BaseDir: {0}", ProgramConfiguration.BaseDir);
						break;
				}
			}
			if(base.MountPoint == null) {
				return Error("missing mountpoint");
			}
			if(ProgramConfiguration.BaseDir == null) {
				return Error("missing basedir");
			}
			return true;
		}

		private static void ShowHelp() {
			Console.Error.WriteLine("usage: ImgTree [options] mountpoint basedir:");
			FileSystem.ShowFuseHelp("ImgTree");
			Console.Error.WriteLine();
			Console.Error.WriteLine("ImgTree options:");
			Console.Error.WriteLine("    basedir                Directory to mirror");
		}

		private static bool Error(string message) {
			Console.Error.WriteLine("ImgTree: error: {0}", message);
			return false;
		}

		public static void Main(string[] args) {
			using(ImageTreeFileSystem fs = new ImageTreeFileSystem()) {
				string[] unhandled = fs.ParseFuseArguments(args);
				if(!fs.ParseArguments(unhandled))
					return;
				fs.Start();
			}
		}
	}
}