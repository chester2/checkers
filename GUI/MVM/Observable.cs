using System.Collections.Generic;
using System.ComponentModel;

namespace GUI
{
    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetProperty<T>(ref T property, T value, string propertyName)
        {
            if (!EqualityComparer<T>.Default.Equals(property, value))
            {
                property = value;
                this.RaisePropertyChanged(propertyName);
            }
        }
    }
}
