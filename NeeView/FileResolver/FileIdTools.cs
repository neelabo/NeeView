using System;

namespace NeeView
{
    public static class FileIdTools
    {
        extension(FileId fileId)
        {
            public FileIdEx ToFileIdEx(VolumeDatabaseCache db)
            {
                var volumePathId = db.AddVolumePath(fileId.VolumePath);
                return new FileIdEx(volumePathId, fileId.FileId128);
            }
        }

        extension(FileIdEx fileIdEx)
        {
            public FileId ToFileId(VolumeDatabaseCache db)
            {
                var volumePath = db.GetVolumePath(fileIdEx.VolumePathId);
                if (volumePath == null) throw new Exception($"Cannot resolve volume path: {fileIdEx.VolumePathId}");

                return new FileId(volumePath, fileIdEx.FileId128);
            }
        }
    }

}

