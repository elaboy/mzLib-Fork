// Copyright 2017 Stefan Solntsev
//
// This file (SourceFile.cs) is part of MassSpectrometry.
//
// MassSpectrometry is free software: you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MassSpectrometry is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
// License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with MassSpectrometry. If not, see <http://www.gnu.org/licenses/>.

namespace Readers
{
    public class SourceFile : IDisposable
    {
        private bool disposedValue;

        public SourceFile(string nativeIdFormat, string massSpectrometerFileFormat, string checkSum, string fileChecksumType, string id)
        {
            NativeIdFormat = nativeIdFormat;
            MassSpectrometerFileFormat = massSpectrometerFileFormat;
            CheckSum = checkSum;
            FileChecksumType = fileChecksumType;
            Id = id;
        }

        public SourceFile(string nativeIdFormat, string massSpectrometerFileFormat, string checkSum, string fileChecksumType, string filePath, string id)
        : this(nativeIdFormat, massSpectrometerFileFormat, checkSum, fileChecksumType, id)
        {
            Uri.TryCreate(Directory.GetParent(filePath).FullName, UriKind.Absolute, out Uri result);
            this.Uri = result;
            this.FileName = Path.GetFileName(filePath);
        }

        public SourceFile(string nativeIdFormat, string massSpectrometerFileFormat, string checkSum, string fileChecksumType, Uri uri, string id, string fileName)
            : this(nativeIdFormat, massSpectrometerFileFormat, checkSum, fileChecksumType, id)
        {
            this.Uri = uri;
            this.FileName = fileName;
        }

        public string NativeIdFormat { get; }
        public string MassSpectrometerFileFormat { get; }
        public string CheckSum { get; }
        public string FileChecksumType { get; }
        public Uri Uri { get; }

        public string FileName { get; }
        public string Id { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SourceFile()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}