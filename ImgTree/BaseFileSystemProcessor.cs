using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Fuse;
using Mono.Unix.Native;

namespace ImgTree {
	internal abstract class BaseFileSystemProcessor: IFileSystemProcessor {
		public abstract Errno AccessPath(string path, AccessModes mask);

		public Errno ChangePathOwner(string path, long uid, long gid) {
			return Errno.EROFS;
		}

		public Errno ChangePathPermissions(string path, FilePermissions mode) {
			return Errno.EROFS;
		}

		public Errno ChangePathTimes(string path, ref Utimbuf buf) {
			return Errno.EACCES;
		}

		public Errno CreateDirectory(string path, FilePermissions mode) {
			return Errno.EROFS;
		}

		public Errno CreateHardLink(string from, string to) {
			return Errno.EROFS;
		}

		public Errno CreateSpecialFile(string path, FilePermissions mode, ulong rdev) {
			return Errno.EROFS;
		}

		public Errno CreateSymbolicLink(string from, string to) {
			return Errno.EROFS;
		}

		public Errno GetFileSystemStatus(string path, out Statvfs stbuf) {
			stbuf = new Statvfs();
			return Errno.ENOSYS;
		}

		public Errno GetPathExtendedAttribute(string path, string name, byte[] value, out int bytesWritten) {
			bytesWritten = 0;
			return Errno.EOPNOTSUPP;
		}

		public abstract Errno GetPathStatus(string path, out Stat buf);

		public Errno ListPathExtendedAttributes(string path, out string[] names) {
			names = null;
			return Errno.EOPNOTSUPP;
		}

		public abstract Errno OpenHandle(string path, OpenedPathInfo info);

		public abstract Errno ReadDirectory(string path, OpenedPathInfo fi, out IEnumerable<DirectoryEntry> paths);

		public abstract Errno ReadHandle(string path, OpenedPathInfo info, byte[] buf, long offset, out int bytesRead);

		public abstract Errno ReadSymbolicLink(string path, out string target);

		public Errno ReleaseHandle(string path, OpenedPathInfo info) {
			return 0;
		}

		public Errno RemoveDirectory(string path) {
			return Errno.EROFS;
		}

		public Errno RemoveFile(string path) {
			return Errno.EROFS;
		}

		public Errno RemovePathExtendedAttribute(string path, string name) {
			return Errno.EOPNOTSUPP;
		}

		public Errno RenamePath(string from, string to) {
			return Errno.EROFS;
		}

		public Errno SetPathExtendedAttribute(string path, string name, byte[] value, XattrFlags flags) {
			return Errno.EOPNOTSUPP;
		}

		public Errno SynchronizeHandle(string path, OpenedPathInfo info, bool onlyUserData) {
			return 0;
		}

		public Errno TruncateFile(string path, long size) {
			return Errno.EROFS;
		}

		public Errno WriteHandle(string path, OpenedPathInfo info, byte[] buf, long offset, out int bytesWritten) {
			bytesWritten = 0;
			return Errno.ENOSPC;
		}
	}
}
