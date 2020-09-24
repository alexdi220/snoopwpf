namespace Snoop.DataAccess.WinUI3 {
    using System;
    using System.ComponentModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;
    using Snoop.DataAccess.Interfaces;

    public class SO_PropertyDescriptor : SnoopObjectBase, ISO_PropertyDescriptor{
        readonly PropertyDescriptor descriptor;
        readonly DependencyPropertyDescriptor dpd;
        public SO_PropertyDescriptor(PropertyDescriptor source) : base(source) {
            this.descriptor = source;
            this.dpd = DependencyPropertyDescriptor.FromProperty(this.descriptor);
        }
        public bool IsReadOnly {
            get { return descriptor.IsReadOnly; }
        }
        public AttributeCollection Attributes {
            get { return this.descriptor.Attributes; }
        }
        public string Name {
            get => this.descriptor.Name;
        }
        public string DisplayName {
            get => this.descriptor.DisplayName;
        }
        public string BindingError {
            get => string.Empty;
        }

        public ISO_ValueSource GetValueSource(ISnoopObject source) {
            return new SO_ValueSource();
        }
        public Type PropertyType {
            get => this.descriptor.PropertyType;
        }
        public Type ComponentType {
            get => this.descriptor.ComponentType;
        }
        public void Clear() { }

        public ISO_Binding GetBinding(ISnoopObject target) {
            return null;
        }

        public void SetValue(ISnoopObject target, string value) {
            (target.Source as DependencyObject).OnUI(x => {
                this.descriptor.SetValue(target.Source, Convert.ChangeType(value, this.descriptor.PropertyType));
            });
        }

        public ISnoopObject GetValue(ISnoopObject target) {
            return Create((target.Source as DependencyObject).OnUI(x => this.descriptor.GetValue(target.Source)));
        }
    }
}