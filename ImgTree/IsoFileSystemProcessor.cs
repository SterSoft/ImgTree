using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Fuse;
using Mono.Unix.Native;
using System.IO;
using DiscUtils.Vfs;
using DiscUtils;

namespace ImgTree {
	internal class IsoFileSystemProcessor: BaseFileSystemProcessor {

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private ImageReaderFactory ImageReaderFactory;
		public string IsoPath { get; set; }

		public IsoFileSystemProcessor(ImageReaderFactory imageReaderFactory) {
			ImageReaderFactory = imageReaderFactory;
		}

		public string InsidePath(string path) {
			logger.Debug("InsidePath");
			string retVal = path.Replace(IsoPath, "");
			logger.Debug(retVal);
			return retVal;
		}

		public override Errno AccessPath(string path, AccessModes mask) {
			logger.Debug("AccessPath");
			if((mask & AccessModes.W_OK) == AccessModes.W_OK)
				return Errno.EROFS;
			using(FileStream src = File.Open(IsoPath, FileMode.Open))
			using(VfsFileSystemFacade iso = ImageReaderFactory.Create(src)) {
				if(!iso.Exists(InsidePath(ProgramConfiguration.BaseDir + path).Replace("/", "\\"))) {
					return Errno.ENOENT;
				}
				return 0;
			}
		}

		public override Errno GetPathStatus(string path, out Stat buf) {
			logger.Debug("GetPathStatus");
			int r = Syscall.lstat(IsoPath, out buf);
			buf.st_mode = FilePermissions.S_IFDIR
				| FilePermissions.S_IRUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH
				| FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH;
			buf.st_nlink = 1;
			if(IsoPath == ProgramConfiguration.BaseDir + path) {
				if(r == -1)
					return Stdlib.GetLastError();
				return 0;
			}
			using(FileStream src = File.Open(IsoPath, FileMode.Open))
			using(VfsFileSystemFacade iso = ImageReaderFactory.Create(src)) {
				DiscFileSystemInfo info = iso.GetFileInfo(InsidePath(ProgramConfiguration.BaseDir + path).Replace("/", "\\"));
				if(!info.Exists)
					info = iso.GetDirectoryInfo(InsidePath(ProgramConfiguration.BaseDir + path).Replace("/", "\\"));
				if(!info.Exists)
					return Errno.ENOENT;
				buf.st_ino = 0;
				if(info is DiscFileInfo) {
					DiscFileInfo fi = ((DiscFileInfo)info);
					buf.st_size = fi.Length;
					buf.st_blocks = (long)Math.Ceiling((double)(fi.Length / buf.st_blksize));
					buf.st_mode = FilePermissions.S_IFREG
					 | FilePermissions.S_IRUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH
					 | FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH;
				} else {
					buf.st_mode = FilePermissions.S_IFDIR
					 | FilePermissions.S_IRUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH
					 | FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH;
				}
				return 0;
			}
		}

		public override Errno OpenHandle(string path, OpenedPathInfo info) {
			logger.Debug("OpenHandle");
			return ProcessFile(ProgramConfiguration.BaseDir + path, info.OpenFlags, delegate (Stream fd) { return 0; });
		}
		private delegate Errno FdCb(Stream fd);
		private Errno ProcessFile(string path, OpenFlags flags, FdCb cb) {
			using(FileStream src = File.Open(IsoPath, FileMode.Open))
			using(VfsFileSystemFacade iso = ImageReaderFactory.Create(src))
			using(Stream fd = iso.OpenFile(InsidePath(path).Replace("/", "\\"), FileMode.Open)) {
				Errno res = cb(fd);
				return res;
			}
		}

		public override Errno ReadDirectory(string path, OpenedPathInfo fi, out IEnumerable<DirectoryEntry> paths) {
			logger.Debug("ReadDirectory");
			try {
				using(FileStream src = File.Open(IsoPath, FileMode.Open))
				using(VfsFileSystemFacade iso = ImageReaderFactory.Create(src)) {
					List<DirectoryEntry> entries = new List<DirectoryEntry>();
					foreach(string directory in iso.GetDirectories(InsidePath(ProgramConfiguration.BaseDir + path).Replace("/", "\\"))) {
						DirectoryEntry e = new DirectoryEntry(Path.GetFileName(directory.Replace("\\", "/")));
						e.Stat.st_ino = 0;
						e.Stat.st_mode = FilePermissions.S_IFDIR
							| FilePermissions.S_IRUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH
							| FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH;
						entries.Add(e);
					}
					foreach(string file in iso.GetFiles(InsidePath(ProgramConfiguration.BaseDir + path).Replace("/", "\\"))) {
						DirectoryEntry e = new DirectoryEntry(Path.GetFileName(file.Replace("\\", "/")));
						e.Stat.st_ino = 0;
						e.Stat.st_mode = FilePermissions.S_IFREG
							| FilePermissions.S_IRUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH
							| FilePermissions.S_IXUSR | FilePermissions.S_IXGRP | FilePermissions.S_IXOTH;
						entries.Add(e);
					}
					paths = entries;
					return 0;
				}
			} catch(Exception ex) {
				logger.Error(ex, "ReadDirectory exception");
				throw;
			}
		}

		public unsafe override Errno ReadHandle(string path, OpenedPathInfo info, byte[] buf, long offset, out int bytesRead) {
			logger.Debug("ReadHandle");
			int br = 0;
			Errno e = ProcessFile(ProgramConfiguration.BaseDir + path, OpenFlags.O_RDONLY, delegate (Stream fd) {
				try {
					fd.Seek(offset, SeekOrigin.Begin);
					br = (int)fd.Read(buf, 0, buf.Length);
					return Errno.EISDIR;
				} catch(Exception ex) {
					logger.Error(ex, "ReadHandle exception");
					return Errno.EIO;
				}
			});
			bytesRead = br;
			if(e == Errno.EISDIR)
				return 0;
			return e;
		}

		public override Errno ReadSymbolicLink(string path, out string target) {
			target = null;
			return Errno.ENOSYS;
		}
	}
}
