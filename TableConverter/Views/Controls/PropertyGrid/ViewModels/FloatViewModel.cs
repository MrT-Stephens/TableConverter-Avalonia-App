using System.ComponentModel;
using FastMember;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels
{
    public sealed class FloatViewModel : PropertyViewModelBase<float?>
    {
        public FloatViewModel(
            INotifyPropertyChanged viewmodel, 
            string displayName, 
            Member propertyInfo, 
            ObjectAccessor viewModelAccessor) 
            : base(viewmodel, displayName, propertyInfo, viewModelAccessor)
        {
        }
    }
}