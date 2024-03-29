﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public MainBorder() {
            SetValue(ButtonsProperty, new ObservableCollection<UIElement>());
        }

        static MainBorder() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainBorder), new FrameworkPropertyMetadata(typeof(MainBorder)));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            UIElement element = this.Template.FindName("MaximumButton", this) as UIElement;
            (element as Button).Click += Instance_RTMaximumClicked;
        }

        #region "Dependency Properties"
        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MainBorder), new PropertyMetadata(""));

        public ObservableCollection<UIElement> Buttons {
            get { return (ObservableCollection<UIElement>)GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CustomContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(ObservableCollection<UIElement>), typeof(MainBorder), new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.AffectsRender
                ));

        public bool IsMaximum {
            get { return (bool)GetValue(IsMaximumProperty); }
            set { SetValue(IsMaximumProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsMaximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMaximumProperty =
            DependencyProperty.Register("IsMaximum", typeof(bool), typeof(MainBorder), new PropertyMetadata(true));

        #endregion "Dependency Properties"

        #region "Routed Events"
        public static readonly RoutedEvent LBIconClickedEvent = EventManager.RegisterRoutedEvent("LBIconClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainBorder));
        public event RoutedEventHandler LBIconClicked {
            add { AddHandler(LBIconClickedEvent, value); }
            remove { RemoveHandler(LBIconClickedEvent, value); }
        }

        public static readonly RoutedEvent RTMaximumClickedEvent = EventManager.RegisterRoutedEvent("RTMaximumClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainBorder));
        public event RoutedEventHandler RTMaximumClicked {
            add { AddHandler(RTMaximumClickedEvent, value); }
            remove { RemoveHandler(RTMaximumClickedEvent, value); }
        }
        #endregion "Routed Events"

        private void Instance_RTMaximumClicked(object sender, RoutedEventArgs e) {
            var vm = sender as FrameworkElement;
            RoutedEventArgs ea = new RoutedEventArgs(RTMaximumClickedEvent, null);
            base.RaiseEvent(ea);
        }
    }
}
