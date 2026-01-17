using System.ComponentModel;
using FastMember;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels
{
    public sealed class BoolViewModel : PropertyViewModelBase<bool?>
    {
        public BoolViewModel(
            INotifyPropertyChanged viewmodel, 
            string displayName, 
            Member propertyInfo, 
            ObjectAccessor viewModelAccessor) 
            : base(viewmodel, displayName, propertyInfo, viewModelAccessor)
        {
        }
    }
}