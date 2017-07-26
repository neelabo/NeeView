﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Picture Factory interface.
    /// </summary>
    public interface IPictureFactory
    {
        Picture Create(ArchiveEntry entry);
        BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size);
        Size CreateFixedSize(ArchiveEntry entry, Size size);

    }



    /// <summary>
    /// Picture Factory
    /// </summary>
    public class PictureFactory : IPictureFactory
    {
        //
        private static PictureFactory _current;
        public static PictureFactory Current = _current = _current ?? new PictureFactory();

        //
        private DefaultPictureFactory _defaultFactory = new DefaultPictureFactory();

        private PdfPictureFactory _pdfFactory = new PdfPictureFactory();


        //
        private TResult RetryWhenOutOfMemory<TResult>(Func<TResult> func)
        {
            int retry = 0;
            RETRY:

            try
            {
                return func();
            }
            catch (OutOfMemoryException) when (retry == 0)
            {
                Debug.WriteLine("Retry...");
                retry++;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                goto RETRY;
            }
            catch(Exception)
            {
                throw;
            }
        }

        //
        public Picture Create(ArchiveEntry entry)
        {
            return RetryWhenOutOfMemory(
                () =>
                {
                    if (entry.Archiver is PdfArchiver)
                    {
                        return _pdfFactory.Create(entry);
                    }
                    else
                    {
                        return _defaultFactory.Create(entry);
                    }
                });
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            Debug.WriteLine($"Create: {entry.EntryLastName} ({size.Truncate()})");

            return RetryWhenOutOfMemory(
                () =>
                {
                    if (entry.Archiver is PdfArchiver)
                    {
                        return _pdfFactory.CreateBitmapSource(entry, size);
                    }
                    else
                    {
                        return _defaultFactory.CreateBitmapSource(entry, size);
                    }
                });
        }

        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            if (entry.Archiver is PdfArchiver)
            {
                return _pdfFactory.CreateFixedSize(entry, size);
            }
            else
            {
                return _defaultFactory.CreateFixedSize(entry, size);
            }
        }
    }

    /// <summary>
    /// Default Picture Factory
    /// </summary>
    public class DefaultPictureFactory : IPictureFactory
    {
        //
        private PictureStream _pictureStream = new PictureStream();

        //
        private BitmapSourceFactory _bitmapFactory = new BitmapSourceFactory();

        //
        public Picture Create(ArchiveEntry entry)
        {
            var picture = new Picture(entry);

            using (var stream = _pictureStream.Create(entry))
            {
                // info
                var info = _bitmapFactory.CreateInfo(stream.Stream);
                var size = new Size(info.PixelWidth, info.PixelHeight);

                // bitmap
                size = PictureProfile.Current.Maximum.IsContains(size) ? Size.Empty : size.Uniformed(PictureProfile.Current.Maximum);
                var bitmapSource = _bitmapFactory.Create(stream.Stream, size, info);

                //
                picture.PictureInfo.Exif = info.Metadata != null ? new BitmapExif(info.Metadata) : null;
                picture.PictureInfo.Decoder = stream.Name ?? ".Net BitmapImage";
                picture.PictureInfo.SetPixelInfo(bitmapSource);

                picture.BitmapSource = bitmapSource;
            }

            return picture;
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            using (var stream = _pictureStream.Create(entry))
            {
                return _bitmapFactory.Create(stream.Stream, size);
            }
        }

        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            return PictureProfile.Current.CreateFixedSize(size);
        }
    }

    /// <summary>
    /// PDF Picture Factory
    /// </summary>
    public class PdfPictureFactory : IPictureFactory
    {
        public Picture Create(ArchiveEntry entry)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;
            var profile = PdfArchiverProfile.Current;

            var bitmapSource = pdfArchiver.CraeteBitmapSource(entry, profile.RenderSize);

            var picture = new Picture(entry);

            picture.PictureInfo.Size = new Size(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            picture.PictureInfo.Decoder = "PDFium";

            picture.BitmapSource = bitmapSource;

            return picture;
        }

        public BitmapSource CreateBitmapSource(ArchiveEntry entry, Size size)
        {
            var pdfArchiver = (PdfArchiver)entry.Archiver;
            size = size.IsEmpty ? PdfArchiverProfile.Current.RenderSize : size;
            return pdfArchiver.CraeteBitmapSource(entry, size);
        }

        public Size CreateFixedSize(ArchiveEntry entry, Size size)
        {
            return PdfArchiverProfile.Current.CreateFixedSize(size);
        }
    }
}
