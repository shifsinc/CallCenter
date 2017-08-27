﻿using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RequestServiceImpl.Dto;

namespace CRMPhone.Controls
{
    /// <summary>
    /// Логика взаимодействия для RequestControl.xaml
    /// </summary>
    public partial class RequestControl : UserControl
    {
        public RequestControl()
        {
            InitializeComponent();
        }

        private void tabItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //e.Key = Key.Tab;
                var request = new TraversalRequest(FocusNavigationDirection.Right) { Wrapped = true };
                if (sender is ComboBox)
                {
                    var parentDepObj = VisualTreeHelper.GetParent(sender as DependencyObject);
                    var comboBoxes = (parentDepObj as WrapPanel).Children.OfType<ComboBox>().ToList();
                    var currentIndex = comboBoxes.IndexOf(sender as ComboBox);
                    if (currentIndex < comboBoxes.Count - 1)
                        comboBoxes[currentIndex + 1].Focus();
                    else
                    {
                        var t = (sender as FrameworkElement).PredictFocus(FocusNavigationDirection.Next);
                        (sender as FrameworkElement).MoveFocus(request);
                    }
                }
                else
                {
                    var t = (sender as FrameworkElement).PredictFocus(FocusNavigationDirection.Next);
                    (sender as FrameworkElement).MoveFocus(request);
                }
            }
        }

        private void OnCbObjectCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var count = CbWorkers.Items.Cast<WorkerForFilterDto>().Count(w => w.Selected);
            if (count > 1)
            {
                CbWorkers.Text = "Выбрано несколько";
            }
            else if (count == 1)
            {
                var item = CbWorkers.Items.Cast<WorkerForFilterDto>().FirstOrDefault(w => w.Selected);
                CbWorkers.Text = item?.FullName;
            }
            else
                CbWorkers.Text = "";
        }

        private void OnCbSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.SelectedItem = null;
        }

        private void CbWorkers_OnDropDownClosed(object sender, EventArgs e)
        {
            var count = CbWorkers.Items.Cast<WorkerForFilterDto>().Count(w => w.Selected);
            if (count > 1)
            {
                CbWorkers.Text = "Выбрано несколько";
            }
            else if (count == 1)
            {
                var item = CbWorkers.Items.Cast<WorkerForFilterDto>().FirstOrDefault(w => w.Selected);
                CbWorkers.Text = item?.FullName;
            }
            else
                CbWorkers.Text = "";

        }
    }
}
