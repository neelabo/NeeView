﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウスルーペ
    /// </summary>
    public class MouseInputLoupe : MouseInputBase
    {
        private LoupeTransform _loupe;

        /// <summary>
        /// ルーペ開始座標
        /// </summary>
        private Point _loupeBasePosition;



        /// <summary>
        /// IsLoupeCenter property.
        /// </summary>
        private bool _IsLoupeCenter;
        public bool IsLoupeCenter
        {
            get { return _IsLoupeCenter; }
            set { if (_IsLoupeCenter != value) { _IsLoupeCenter = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// MinimumScale property.
        /// </summary>
        public double MinimumScale
        {
            get { return _minimumScale; }
            set { if (_minimumScale != value) { _minimumScale = value; RaisePropertyChanged(); } }
        }

        private double _minimumScale = 2.0;

        /// <summary>
        /// MaximumScale property.
        /// </summary>
        public double MaximumScale
        {
            get { return _maximumScale; }
            set { if (_maximumScale != value) { _maximumScale = Math.Max(value, _minimumScale); RaisePropertyChanged(); } }
        }

        private double _maximumScale = 10.0;


        /// <summary>
        /// DefaultScale property.
        /// </summary>
        public double DefaultScale
        {
            get { return _loupe.DefaultScale; }
            set
            {
                if (_loupe.DefaultScale != value)
                {
                    _loupe.DefaultScale = NVUtility.Clamp(value, _minimumScale, MaximumScale);
                    RaisePropertyChanged();
                }
            }
        }


        /// <summary>
        /// ScaleStep property.
        /// </summary>
        public double ScaleStep
        {
            get { return _scaleStep; }
            set { if (_scaleStep != value) { _scaleStep = value; RaisePropertyChanged(); } }
        }

        private double _scaleStep = 1.0;

        /// <summary>
        /// IsResetByRestart property.
        /// </summary>
        public bool IsResetByRestart
        {
            get { return _isResetByRestart; }
            set { if (_isResetByRestart != value) { _isResetByRestart = value; RaisePropertyChanged(); } }
        }

        private bool _isResetByRestart = false;

        /// <summary>
        /// IsResetByPageChanged property.
        /// </summary>
        public bool IsResetByPageChanged
        {
            get { return _isResetByPageChanged; }
            set { if (_isResetByPageChanged != value) { _isResetByPageChanged = value; RaisePropertyChanged(); } }
        }

        private bool _isResetByPageChanged = true;




        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="context"></param>
        public MouseInputLoupe(MouseInputContext context) : base(context)
        {
            _loupe = LoupeTransform.Current;
        }


        /// <summary>
        /// 長押しモード？
        /// 長押しモードの場合、全てのマウス操作がルーペ専属になる
        /// </summary>
        private bool _isLongDownMode;

        /// <summary>
        /// ボタン押されている？
        /// </summary>
        private bool _isButtonDown;

        /// <summary>
        /// 状態開始処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter">trueならば長押しモード</param>
        public override void OnOpened(FrameworkElement sender, object parameter)
        {
            if (parameter is bool isLongDownMode)
            {
                _isLongDownMode = isLongDownMode;
            }
            else
            {
                _isLongDownMode = false;
            }

            sender.Focus();
            sender.CaptureMouse();
            sender.Cursor = Cursors.None;

            _context.StartPoint = Mouse.GetPosition(sender);
            var center = new Point(sender.ActualWidth * 0.5, sender.ActualHeight * 0.5);
            Vector v = _context.StartPoint - center;
            _loupeBasePosition = (Point)(this.IsLoupeCenter ? -v : -v + v / _loupe.Scale);
            _loupe.Position = _loupeBasePosition;

            _loupe.IsEnabled = true;
            _isButtonDown = false;

            if (_isResetByRestart)
            {
                _loupe.Scale = _loupe.DefaultScale;
            }
        }

        /// <summary>
        /// 状態終了処理
        /// </summary>
        /// <param name="sender"></param>
        public override void OnClosed(FrameworkElement sender)
        {
            sender.Cursor = null;
            sender.ReleaseMouseCapture();

            _loupe.IsEnabled = false;
        }

        /// <summary>
        /// マウスボタンが押されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isButtonDown = true;

            if (_isLongDownMode)
            {
            }
            else
            {
                // ダブルクリック？
                if (e.ClickCount >= 2)
                {
                    // コマンド決定
                    MouseButtonChanged?.Invoke(sender, e);
                    if (e.Handled)
                    {
                        // その後の操作は全て無効
                        _isButtonDown = false;
                    }
                }
            }
        }

        /// <summary>
        /// マウスボタンが離されたときの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLongDownMode)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    // ルーペ解除
                    ResetState();
                }
            }
            else
            {
                if (!_isButtonDown) return;

                // コマンド決定
                // 離されたボタンがメインキー、それ以外は装飾キー
                MouseButtonChanged?.Invoke(sender, e);

                // その後の入力は全て無効
                _isButtonDown = false;
            }
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(_context.Sender);
            _loupe.Position = _loupeBasePosition - (point - _context.StartPoint);

            e.Handled = true;
        }

        /// <summary>
        /// マウスホイール処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TODO: 長押しでない時の他のホイール操作のあつかい
            if (e.Delta > 0)
            {
                LoupeZoomIn();
            }
            else
            {
                LoupeZoomOut();
            }

            e.Handled = true;
        }

        /// <summary>
        /// ズームイン
        /// </summary>
        public void LoupeZoomIn()
        {
            _loupe.Scale = Math.Min(_loupe.Scale + _scaleStep, _maximumScale);
        }

        /// <summary>
        /// ズームアウト
        /// </summary>
        public void LoupeZoomOut()
        {
            _loupe.Scale = Math.Max(_loupe.Scale - _scaleStep, _minimumScale);
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnKeyDown(object sender, KeyEventArgs e)
        {
            // ESC で 状態解除
            if (e.Key == Key.Escape)
            {
                // ルーペ解除
                ResetState();

                e.Handled = true;
            }
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public int _Version { get; set; } = Config.Current.ProductVersionNumber;

            [DataMember]
            public bool IsLoupeCenter { get; set; }
            [Obsolete, DataMember]
            public bool IsVisibleLoupeInfo { get; set; }

            [DataMember, DefaultValue(2.0)]
            [PropertyMember("ルーペ標準倍率", Tips = "ルーペの初期倍率です")]
            public double DefaultScale { get; set; }

            [DataMember, DefaultValue(2.0)]
            [PropertyMember("ルーペ最小倍率", Tips = "ルーペの最小倍率です")]
            public double MinimumScale { get; set; }

            [DataMember, DefaultValue(10.0)]
            [PropertyMember("ルーペ最大倍率", Tips = "ルーペの最大倍率です")]
            public double MaximumScale { get; set; }

            [DataMember, DefaultValue(1.0)]
            [PropertyMember("ルーペ倍率変化単位", Tips = "ルーペ倍率をこの値で変化させます")]
            public double ScaleStep { get; set; }

            [DataMember, DefaultValue(false)]
            [PropertyMember("ルーペ倍率リセット", Tips = "ルーペを開始するたびに標準倍率に戻します")]
            public bool IsResetByRestart { get; set; }

            [DataMember, DefaultValue(true)]
            [PropertyMember("ルーペページ切り替え解除", Tips = "ページを切り替えるとルーペを解除します")]
            public bool IsResetByPageChanged { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.IsLoupeCenter = this.IsLoupeCenter;
            memento.DefaultScale = this.DefaultScale;
            memento.MinimumScale = this.MinimumScale;
            memento.MaximumScale = this.MaximumScale;
            memento.ScaleStep = this.ScaleStep;
            memento.IsResetByRestart = this.IsResetByRestart;
            memento.IsResetByPageChanged = this.IsResetByPageChanged;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsLoupeCenter = memento.IsLoupeCenter;
            this.MinimumScale = memento.MinimumScale;
            this.MaximumScale = memento.MaximumScale;
            this.DefaultScale = memento.DefaultScale;
            this.ScaleStep = memento.ScaleStep;
            this.IsResetByRestart = memento.IsResetByRestart;
            this.IsResetByPageChanged = memento.IsResetByPageChanged;

#pragma warning disable CS0612

            // compatible before ver.26
            if (memento._Version < Config.GenerateProductVersionNumber(1, 26, 0))
            {
                _loupe.IsVisibleLoupeInfo = memento.IsVisibleLoupeInfo;
            }

#pragma warning restore CS0612

        }
        #endregion

    }
}
