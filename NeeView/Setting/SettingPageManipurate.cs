using NeeView.Data;
using NeeView.Properties;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Setting
{
    /// <summary>
    /// Setting: Manipulate
    /// </summary>
    public class SettingPageManipulate : SettingPage
    {
        public SettingPageManipulate() : base(TextResources.GetString("SettingPage.Manipulate"))
        {
            var centerEnumMapWithoutAuto = typeof(DragControlCenter).VisibleAliasNameDictionary().Where(e => (DragControlCenter)e.Key != DragControlCenter.Auto).ToDictionary();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.GeneralViewOperation"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsLimitMove))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ViewHorizontalOrigin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ViewVerticalOrigin))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.RotateCenter), new PropertyMemberElementOptions() { EnumMap = centerEnumMapWithoutAuto })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ScaleCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.FlipCenter), new PropertyMemberElementOptions() { EnumMap = centerEnumMapWithoutAuto })));
            section.Children.Add(new SettingItemIndexValue<double>(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.AngleFrequency)), new AngleFrequency(), false));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.IsKeepPageTransform))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.ScrollDuration))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.PageMoveDuration))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.View, nameof(ViewConfig.AutoRotatePolicy))));

            this.Items = new List<SettingItem>() { section };
        }

        /// <summary>
        /// ビュー回転スナップ値
        /// </summary>
        public class AngleFrequency : IndexDoubleValue
        {
            private static readonly List<double> _values = new() { 0, 5, 10, 15, 20, 30, 45, 60, 90 };

            public AngleFrequency() : base(_values)
            {
            }

            public AngleFrequency(double value) : base(_values)
            {
                Value = value;
            }

            public override string ValueString => Value == 0 ? TextResources.GetString("Word.Stepless") : $"{Value} {TextResources.GetString("Word.Degree")}";
        }
    }

    /// <summary>
    /// Setting: Mouse
    /// </summary>
    public class SettingPageMouse : SettingPage
    {
        public SettingPageMouse() : base(TextResources.GetString("SettingPage.Manipulate.Mouse"))
        {
            this.Items = new List<SettingItem>();

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.MouseDrag"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsDragEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(DragActionTable.Current, nameof(DragActionTable.Elements)), new SettingMouseDragControl()) { IsStretch = true, });
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.MinimumDragDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.InertiaSensitivity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsGestureEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.GestureMinimumDistance))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.MouseHold"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonDownMode))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonMask))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonDownTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.LongButtonRepeatTime))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.HoverScroll"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.HoverScrollDuration))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.AutoScroll"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.AutoScrollSensitivity))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.MouseWheelScroll"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsMouseWheelScrollEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.MouseWheelScrollSensitivity))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.MouseWheelScrollDuration))));
            this.Items.Add(section);

            section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.MouseVisibility"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.CursorHideTime))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.CursorHideReleaseDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Mouse, nameof(MouseConfig.IsCursorHideReleaseAction))));
            this.Items.Add(section);
        }
    }

    /// <summary>
    /// Setting: Touch
    /// </summary>
    public class SettingPageTouch : SettingPage
    {
        public SettingPageTouch() : base(TextResources.GetString("SettingPage.Manipulate.Touch"))
        {
            var dragEnumMap = TouchActionClass.Drag.GetAliasNameMap().ToDictionary(e => (Enum)e.Key, e => e.Value);
            var holdEnumMap = TouchActionClass.Hold.GetAliasNameMap().ToDictionary(e => (Enum)e.Key, e => e.Value);

            var section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.TouchGeneral"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.DragAction), new PropertyMemberElementOptions() { EnumMap = dragEnumMap })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.HoldAction), new PropertyMemberElementOptions() { EnumMap = holdEnumMap })));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsAngleEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.IsScaleEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.GestureMinimumDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.MinimumManipulationRadius))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.MinimumManipulationDistance))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Touch, nameof(TouchConfig.InertiaSensitivity))));

            this.Items = new List<SettingItem>() { section };
        }
    }

    /// <summary>
    /// Setting: Loupe
    /// </summary>
    public class SettingPageLoupe : SettingPage
    {
        public SettingPageLoupe() : base(TextResources.GetString("SettingPage.Manipulate.Loupe"))
        {
            var section = new SettingItemSection(TextResources.GetString("SettingPage.Manipulate.LoupeGeneral"));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsLoupeCenter))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsResetByRestart))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsVisibleLoupeInfo))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsResetByPageChanged))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsWheelScalingEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.IsEscapeKeyEnabled))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.DefaultScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.MinimumScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.MaximumScale))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.ScaleStep))));
            section.Children.Add(new SettingItemProperty(PropertyMemberElement.Create(Config.Current.Loupe, nameof(LoupeConfig.Speed))));

            this.Items = new List<SettingItem>() { section };
        }
    }
}
