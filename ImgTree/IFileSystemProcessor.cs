using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Unix.Native;
using Mono.Fuse;

namespace ImgTree {
	internal interface IFileSystemProcessor {
		Errno GetPathStatus(string path, out Stat buf);
		Errno AccessPath(string path, AccessModes mask);
		Errno ReadSymbolicLink(string path, out string target);
		Errno ReadDirectory(string path, OpenedPathInfo fi,
			out IEnumerable<DirectoryEntry> paths);
		Errno CreateSpecialFile(string path, FilePermissions mode, ulong rdev);
		Errno CreateDirectory(string path, FilePermissions mode);
		Errno RemoveFile(string path);
		Errno RemoveDirectory(string path);
		Errno CreateSymbolicLink(string from, string to);
		Errno RenamePath(string from, string to);
		Errno CreateHardLink(string from, string to);
		Errno ChangePathPermissions(string path, FilePermissions mode);
		Errno ChangePathOwner(string path, long uid, long gid);
		Errno TruncateFile(string path, long size);
		Errno ChangePathTimes(string path, ref Utimbuf buf);
		Errno OpenHandle(string path, OpenedPathInfo info);
		Errno ReadHandle(string path, OpenedPathInfo info, byte[] buf
			, long offset, out int bytesRead);
		Errno WriteHandle(string path, OpenedPathInfo info, byte[] buf
			, long offset, out int bytesWritten);
		Errno GetFileSystemStatus(string path, out Statvfs stbuf);
		Errno ReleaseHandle(string path, OpenedPathInfo info);
		Errno SynchronizeHandle(string path, OpenedPathInfo info
			, bool onlyUserData);
		Errno SetPathExtendedAttribute(string path, string name, byte[] value
			, XattrFlags flags);
		Errno GetPathExtendedAttribute(string path, string name, byte[] value
			, out int bytesWritten);
		Errno ListPathExtendedAttributes(string path, out string[] names);
		Errno RemovePathExtendedAttribute(string path, string name);
	}
}
