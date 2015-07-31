using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SSCOUIModels;
using System.ComponentModel;
using System.Collections.ObjectModel;
using FPsxWPF.Controls;
using SSCOControls;
using System.Windows.Controls.Primitives;
using SSCOUIModels.Helpers;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Collections.Specialized;

namespace SSCOUIViews.Controls
{
    /// <summary>
    /// Interaction logic for AlphaNumericKeyboard.xaml
    /// </summary>
    public partial class AlphaNumericKeyboard : Grid
    {
        public AlphaNumericKeyboard()            
        {            
            InitializeComponent();
        }

        public void initKeyboardProperties()
        {   
            Line1KeysCollection = viewModel.GetPropertyValue("Line2AlphaNumericKeys") as ObservableCollection<GridItem>;
            Line2KeysCollection = viewModel.GetPropertyValue("Line3AlphaNumericKeys") as ObservableCollection<GridItem>;
            Line3KeysCollection = viewModel.GetPropertyValue("Line4AlphaNumericKeys") as ObservableCollection<GridItem>;
            Line4KeysCollection = viewModel.GetPropertyValue("Line1AlphaNumericKeys") as ObservableCollection<GridItem>;
            removeAddEvents();
        }
        
        public void retrieveCustomKeyboardProperties()
        {
            Line1AlphaNumKeys = UIChildFinder.FindVisualChild<UniformGrid>(Line1Keys, "Line1AlphaNumKeys") as UniformGrid;
            Line2AlphaNumKeys = UIChildFinder.FindVisualChild<UniformGrid>(Line2Keys, "Line2AlphaNumKeys") as UniformGrid;
            Line3AlphaNumKeys = UIChildFinder.FindVisualChild<UniformGrid>(Line3Keys, "Line3AlphaNumKeys") as UniformGrid;
            bindAllLineKeys();
        }        

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.viewModel = DataContext as IMainViewModel;            
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler tempHandler = EnterButtonClick;
            if (null != tempHandler)
            {
                tempHandler(sender, e);
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RoutedEventHandler tempHandler = InputTextBoxTextChanged;
            if (null != tempHandler)
            {
                tempHandler(sender, e);
            }
            else
            {
                handleBaseInputTextBox_TextChange();
            }            
        }

        private void handleBaseInputTextBox_TextChange()
        {
            this.BSButton.IsEnabled = (this.InputTextBox.Text.Length > 0);
            this.EnterOperatorButton.IsEnabled = (this.InputTextBox.Text.Length >= MinInput);
        }

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler tempHandler = KeyboardButtonClick;
            if (null != tempHandler)
            {
                tempHandler(sender, e);
            }
            else
            {
                handleBaseKeyboardButton_Click(sender);
            }
        }

