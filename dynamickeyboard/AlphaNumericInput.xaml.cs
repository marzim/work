using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SSCOUIModels.Controls;
using FPsxWPF.Controls;
using SSCOUIModels;
using SSCOControls;
using System.ComponentModel;
using System.Security;
using System.Globalization;

namespace SSCOUIViews.Views
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class AlphaNumericInput : PopupView
    {
        public AlphaNumericInput(IMainViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
            this.GenericAlphaNumericKeyboard.initKeyboardProperties();
            removeAddEvents();
        }
         
        /// <summary>
        /// Keyboard click events
        /// </summary>
        /// <param name="sender">This is a parameter with a type of object</param>
        /// <param name="e">This is a parameter with a type of RoutedEventArgs</param>
        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            WatermarkTextBox textBox = Keyboard.FocusedElement as WatermarkTextBox;
            if (null == textBox)
            {
                return;
            }
            ImageButton button = sender as ImageButton;
            if (null != button.CommandParameter)
            {
                Key key = Key.None;
                try
                {
                    key = (Key)new KeyConverter().ConvertFromString(button.CommandParameter.ToString());
                }
                catch (InvalidCastException)
                {
                }
                if (key != Key.None && key != Key.Space)
                {
                    textBox.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(textBox), 0, key) { RoutedEvent = Keyboard.KeyDownEvent });
                }
                else if (key == Key.Space)
                {
                    textBox.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, textBox, " ")) { RoutedEvent = TextCompositionManager.TextInputEvent });
                }
            }
            else
            {
                textBox.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, textBox, button.Text)) { RoutedEvent = TextCompositionManager.TextInputEvent });
            }
        }

        /// <summary>
        /// Preview KeyDown events to have our own handling for Space key
        /// </summary>
        /// <param name="sender">This is a parameter with a type of object</param>
        /// <param name="e">This is a parameter with a type of KeyEventArgs</param>
        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            WatermarkTextBox textBox = Keyboard.FocusedElement as WatermarkTextBox;
            if (null == textBox)
            {
                return;
            }
            if (e.Key == Key.Space)
            {
                textBox.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, textBox, " ")) { RoutedEvent = TextCompositionManager.TextInputEvent });
                e.Handled = true;
            }
        }

        
        public override void Show(bool isShowing)
        {
            if (isShowing)
            {
                this.GenericAlphaNumericKeyboard.retrieveCustomKeyboardProperties();
                SetFocus(GenericAlphaNumericKeyboard.InputTextBox);
                this.GenericAlphaNumericKeyboard.ShiftButton.IsChecked = false;
                this.GenericAlphaNumericKeyboard.ToggleShift(false);
                this.GenericAlphaNumericKeyboard.InputTextBox.Clear();
                this.GenericAlphaNumericKeyboard.InputTextBox.PasswordMode = false;
            }
            else
            {
                this.GenericAlphaNumericKeyboard.removeEvents();
                this.GenericAlphaNumericKeyboard.InputTextBox.PreviewKeyDown -= InputTextBox_PreviewKeyDown;
                this.GenericAlphaNumericKeyboard.EnterOperatorButton.Click -= EnterButton_Click;
                this.GenericAlphaNumericKeyboard.KeyboardButtonClick -= KeyboardButton_Click;                
            }
            base.Show(isShowing);
        }

        /// <summary>
        /// OnStateParamChanged that accepts param that is set in app.config
        /// </summary>
        /// <param name="param">String type of param.</param>
        public override void OnStateParamChanged(string param)
        {
            this.Instructions.Visibility = contextForInstructionControlShow.Contains(param) ? Visibility.Visible : Visibility.Collapsed;
            UpdateText("Leadthrutext", this.TitleControl);
            UpdateText("MessageBoxNoEcho", this.Message);
            UpdateText("InstructionScreenTitle", this.Instructions);
            
            if (param.Equals("OperatorKeyboard"))
            {
                this.Instructions.Text = SSCOUIStrings.Properties.Resources.EnterIDText;
                this.Message.Text = SSCOUIStrings.Properties.Resources.EnterIDMessage;                    
            }
            this.TitleControl.Visibility = contextForTitleControlShow.Contains(param) ? 
                System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// EnterButton click events
        /// </summary>
        /// <param name="sender">This is a parameter with a type of object</param>
        /// <param name="e">This is a parameter with a type of RoutedEventArgs</param>
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            string commandParam = String.Empty;
            if (null != viewModel.StateParam)
            {
                switch (viewModel.StateParam)
                {
                    case "Keyboard0409":
                    case "OperatorKeyboard":
                        commandParam = "EnterKeyboard({0})";
                        break;
                    case "ExtendedAlphaNumeric":
                        commandParam = "EnterAlphaNum({0}?@?AtSignKey?.?KeyBoardP3?-?MinusKey)";
                        break;
                    default:
                        commandParam = "EnterAlphaNum({0})";
                        break;
                }
            }
            this.GenericAlphaNumericKeyboard.EnterOperatorButton.CommandParameter = String.Format(commandParam, this.GenericAlphaNumericKeyboard.InputTextBox.PasswordMode ? this.GenericAlphaNumericKeyboard.InputTextBox.PasswordText : this.GenericAlphaNumericKeyboard.InputTextBox.Text);
            this.GenericAlphaNumericKeyboard.InputTextBox.Text = string.Empty;
        }

        public override void OnPropertyChanged(string name, object value)
        {
            if (name.Equals("NextGenData") && viewModel.StateParam.Equals("OperatorKeyboard"))
            {
                SetFocus(GenericAlphaNumericKeyboard.InputTextBox);
                GenericAlphaNumericKeyboard.InputTextBox.Clear();                
                if (value.ToString().Equals("EnterPassword"))
                {
                    this.Instructions.Text = SSCOUIStrings.Properties.Resources.EnterPasswordText;
                    Message.Text = SSCOUIStrings.Properties.Resources.EnterPasswordMessage;
                    GenericAlphaNumericKeyboard.InputTextBox.PasswordMode = true;
                }
                else if (value.ToString().Equals("EnterID"))
                {
                    this.Instructions.Text = SSCOUIStrings.Properties.Resources.EnterIDText;
                    Message.Text = SSCOUIStrings.Properties.Resources.EnterIDMessage;
                    GenericAlphaNumericKeyboard.InputTextBox.PasswordMode = false;
                }
            }
            else if (name.Equals("Leadthrutext"))
            {
                UpdateText(name, this.TitleControl);
            }
            else if (name.Equals("MessageBoxNoEcho"))
            {
                UpdateText(name, this.Message);
            }
            else if (name.Equals("InstructionScreenTitle"))
            {
                UpdateText(name, this.Instructions);
            }
            else if (name.Equals("UIEchoField"))
            {
                MinInput = viewModel.UIEchoField.MinLength;
                GenericAlphaNumericKeyboard.InputTextBox.MaxLength = viewModel.UIEchoField.MaxLength;
            }
        }

        private void UpdateText(string property, TextBlock tb)
        {
            string propertyText = GetPropertyStringValue(property);

            if (propertyText.Length > 0)
            {
                string[] headerText = propertyText.Split(':');
                tb.Text = headerText[0];
                if (headerText.Length > 1)
                {
                    tb.Text = tb.Text + ":\n" + headerText[1];
                }
            }
        }
        
        private void removeAddEvents()
        {        
            this.GenericAlphaNumericKeyboard.KeyboardButtonClick -= KeyboardButton_Click;
            this.GenericAlphaNumericKeyboard.KeyboardButtonClick += KeyboardButton_Click;
            this.GenericAlphaNumericKeyboard.EnterButtonClick -= EnterButton_Click;
            this.GenericAlphaNumericKeyboard.EnterButtonClick += EnterButton_Click;
            this.GenericAlphaNumericKeyboard.InputTextBox.PreviewKeyDown -= InputTextBox_PreviewKeyDown;            
            this.GenericAlphaNumericKeyboard.InputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;            
        }

        private int MinInput = 1;

        private static List<string> contextForTitleControlShow = new List<string>() { "AlphaNumeric", "ExtendedAlphaNumeric", 
            "AlphaNumericPassword" };

        private static List<string> contextForInstructionControlShow = new List<string>() { "Keyboard0409", "OperatorKeyboard", 
            "AlphaNumericPassword", "Keyboard041f", "Keyboard0c0c", "Keyboard080a", "Keyboard0424", "Keyboard040c" };
    } 
}
