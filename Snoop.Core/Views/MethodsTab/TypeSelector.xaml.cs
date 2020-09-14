﻿// (c) Copyright Cory Plotts.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Snoop.Views.MethodsTab
{
    using System;
    using System.Collections.Generic;

    public partial class TypeSelector : ITypeSelector
    {
        public TypeSelector()
        {
            this.InitializeComponent();

            this.Loaded += this.TypeSelector_Loaded;
        }

        public static List<Type> GetDerivedTypes(Type baseType)
        {
            var typesAssignable = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (baseType.IsAssignableFrom(type))
                    {
                        typesAssignable.Add(type);
                    }
                }
            }

            if (!baseType.IsAbstract)
            {
                typesAssignable.Add(baseType);
            }

            typesAssignable.Sort(new TypeComparerByName());

            return typesAssignable;
        }

        public List<Type> DerivedTypes { get;  set; }

        private void TypeSelector_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DerivedTypes == null)
            {
                this.DerivedTypes = GetDerivedTypes(this.BaseType);
            }

            this.comboBoxTypes.ItemsSource = this.DerivedTypes;
        }

        public Type BaseType { get; set; }

        public object Instance
        {
            get;
            private set;
        }

        private void ButtonCreateInstance_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Instance = Activator.CreateInstance((Type)this.comboBoxTypes.SelectedItem);
            this.Close();
        }

        private void ButtonCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}