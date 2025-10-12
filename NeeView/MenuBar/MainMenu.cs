using NeeLaboratory.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NeeView
{
    public class MainMenu : BindableBase
    {
        static MainMenu() => Current = new MainMenu();
        public static MainMenu Current { get; }


        private readonly MenuTree _mainMenuSource;
        private Menu? _menu;

#if DEBUG
        private readonly DebugMenu _debugMenu = new();
#endif


        private MainMenu()
        {
            _mainMenuSource = MenuTree.CreateDefault();

            Config.Current.MenuBar.AddPropertyChanged(nameof(MenuBarConfig.IsHamburgerMenu),
                (s, e) => Update());

            RoutedCommandTable.Current.Changed +=
                (s, e) => AppDispatcher.Invoke(() => UpdateInputGestureText());

            PageSlider.Current.SliderDirectionChanged +=
                (s, e) => AppDispatcher.Invoke(() => UpdateInputGestureText());

            Config.Current.Command.SubscribePropertyChanged(nameof(CommandConfig.IsReversePageMove),
                (s, e) => AppDispatcher.Invoke(() => UpdateInputGestureText()));

            Update();
        }


        public MenuTree MenuSource => _mainMenuSource;

        public Menu? Menu
        {
            get { return _menu; }
            set { SetProperty(ref _menu, value); }
        }


        public void Update()
        {
            this.Menu = CreateMainMenu(Config.Current.MenuBar.IsHamburgerMenu);
            this.Menu.UpdateInputGestureText();
        }

        public void UpdateInputGestureText()
        {
            Menu?.UpdateInputGestureText();
        }

        private Menu CreateMainMenu(bool isHamburgerMenu)
        {
            var items = _mainMenuSource.CreateMenuItems(true);
#if DEBUG
            items.Add(_debugMenu.CreateDevMenuItem());
#endif

            var menu = new Menu();
            if (isHamburgerMenu)
            {
                var image = new Image();
                image.Width = 18;
                image.Height = 18;
                image.Source = CreateMenuIcon();

                var topMenu = new MenuItem();
                topMenu.Header = image;
                foreach (var item in items)
                {
                    topMenu.Items.Add(item);
                }
                menu.Items.Add(topMenu);
            }
            else
            {
                menu.Margin = new Thickness(0, 0, 40, 0);
                foreach (var item in items)
                {
                    menu.Items.Add(item);
                }
            }

            return menu;
        }

        /// <summary>
        /// メニューアイコン作成
        /// </summary>
        /// <remarks>
        /// アイコンの色は親のForegroundを参照する
        /// </remarks>
        private static DrawingImage CreateMenuIcon()
        {
            var drawing = new GeometryDrawing();
            drawing.Geometry = App.Current.Resources["g_menu_24px"] as Geometry;
            BindingOperations.SetBinding(drawing, GeometryDrawing.BrushProperty, new Binding(nameof(Control.Foreground)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Control), 1) });
            var drawingImage = new DrawingImage(drawing);
            return drawingImage;
        }

    }
}
