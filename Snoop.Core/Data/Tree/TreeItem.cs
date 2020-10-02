// (c) Copyright Cory Plotts.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Snoop.Data.Tree
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Media;
    using JetBrains.Annotations;
    using Snoop.DataAccess.Interfaces;
    using Snoop.DataAccess.Sessions;
    using Snoop.Infrastructure.Helpers;

    public class TreeItem : INotifyPropertyChanged
    {
        private bool isExpanded;
        private bool isSelected;

        private string name = string.Empty;
        private string nameLower = string.Empty;
        private readonly string typeNameLower;
        private int childItemCount;

        public TreeItem(ISnoopObject target, TreeItem parent, TreeService treeService)
        {
            this.Target = target ?? throw new ArgumentNullException(nameof(target));
            this.TargetType = this.Target.TypeName;

            this.typeNameLower = this.TargetType.ToLower();

            this.Parent = parent;
            this.TreeService = treeService;

            if (parent != null)
            {
                this.Depth = parent.Depth + 1;
            }
        }

        /// <summary>
        /// The WPF object that this instance is wrapping
        /// </summary>
        public ISnoopObject Target { get; }

        public string TargetType { get; }

        /// <summary>
        /// The parent of this instance
        /// </summary>
        public TreeItem Parent { get; }

        public TreeService TreeService { get; }

        /// <summary>
        /// The depth (in the visual tree) of this instance
        /// </summary>
        public int Depth { get; }

        public string Name
        {
            get => this.name;

            set
            {
                if (this.name == value)
                {
                    return;
                }

                // ensure that name never is null
                this.name = value ?? string.Empty;
                this.nameLower = this.name.ToLower();

                this.OnPropertyChanged(nameof(this.Name));
                this.OnPropertyChanged(nameof(this.DisplayName));
            }
        }

        public virtual string DisplayName => this.Name;

        public int SortOrder { get; protected set; }

        /// <summary>
        /// The children of this instance
        /// </summary>
        public ObservableCollection<TreeItem> Children { get; } = new ObservableCollection<TreeItem>();

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;

                // Need to expand all ancestors so this will be visible in the tree.
                if (this.isSelected)
                {
                    this.Parent?.ExpandTo();
                }

                this.OnPropertyChanged(nameof(this.IsSelected));
                this.OnIsSelectedChanged();
            }
        }

        /// <summary>
        /// Need this to databind to TreeView so we can expand our children.
        /// </summary>
        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (this.isExpanded == value)
                {
                    return;
                }

                this.isExpanded = value;
                this.OnPropertyChanged(nameof(this.IsExpanded));
            }
        }

        public virtual ISO_Visual MainVisual => null;

        public virtual Brush TreeBackgroundBrush => new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));

        public virtual Brush VisualBrush => null;

        /// <summary>
        /// Checks to see if any property on this element has a binding error.
        /// </summary>
        public virtual bool HasBindingError => false;

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            var sb = new StringBuilder(4 + 1 + this.Name.Length + 2 + this.TargetType.Length + 1 + this.childItemCount > 0 ? 3 : 0);

            // [depth] name (type) numberOfChildren
            sb.AppendFormat("[{0:D3}] {1} ({2})", this.Depth, this.Name, this.TargetType);

            if (this.childItemCount != 0)
            {
                sb.Append(' ');
                sb.Append(this.childItemCount.ToString());
            }

            return sb.ToString();
        }

        protected virtual void OnIsSelectedChanged() {
            if (IsSelected)
                ExtensionLocator.From(this.Target).Get<IDAS_AdornerService>().HighlightedElement = this.Target;
        }

        /// <summary>
        /// Expand this element and all elements leading to it.
        /// Used to show this element in the tree view.
        /// </summary>
        public void ExpandTo()
        {
            this.Parent?.ExpandTo();

            this.IsExpanded = true;
        }

        /// <summary>
        /// Update the view of this visual, rebuild children as necessary
        /// </summary>
        public void Reload()
        {
            this.Children.Clear();
            this.Name = this.GetName();
            this.ReloadCore();

            // Reset children count prior to re-calculation
            this.childItemCount = 0;

            // calculate the number of dependency object children
            foreach (var child in this.Children)
            {
                this.childItemCount++;

                this.childItemCount += child.childItemCount;
            }
        }

        public virtual string GetName()
        {
            var result = string.Empty;

            if (this.Target is ISO_FrameworkElement targetFrameworkElement)
            {
                result = targetFrameworkElement.GetName();
                
            }

            return result;
        }

        protected virtual void ReloadCore() {
            // if(Parent!=null && !Parent.IsExpanded)
            //     return;
            var childrenCount = VisualTreeHelper2.GetChildrenCount(Target);
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper2.GetChild(Target, i);
                var result = TreeService.Construct(child, this);
                Children.Add(result);
            }
        }

        public virtual TreeItem FindNode(ISnoopObject target)
        {
            if (target is null)
            {
                return null;
            }

            if (ReferenceEquals(this.Target?.Source, target?.Source))
            {
                return this;
            }

            foreach (var child in this.Children)
            {
                var node = child.FindNode(target);

                if (!(node is null))
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Used for tree search.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <returns><c>true</c> if this matches <paramref name="value"/>. Otherwise <c>false</c>.</returns>
        public bool Filter(string value)
        {
            if (this.typeNameLower.Contains(value))
            {
                return true;
            }

            if (this.nameLower.Contains(value))
            {
                return true;
            }

            if (int.TryParse(value, out var n)
                && n == this.Depth)
            {
                return true;
            }

            return false;
        }

        protected void RemoveChild(TreeItem item)
        {
            item.IsSelected = false;
            this.Children.Remove(item);
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}