﻿/* ========================================================================
 * Copyright (c) 2005-2021 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

namespace UACloudLibrary
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using UACloudLibrary.Interfaces;

    /// <summary>
    /// Azure storage class
    /// </summary>
    public class LocalFileStorage : IFileStorage
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        public LocalFileStorage(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger("LocalFileStorage");
        }

        /// <summary>
        /// Find a file based on a unique name
        /// </summary>
        public Task<string> FindFileAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                if (File.Exists(Path.Combine(Path.GetTempPath(), name)))
                {
                    return Task.FromResult(name);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Upload a file to a blob and return the filename for storage in the index db
        /// </summary>
        public async Task<string> UploadFileAsync(string name, string content, CancellationToken cancellationToken = default)
        {
            try
            {
                await File.WriteAllTextAsync(Path.Combine(Path.GetTempPath(), name), content).ConfigureAwait(false);
                return name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Download a blob to a file.
        /// </summary>
        public async Task<string> DownloadFileAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await File.ReadAllTextAsync(Path.Combine(Path.GetTempPath(), name)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }
    }
}