        private void handleBaseKeyboardButton_Click(object sender)
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
                if (key != Key.None)
                {
                    textBox.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(textBox), 0, key) { RoutedEvent = Keyboard.KeyDownEvent });
                }                
            }
            else
            {
                textBox.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                    new TextComposition(InputManager.Current, textBox, button.Text)) { RoutedEvent = TextCompositionManager.TextInputEvent });
            }
        }

        private void ShiftButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleShift(ShiftButton.IsChecked.Value);
        }

        public void ToggleShift(bool isUpper)
        {
            object[] rows = { Line1AlphaNumKeys, Line2AlphaNumKeys, Line3AlphaNumKeys };
            
            for (int i = 0; i < rows.Length; i++)
            {
                foreach (ImageButton button in UIChildFinder.FindVisualChildren<ImageButton>(rows[i] as System.Windows.Controls.Primitives.UniformGrid))
                {
                    button.Text = isUpper ? button.Text.ToUpper() : button.Text.ToLower();
                }
            }
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            RoutedEventHandler tempHandler = InputTextBoxPreviewKeydown;
            if (null != tempHandler)
            {
                tempHandler(sender, e);
            }
            else
            {
                handleBaseInputTextBox_PreviewKeydown(sender, e);
            }
        }

        private void handleBaseInputTextBox_PreviewKeydown(object sender, KeyEventArgs e)
        {
            WatermarkTextBox textBox = Keyboard.FocusedElement as WatermarkTextBox;
            if (null == textBox)
            {
                return;
            }
            if (e.Key == Key.Enter && InputTextBox.Text.Length > 0)
            {
                invokeButtonClick(EnterOperatorButton);
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Space)
            {
                textBox.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                        new TextComposition(InputManager.Current, textBox, " ")) { RoutedEvent = TextCompositionManager.TextInputEvent });
                e.Handled = true;
                return;
            }            
        }

        private void invokeButtonClick(Button btn)
        {            
            ButtonAutomationPeer peer = new ButtonAutomationPeer(btn);
            IInvokeProvider invokeProvider = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            if (null != invokeProvider)
            {
                invokeProvider.Invoke();
            }            
        }

        private void Line1KeysCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {   
            if (e.Action == NotifyCollectionChangedAction.Reset || (e.Action == NotifyCollectionChangedAction.Replace && replaceBindValues(sender)))
            //if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                
                bindLineKeys(Line1KeysCollection, Line1Keys, Line1AlphaNumKeys, "AlphaNumP1", "Line1Alpha");
            }
        }
        
        private void Line2KeysCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                bindLineKeys(Line2KeysCollection, Line2Keys, Line2AlphaNumKeys, "AlphaNumP4", "");                
            }
        }
        
        private void Line3KeysCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                bindLineKeys(Line3KeysCollection, Line3Keys, Line3AlphaNumKeys, "AlphaNumP2", "");                
            }
        }

        private void Line4KeysCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                bindLineKeys(Line4KeysCollection, NumericLine1Keys, null, "AlphaNumP1", "Line1Numeric;Columns=3;Row=1");
                bindLineKeys(Line4KeysCollection, NumericLine2Keys, null, "AlphaNumP1", "Line1Numeric;Columns=3;Row=2");
                bindLineKeys(Line4KeysCollection, NumericLine3Keys, null, "AlphaNumP1", "Line1Numeric;Columns=3;Row=3");
                bindLineKeys(Line4KeysCollection, NumericLine4Keys, null, "AlphaNumP1", "Line1Numeric;Columns=3;Row=4");
            }
        }

        private bool replaceBindValues(object sender)
        {
            var gridItem = sender as ObservableCollection<GridItem>;
            if (null != gridItem && gridItem[gridItem.Count - 1].Text != null && gridItem[gridItem.Count - 1].Data != null)
            {
                return true;
            }
            return false;
        }

        private void bindAllLineKeys()
        {
            bindLineKeys(Line1KeysCollection, Line1Keys, Line1AlphaNumKeys, "AlphaNumP1", "Line1Alpha");
            bindLineKeys(Line2KeysCollection, Line2Keys, Line2AlphaNumKeys, "AlphaNumP4", "");
            bindLineKeys(Line3KeysCollection, Line3Keys, Line3AlphaNumKeys, "AlphaNumP2", "");
            bindLineKeys(Line4KeysCollection, NumericLine1Keys, null, "AlphaNumP1", "Line1Numeric;Columns=3;Row=1");
            bindLineKeys(Line4KeysCollection, NumericLine2Keys, null, "AlphaNumP1", "Line1Numeric;Columns=3;Row=2");
            bindLineKeys(Line4KeysCollection, NumericLine3Keys, null, "AlphaNumP1", "Line1Numeric;Columns=3;Row=3");
            bindLineKeys(Line4KeysCollection, NumericLine4Keys, null, "AlphaNumP1", "Line1Numeric;Columns=3;Row=4");
        }

        private void bindLineKeys(ObservableCollection<GridItem> items, ItemsControl lineKey, UniformGrid alphaNumKeys, string propertyName, string converterParam)
        {
            if (null != items)
            {
                MultiBinding mb = new MultiBinding();
                mb.ConverterParameter = converterParam;
                mb.Converter = new AlphaNumericKeysHandler();
                mb.Bindings.Add(new Binding() { Source = items });
                mb.Bindings.Add(new Binding() { Source = viewModel.GetPropertyValue(propertyName) });
                BindingOperations.SetBinding(lineKey, ItemsControl.ItemsSourceProperty, mb);
                if (null != alphaNumKeys)
                {
                    alphaNumKeys.Columns = (lineKey.ItemsSource as ObservableCollection<GridItem>).Count;
                }            
            }            
        }

        private void HotKeysKeyboardButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void removeAddEvents()
        {
            Line1KeysCollection.CollectionChanged -= Line1KeysCollection_CollectionChanged;
            Line1KeysCollection.CollectionChanged += Line1KeysCollection_CollectionChanged;
            Line2KeysCollection.CollectionChanged -= Line2KeysCollection_CollectionChanged;
            Line2KeysCollection.CollectionChanged += Line2KeysCollection_CollectionChanged;
            Line3KeysCollection.CollectionChanged -= Line3KeysCollection_CollectionChanged;
            Line3KeysCollection.CollectionChanged += Line3KeysCollection_CollectionChanged;
            Line4KeysCollection.CollectionChanged -= Line4KeysCollection_CollectionChanged;
            Line4KeysCollection.CollectionChanged += Line4KeysCollection_CollectionChanged;
        }

        public void removeEvents()
        {
            Line1KeysCollection.CollectionChanged -= Line1KeysCollection_CollectionChanged;
            Line2KeysCollection.CollectionChanged -= Line2KeysCollection_CollectionChanged;
            Line3KeysCollection.CollectionChanged -= Line3KeysCollection_CollectionChanged;
            Line4KeysCollection.CollectionChanged -= Line4KeysCollection_CollectionChanged;            
        }
        
        private int MinInput = 1;
        IMainViewModel viewModel;

        public event RoutedEventHandler KeyboardButtonClick;
        public event RoutedEventHandler EnterButtonClick;
        public event RoutedEventHandler InputTextBoxTextChanged;
        public event RoutedEventHandler InputTextBoxPreviewKeydown;
        private UniformGrid Line1AlphaNumKeys;
        private UniformGrid Line2AlphaNumKeys;
        private UniformGrid Line3AlphaNumKeys;
        private ObservableCollection<GridItem> Line1KeysCollection;
        private ObservableCollection<GridItem> Line2KeysCollection;
        private ObservableCollection<GridItem> Line3KeysCollection;
        private ObservableCollection<GridItem> Line4KeysCollection;

        
    }
}
