﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;

namespace SiliconStudio.Presentation.Core
{
    /// <summary>
    /// This class hold the <see cref="IsFocusedProperty"/> attached dependency property that allows to give the focus to a control using bindings.
    /// </summary>
    public static class FocusManager
    {
        /// <summary>
        /// Identify the IsFocused attached dependency property.
        /// </summary>
        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(FocusManager), new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        /// <summary>
        /// Gets whether the given object has currently the focus.
        /// </summary>
        /// <param name="obj">The object. If it is not an <see cref="UIElement"/>, this method will return <c>false</c>.</param>
        /// <returns><c>true</c> if the given object has the focus, false otherwise.</returns>
        public static bool GetIsFocused(DependencyObject obj)
        {
            var uiElement = obj as UIElement;
            return uiElement != null && uiElement.IsFocused;
        }

        /// <summary>
        /// Gives the focus to the given object.
        /// </summary>
        /// <param name="obj">The object that should get the focus.</param>
        /// <param name="value">The state of the focus. If value is <c>true</c>, the object will get the focus. Otherwise, this method does nothing.</param>
        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        /// <summary>
        /// Raised when the <see cref="IsFocusedProperty"/> dependency property is modified.
        /// </summary>
        /// <param name="obj">The dependency object.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnIsFocusedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var uiElement = (UIElement)obj;
            if ((bool)e.NewValue)
            {
                uiElement.Focus();
            }
        }
    }
}
