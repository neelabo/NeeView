﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 画像情報
    /// </summary>
    public class PictureInfo
    {
        /// <summary>
        /// 画像サイズ
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Length { get; set; } = -1;

        /// <summary>
        /// 最終更新日
        /// </summary>
        public DateTime? LastWriteTime { get; set; }

        /// <summary>
        /// EXIF
        /// </summary>
        public BitmapExif Exif { get; set; }


        /// <summary>
        /// Archiver
        /// </summary>
        public string Archiver { get; set; }

        /// <summary>
        /// Decoder
        /// </summary>
        public string Decoder { get; set; }


        // 実際に読み込まないとわからないもの

        /// <summary>
        /// 基本色
        /// </summary>
        public Color Color { get; set; } = Colors.Black;

        /// <summary>
        /// ピクセル深度
        /// </summary>
        public int BitsPerPixel { get; set; }


        //
        public bool IsPixelInfoEnabled => BitsPerPixel > 0;

        //
        public PictureInfo()
        {
        }

        //
        public PictureInfo(ArchiveEntry entry)
        {
            this.Length = entry.Length;
            this.LastWriteTime = entry.LastWriteTime;
            this.Archiver = entry.Archiver.ToString();
        }


        //
        public void SetPixelInfo(BitmapSource bitmap)
        {
            this.Size = new Size(bitmap.PixelWidth, bitmap.PixelHeight);

            // 以下、補助情報なので重要度は低い
            try
            {
                this.Color = bitmap.GetOneColor();
                this.BitsPerPixel = bitmap.GetSourceBitsPerPixel();
            }
            catch
            {
            }
        }


        //
        public static PictureInfo Create(ArchiveEntry entry, Size size, BitmapMetadata metadata)
        {
            var info = new PictureInfo();
            info.Size = size;
            info.Length = entry.Length;
            info.LastWriteTime = entry.LastWriteTime;
            info.Exif = metadata != null ? new BitmapExif(metadata) : null;
            info.Archiver = entry.Archiver.ToString();

            return info;
        }


    }

}
