using System.ComponentModel;
using FastMember;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels
{
    public sealed class DoubleViewModel : PropertyViewModelBase<double?>
    {
        public DoubleViewModel(
            INotifyPropertyChanged viewmodel, 
            string displayName, 
            Member propertyInfo, 
            ObjectAccessor viewModelAccessor) 
            : base(viewmodel, displayName, propertyInfo, viewModelAccessor)
        {
        }
    }
}