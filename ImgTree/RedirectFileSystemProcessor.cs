using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Fuse;
using Mono.Unix.Native;

namespace ImgTree {
	internal class RedirectFileSystemProcessor: BaseFileSystemProcessor {

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public override Errno AccessPath(string path, AccessModes mask) {
			logger.Debug("RedirectFS AccessPath");
			int r = Syscall.access(ProgramConfiguration.BaseDir + path, mask);
			if(r == -1)
				return Stdlib.GetLastError();
			return 0;
		}
		
		public override Errno GetPathStatus(string path, out Stat buf) {
			logger.Debug("RedirectFS GetPathStatus");
			int r = Syscall.lstat(ProgramConfiguration.BaseDir + path, out buf);
			logger.Debug("RedirectFS GetPathStatus r {0}", r);
			if(r == -1)
				return Stdlib.GetLastError();
			return 0;
			//int r = Syscall.lstat(ProgramConfiguration.BaseDir + path, out buf);
			//if(r == -1)
			//	return Stdlib.GetLastError();
			//if((buf.st_mode & FilePermissions.S_IFREG) 
			//	== FilePermissions.S_IFREG
			//	&& path.EndsWith(".iso"
			//		, StringComparison.InvariantCultureIgnoreCase)) {
				//e.Stat.st_mode = (FilePermissions)(de.d_type << 12);
			//	buf.st_mode
			//		= FilePermissions.S_IFDIR
			//		| FilePermissions.S_IRWXU
			//		| FilePermissions.S_IRWXG
			//		| FilePermissions.S_IRWXO;
			//}
			//return 0;
		}

		public override Errno OpenHandle(string path, OpenedPathInfo info) {
			logger.Debug("RedirectFS OpenHandle");
			return ProcessFile(ProgramConfiguration.BaseDir + path, info.OpenFlags, delegate (int fd) { return 0; });
		}
		private delegate int FdCb(int fd);
		private static Errno ProcessFile(string path, OpenFlags flags, FdCb cb) {
			logger.Debug("RedirectFS ProcessFile");
			int fd = Syscall.open(path, flags);
			if(fd == -1)
				return Stdlib.GetLastError();
			int r = cb(fd);
			Errno res = 0;
			if(r == -1)
				res = Stdlib.GetLastError();
			Syscall.close(fd);
			return res;
		}

		public override Errno ReadDirectory(string path, OpenedPathInfo fi,
				out IEnumerable<DirectoryEntry> paths) {
			logger.Debug("RedirectFS ReadDirectory");
			IntPtr dp = Syscall.opendir(ProgramConfiguration.BaseDir + path);
			if(dp == IntPtr.Zero) {
				paths = null;
				return Stdlib.GetLastError();
			}

			Dirent de;
			List<DirectoryEntry> entries = new List<DirectoryEntry>();
			while((de = Syscall.readdir(dp)) != null) {
				FilePermissions st_mode = (FilePermissions)(de.d_type << 12);
				DirectoryEntry e = new DirectoryEntry(de.d_name);
				e.Stat.st_ino = de.d_ino;
				e.Stat.st_mode = (FilePermissions)(de.d_type << 12);
				if((st_mode & FilePermissions.S_IFREG)
					== FilePermissions.S_IFREG
					&& de.d_name.EndsWith(".iso"
					, StringComparison.InvariantCultureIgnoreCase))
					e.Stat.st_mode = FilePermissions.S_IFDIR
						| FilePermissions.S_IRUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH
						| FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH;
				entries.Add(e);
			}
			Syscall.closedir(dp);

			paths = entries;
			return 0;
		}

		public unsafe override Errno ReadHandle(string path, OpenedPathInfo info, byte[] buf, long offset, out int bytesRead) {
			logger.Debug("RedirectFS ReadHandle");
			int br = 0;
			Errno e = ProcessFile(ProgramConfiguration.BaseDir + path, OpenFlags.O_RDONLY, delegate (int fd) {
				fixed (byte* pb = buf) {
					return br = (int)Syscall.pread(fd, pb, (ulong)buf.Length, offset);
				}
			});
			bytesRead = br;
			return e;
		}

		public override Errno ReadSymbolicLink(string path, out string target) {
			logger.Debug("RedirectFS ReadSymbolicLink");
			target = null;
			StringBuilder buf = new StringBuilder(256);
			do {
				int r = Syscall.readlink(ProgramConfiguration.BaseDir + path, buf);
				if(r < 0) {
					return Stdlib.GetLastError();
				} else if(r == buf.Capacity) {
					buf.Capacity *= 2;
				} else {
					target = buf.ToString(0, r);
					return 0;
				}
			} while(true);
		}
		
	}
}
