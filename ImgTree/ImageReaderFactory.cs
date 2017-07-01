using DiscUtils.Vfs;
using DiscUtils.Iso9660;
using DiscUtils.Udf;
using System.IO;
using System;

namespace ImgTree {
    public class ImageReaderFactory {
        public enum ImageType {
            Invalid = 0,
            Iso9660 = 1,
            Udf = 2
        }

        private ImageType Type { get; set; }

        public ImageReaderFactory(ImageType type) {
            Type = type;
        }
        public VfsFileSystemFacade Create(Stream src) {
            if(Type == ImageType.Iso9660)
                return new CDReader(src, true);
            else if(Type == ImageType.Udf)
                return new UdfReader(src);
            throw new InvalidOperationException("Invalid image format specified");
        }
    }
}