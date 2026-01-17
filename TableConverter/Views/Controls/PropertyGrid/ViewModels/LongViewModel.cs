using System.ComponentModel;
using FastMember;

namespace TableConverter.Views.Controls.PropertyGrid.ViewModels
{
    public sealed class LongViewModel : PropertyViewModelBase<long?>
    {
        public LongViewModel(
            INotifyPropertyChanged viewmodel,
            string displayName,
            Member propertyInfo,
            ObjectAccessor viewModelAccessor)
            : base(viewmodel, displayName, propertyInfo, viewModelAccessor)
        {
        }
    }
}