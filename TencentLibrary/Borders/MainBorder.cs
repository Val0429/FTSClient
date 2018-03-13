﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TencentLibrary.Borders {
    public class MainBorder : ContentControl {
        static MainBorder() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainBorder), new FrameworkPropertyMetadata(typeof(MainBorder)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            UIElement element = this.Template.FindName("LBIcon", this) as UIElement;
            element.MouseDown += Instance_LBIconClicked;
        }

        #region "Dependency Properties"
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MainBorder), new PropertyMetadata(""));

        #endregion "Dependency Properties"

        #region "Routed Events"
        public static readonly RoutedEvent LBIconClickedEvent = EventManager.RegisterRoutedEvent("LBIconClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainBorder));
        public event RoutedEventHandler LBIconClicked {
            add { AddHandler(LBIconClickedEvent, value); }
            remove { RemoveHandler(LBIconClickedEvent, value); }
        }
        #endregion "Routed Events"

        private void Instance_LBIconClicked(object sender, MouseButtonEventArgs e) {
            var vm = sender as FrameworkElement;
            RoutedEventArgs ea = new RoutedEventArgs(MainBorder.LBIconClickedEvent, null);
            base.RaiseEvent(ea);
        }
    }
}
