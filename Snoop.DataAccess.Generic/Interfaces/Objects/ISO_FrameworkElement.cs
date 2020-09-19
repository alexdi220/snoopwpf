﻿namespace Snoop.DataAccess.Interfaces {
    public interface ISO_FrameworkElement : ISO_UIElement {
        ISO_DependencyObject GetTemplatedParent();
        ISO_ResourceDictionary GetResources();
        double GetActualHeight();
        double GetActualWidth();
    }
}