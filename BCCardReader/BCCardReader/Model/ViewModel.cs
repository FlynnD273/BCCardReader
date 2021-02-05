using BCCardReader.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCCardReader.Model
{
    class ViewModel : NotifyPropertyChangedBase
    {
        private string _textInput;
        public string TextInput
        {
            get { return _textInput; }
            set { _UpdateField(ref _textInput, value); }
        }

        private string _textDisplay;
        public string TextDisplay
        {
            get { return _textDisplay; }
            set { _UpdateField(ref _textDisplay, value); }
        }

        public DelegateCommand UpdateDisplayCommand { get; }

        public ViewModel()
        {
            UpdateDisplayCommand = new DelegateCommand(_UpdateDisplay);
        }

        private void _UpdateDisplay()
        {
            TextDisplay = TextInput;
        }
    }
}
