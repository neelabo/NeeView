﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NeeView
{
    // HistoryChangedイベントの種類
    public enum HistoryChangedType
    {
        Load,
        Clear,
        Add,
        Update,
        Remove,
    }

    // HistoryChangedイベントの引数
    public class HistoryChangedArgs
    {
        public HistoryChangedType HistoryChangedType { get; set; }
        public string Key { get; set; }

        public HistoryChangedArgs(HistoryChangedType type, string key)
        {
            HistoryChangedType = type;
            Key = key;
        }
    }

    /// <summary>
    /// 履歴
    /// </summary>
    public class BookHistory
    {
        // 履歴に追加、削除された
        public event EventHandler<HistoryChangedArgs> HistoryChanged;

        // 履歴
        public LinkedList<Book.Memento> History { get; private set; } = new LinkedList<Book.Memento>();

        // 履歴保持最大数
        private int _MaxHistoryCount = 100;
        public int MaxHistoryCount
        {
            get { return _MaxHistoryCount; }
            set { _MaxHistoryCount = value; Resize(); }
        }

        // 履歴クリア
        public void Clear()
        {
            History.Clear();
            HistoryChanged?.Invoke(this, new HistoryChangedArgs(HistoryChangedType.Clear, null));
        }

        // 履歴サイズ調整
        private void Resize()
        {
            while (History.Count > MaxHistoryCount)
            {
                var path = History.Last().Place;
                History.RemoveLast();
                HistoryChanged?.Invoke(this, new HistoryChangedArgs(HistoryChangedType.Remove, path));
            }
        }


        // 履歴追加
        public void Add(Book book)
        {
            if (book?.Place == null) return;
            if (book.Pages.Count <= 0) return;

            var changedType = HistoryChangedType.Add;

            var item = History.FirstOrDefault(e => e.Place == book.Place);
            if (item != null)
            {
                History.Remove(item);
                changedType = HistoryChangedType.Update;
            }

            var setting = book.CreateMemento();
            History.AddFirst(setting);
            HistoryChanged?.Invoke(this, new HistoryChangedArgs(changedType, setting.Place));

            Resize();
        }

        // 履歴削除
        public void Remove(string place)
        {
            var item = History.FirstOrDefault(e => e.Place == place);
            if (item != null)
            {
                History.Remove(item);
                HistoryChanged?.Invoke(this, new HistoryChangedArgs(HistoryChangedType.Remove, item.Place));
            }
        }

        // 履歴検索
        public Book.Memento Find(string place)
        {
            return History.FirstOrDefault(e => e.Place == place);
        }

        // 最近使った履歴のリストアップ
        public List<Book.Memento> ListUp(int size)
        {
            var list = new List<Book.Memento>();
            foreach (var item in History)
            {
                if (list.Count >= size) break;
                list.Add(item);
            }
            return list;
        }


        #region Memento

        /// <summary>
        /// 履歴Memento
        /// </summary>
        [DataContract]
        public class Memento
        {
            [DataMember]
            public List<Book.Memento> History { get; set; }

            [DataMember]
            public int MaxHistoryCount { get; set; }

            private void Constructor()
            {
                History = new List<Book.Memento>();
                MaxHistoryCount = 100;
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            // 結合
            public void Merge(Memento memento)
            {
                History = History.Concat(memento?.History).Distinct(new Book.MementoPlaceCompare()).ToList();
                if (MaxHistoryCount < memento.MaxHistoryCount) MaxHistoryCount = memento.MaxHistoryCount;
            }

            // ファイルに保存
            public void Save(string path)
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = new System.Text.UTF8Encoding(false);
                settings.Indent = true;
                using (XmlWriter xw = XmlWriter.Create(path, settings))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Memento));
                    serializer.WriteObject(xw, this);
                }
            }

            // ファイルから読み込み
            public static Memento Load(string path)
            {
                using (XmlReader xr = XmlReader.Create(path))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Memento));
                    Memento memento = (Memento)serializer.ReadObject(xr);
                    return memento;
                }
            }
        }

        // memento作成
        public Memento CreateMemento(bool removeTemporary)
        {
            var memento = new Memento();
            memento.History = this.History.ToList();
            if (removeTemporary)
            {
                memento.History.RemoveAll((e) => e.Place.StartsWith(Temporary.TempDirectory));
            }

            memento.MaxHistoryCount = this.MaxHistoryCount;
            return memento;
        }

        // memento適用
        public void Restore(Memento memento)
        {
            this.History = new LinkedList<Book.Memento>(memento.History);
            this.MaxHistoryCount = memento.MaxHistoryCount;
            this.HistoryChanged?.Invoke(this, new HistoryChangedArgs(HistoryChangedType.Load, null));
        }


        #endregion
    }
}
