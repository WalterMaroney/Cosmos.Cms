﻿using CDT.Cosmos.BlobService.Drivers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDT.Cosmos.BlobService
{
    public class StorageSynchronizer
    {
        private readonly ICosmosStorage _source;
        private readonly ICosmosStorage _destination;

        public StorageSynchronizer(ICosmosStorage source, ICosmosStorage destination)
        {
            _source = source;
            _destination = destination;
        }

        private async Task GetFileLists()
        {
            var results1 = await _source.GetBlobNamesByPath(null);
            var results2 = await _destination.GetBlobNamesByPath(null);

            foreach (var blob in results1)
            {
                if (results2.Any(a => a.Equals(blob)))
                {
                    // The blob is in both storage accounts
                }
                else
                {
                    var metaData = await _source.GetFileMetadataAsync(blob);
                    // The blob does not exist in storage 2
                    using var readStream = await _source.GetStreamAsync(blob);
                    {
                        var uploadMetadata = new Models.FileUploadMetaData()
                        {
                            ChunkIndex = 0,
                            ContentType = metaData.ContentType,
                            FileName = metaData.FileName,
                            RelativePath = metaData.FileName,
                            TotalChunks = 1,
                            TotalFileSize = metaData.ContentLength,
                            UploadUid = Guid.NewGuid().ToString()
                        };

                        await _destination.UploadStreamAsync(readStream, uploadMetadata, DateTimeOffset.UtcNow);
                    }

                }
            }
        }
    }
}
