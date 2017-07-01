using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiscUtils.Udf;
using DiscUtils.Iso9660;

namespace ImgTree {
	internal static class FileSystemProcessorFactory {

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static string IsoPath(string path) {
			logger.Debug("IsoPath");
			FileInfo fi = new FileInfo(path);
			while(!fi.Exists) {
				if(fi.FullName == ProgramConfiguration.BaseDir)
					return string.Empty;
				fi = new FileInfo(fi.Directory.FullName);
				logger.Debug("IsoPath, fi.FullName: {0}", fi.FullName);
			}
			return fi.FullName;
		}

		public static ImageReaderFactory.ImageType IsIso(string path) {
			logger.Debug("IsIso");
			if(Directory.Exists(path))
				return ImageReaderFactory.ImageType.Invalid;
			if(File.Exists(path)) {
				using(Stream src = File.OpenRead(path)) {
					if (CDReader.Detect(src))
						return ImageReaderFactory.ImageType.Iso9660;
					if(UdfReader.Detect(src))
						return ImageReaderFactory.ImageType.Udf;
				}
			}
			return ImageReaderFactory.ImageType.Invalid;
		}
		private static bool IsInIso(string path) {
			logger.Debug("IsInIso");			
			if(File.Exists(path) || Directory.Exists(path))
				return false;
			FileInfo fi = new FileInfo(path);
			while(!fi.Exists) {								
				logger.Debug("IsInIso, fi.FullName: {0}", fi.FullName);
				if(fi.FullName == ProgramConfiguration.BaseDir)
					return false;
				fi = new FileInfo(fi.Directory.FullName);
			}			
			return fi.Exists;
		}

		public static IFileSystemProcessor Create(string path) {
			ImageReaderFactory.ImageType isIso = IsIso(ProgramConfiguration.BaseDir +  path);
			if(isIso != ImageReaderFactory.ImageType.Invalid
				|| IsInIso(ProgramConfiguration.BaseDir + path)) {
				isIso = IsIso(IsoPath(ProgramConfiguration.BaseDir + path));
				return new IsoFileSystemProcessor(new ImageReaderFactory(isIso)) {
					IsoPath = IsoPath(ProgramConfiguration.BaseDir + path)
				};
			} else {
				return new RedirectFileSystemProcessor();
			}
		}
	}
}
