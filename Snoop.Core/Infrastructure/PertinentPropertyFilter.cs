// (c) Copyright Cory Plotts.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Snoop.Infrastructure
{
    using System.ComponentModel;
    using System.Windows;
    using Snoop.DataAccess.Interfaces;
    using Snoop.DataAccess.Sessions;

    public static class PertinentPropertyFilter
    {
        public static bool Filter(ISnoopObject target, ISO_PropertyDescriptor property)
        {
            var frameworkElement = target as ISO_FrameworkElement;

            if (frameworkElement == null)
            {
                return true;
            }
            var attachedPropertyForChildren = (AttachedPropertyBrowsableForChildrenAttribute)property.Attributes[typeof(AttachedPropertyBrowsableForChildrenAttribute)];

            if (attachedPropertyForChildren != null) {

                var dpd = ExtensionLocator.From(target).Get<IDAS_TypeDescriptor>().GetDependencyPropertyDescriptor(property);
                if (dpd == null)
                {
                    return false;
                }

                var currentElement = frameworkElement;
                do
                {
                    currentElement = currentElement.Parent as ISO_FrameworkElement;
                    if (currentElement != null
                        && dpd.ComponentType.IsInstanceOfType(currentElement.Source))
                    {
                        return true;
                    }
                } 
                while (attachedPropertyForChildren.IncludeDescendants && currentElement != null);

                return false;
            }
            //
            // var attachedPropertyForType = (AttachedPropertyBrowsableForTypeAttribute)property.Attributes[typeof(AttachedPropertyBrowsableForTypeAttribute)];
            //
            // if (attachedPropertyForType != null)
            // {
            //     // when using [AttachedPropertyBrowsableForType(typeof(IMyInterface))] and IMyInterface is not a DependencyObject, Snoop crashes.
            //     // see http://snoopwpf.codeplex.com/workitem/6712
            //
            //     if (typeof(DependencyObject).IsAssignableFrom(attachedPropertyForType.TargetType))
            //     {
            //         var doType = DependencyObjectType.FromSystemType(attachedPropertyForType.TargetType);
            //         if (doType != null
            //             && doType.IsInstanceOfType(frameworkElement.Source))
            //         {
            //             return true;
            //         }
            //     }
            //
            //     return false;
            // }

            var attachedPropertyForAttribute = (AttachedPropertyBrowsableWhenAttributePresentAttribute)property.Attributes[typeof(AttachedPropertyBrowsableWhenAttributePresentAttribute)];

            if (attachedPropertyForAttribute != null)
            {
                var dependentAttribute = TypeDescriptor.GetAttributes(target)[attachedPropertyForAttribute.AttributeType];
                if (dependentAttribute != null)
                {
                    return !dependentAttribute.IsDefaultAttribute();
                }

                return false;
            }

            return true;
        }
    }
}