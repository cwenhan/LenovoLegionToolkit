﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Extensions;
using LenovoLegionToolkit.Lib.Features;

namespace LenovoLegionToolkit.WPF.Windows.Automation
{
    public partial class PowerModeWindow
    {
        private readonly PowerModeFeature _feature = IoCContainer.Resolve<PowerModeFeature>();
        private readonly PowerModeState _powerModeState;

        public event EventHandler<PowerModeState>? OnSave;

        public PowerModeWindow(PowerModeState powerModeState)
        {
            _powerModeState = powerModeState;

            InitializeComponent();

            ResizeMode = ResizeMode.CanMinimize;

            _titleBar.UseSnapLayout = false;
            _titleBar.CanMaximize = false;

            Loaded += PowerModeWindow_Loaded;
            IsVisibleChanged += PowerModeWindow_IsVisibleChanged;
        }

        private async void PowerModeWindow_Loaded(object sender, RoutedEventArgs e) => await RefreshAsync();

        private async void PowerModeWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsLoaded && IsVisible)
                await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            _loader.IsLoading = true;

            var states = await _feature.GetAllStatesAsync();

            foreach (var state in states)
            {
                var radio = new RadioButton
                {
                    Content = state.GetDisplayName(),
                    Tag = state,
                    IsChecked = state == _powerModeState
                };
                _content.Children.Add(radio);
            }

            _loader.IsLoading = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var radioButton = _content.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked ?? false);
            var state = (PowerModeState?)radioButton?.Tag;

            if (state is null)
                return;

            OnSave?.Invoke(this, state.Value);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}