using System;
using System.ComponentModel;
using HgLib;

namespace VisualHg.ViewModel
{
    public class PendingChange : INotifyPropertyChanged
    {
        private ComparableStatus _status;


        public string ShortName { get; set; }
        
        public string RootName { get; set; }

        public ComparablePath Name { get; set; }

        public ComparablePath FullName { get; set; }

        public ComparableStatus Status
        {
            get => _status;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Status");
                }

                if (_status == null || _status.Value != value.Value)
                {
                    _status = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };


        public PendingChange() { }

        public PendingChange(HgFileInfo file)
        {
            ShortName = file.ShortName;
            RootName = file.RootName;
            Status = file.Status;
            Name = file.Name;
            FullName = file.FullName;
        }


        public bool Equals(HgFileInfo file)
        {
            return StringComparer.InvariantCultureIgnoreCase.Equals((string)FullName, file.FullName);
        }
    }
}