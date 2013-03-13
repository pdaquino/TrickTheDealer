using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Input.Touch;

namespace TrickTheDealer
{
    public partial class MainPage : PhoneApplicationPage
    {
        private CardDeck deck = new CardDeck();
        CardSolver solver = new CardSolver();
        BruteForceSolver bruteForceSolver = new BruteForceSolver();
        private double defaultAggressiveness = 0.25;
        private const double minAggressiveness = 0.001;
        private const double maxAggressiveness = 0.999;

        private Stack<CardValue> actionStack = new Stack<CardValue>();
        private DragDropTracker dragTracker = null;
        private ICollection<ButtonFrame> buttonFrames = new List<ButtonFrame>();
        private StrategyEvaluator strategyEvaluator = null;
        private Strategy currentOptimalStrategy = null;
        private Strategy currentUnconstrainedStrategy = null;
        private bool isUnconstrainedAlwaysOptimal = false;
        private StrategyConstraint currentConstraint = new StrategyConstraint();
        private ExpectedDrinks currentExpectedDrinks = null;
        private ExpectedDrinks currentUnconstrainedDrinks = null;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            startNewGame();
            // HACK
            translateTransform = new TranslateTransform();
            myRectangle.RenderTransform = translateTransform;
        }

        private void startNewGame()
        {
            AggressivenessSlider.Value = defaultAggressiveness;
            AggressivenessSlider.Minimum = minAggressiveness;
            AggressivenessSlider.Maximum = maxAggressiveness;

            deck.Shuffle();
            DeckChanged();
            GetUndoButton().IsEnabled = false;
        }


        private void Card_Button_Click(object sender, RoutedEventArgs e)
        {
            Button cardButton = (Button)sender;
            CardValue cardValue = CardValueFromButton(cardButton);
            Card selectedCard = deck.Cards.Where(x => x.Value.Equals(cardValue)).First();
            if (selectedCard.NumberLeft > 0)
            {
                selectedCard.CardDrawn();
                DeckChanged();
                actionStack.Push(cardValue);
                GetUndoButton().IsEnabled = true;
            }
        }

        private static CardValue CardValueFromButton(Button cardButton)
        {
            string name = cardButton.Name;
            int numericValue = int.Parse(name.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[1]);
            CardValue cardValue = new CardValue(numericValue);
            return cardValue;
        }

        private ApplicationBarMenuItem GetUndoButton()
        {
            return (ApplicationBarMenuItem)ApplicationBar.MenuItems[0];
        }
        //private void computeOptimalStrategy()
        //{
        //    Strategy strategy = solver.FindOptimalStrategy(deck);
        //    this.CardMiddle.Content = strategy.middleChoice.Name;
        //    this.CardLower.Content = strategy.lowerChoice.Name;
        //    this.CardHigher.Content = strategy.higherChoice.Name;
        //}

        private void DeckChanged()
        {
            currentConstraint.Reset();
            strategyEvaluator = new StrategyEvaluator(bruteForceSolver.GetAllStrategies(), deck.CardsLikelihoods());
            refreshDeckCount();
            computeOptimalStrategy(GetAggressiveness());
            DisplayExpectedDrinks();
            DrinksPivot.SelectedIndex = 1;
        }

        private void AggressivenessChanged()
        {
            computeOptimalStrategy(GetAggressiveness());
            DisplayExpectedDrinks();
        }

        private void ConstraintsChanged()
        {
            computeOptimalStrategy(GetAggressiveness());
            DisplayExpectedDrinks();
        }

        private void computeOptimalStrategy()
        {
            computeOptimalStrategy(AggressivenessSlider.Value);
        }

        private void computeOptimalStrategy(double aggressiveness)
        {
            if (strategyEvaluator == null) return;
            currentOptimalStrategy = strategyEvaluator.GetBestStrategyWithConstraints(currentConstraint, aggressiveness);
            currentExpectedDrinks = strategyEvaluator.GetExpectedDrinks(currentOptimalStrategy);
            if (currentConstraint.AreAllNull())
            {
                currentUnconstrainedStrategy = currentOptimalStrategy;
                currentUnconstrainedDrinks = currentExpectedDrinks;
            }
            
            bool isAlwaysOptimal = IsCurrentOptimalStable() &&
                (currentConstraint.IsSatisfiedBy(currentUnconstrainedStrategy));
            if (currentConstraint.AreAllNull())
            {
                isUnconstrainedAlwaysOptimal = isAlwaysOptimal;
            }
            paintStrategy(currentOptimalStrategy, isAlwaysOptimal);
        }

        private bool IsCurrentOptimalStable()
        {
            Strategy lowAggressivenessStrategy = strategyEvaluator.GetBestStrategy(minAggressiveness);
            Strategy highAggressivenessStrategy = strategyEvaluator.GetBestStrategy(maxAggressiveness);
            return lowAggressivenessStrategy.Equals(highAggressivenessStrategy);
        }

        private void paintStrategy(Strategy strategy, bool isAlwaysOptimal)
        {
            Color defaultBackgroundColor = (Color)Resources["PhoneBackgroundColor"];
            Color defaultBorderColor = (Color)Resources["PhoneBorderColor"];
            Color defaultForegroundColor = (Color)Resources["PhoneForegroundColor"];
            for (int card = CardDeck.kLowestCardValue; card <= CardDeck.kHighestCardValue; card++)
            {
                Button cardButton = findCardButton(new CardValue(card));
                if (cardButton.IsEnabled)
                {
                    cardButton.BorderBrush = new SolidColorBrush(defaultBorderColor);
                    cardButton.Background = new SolidColorBrush(defaultBackgroundColor);
                    cardButton.Foreground = new SolidColorBrush(defaultForegroundColor);
                    cardButton.Opacity = 1.0;
                }
            }
            Color accentColor = (Color)Resources["PhoneAccentColor"];
            Color goldColor = Color.FromArgb(255, 212, 175, 55);
            Color strategyColor = accentColor;
            if (isAlwaysOptimal)
            {
                strategyColor = goldColor ; // gold
            }
            paintMiddleChoice(strategy.middleChoice, strategyColor);
            paintSecondChoices(strategy.lowerChoice, strategy.middleChoice, strategyColor);
            paintSecondChoices(strategy.higherChoice, strategy.middleChoice, strategyColor);

            if (currentConstraint.MiddleChoice != null
                && !currentConstraint.MiddleChoice.Equals(currentUnconstrainedStrategy.middleChoice)
                && !currentUnconstrainedStrategy.middleChoice.Equals(strategy.lowerChoice)
                && !currentUnconstrainedStrategy.middleChoice.Equals(strategy.higherChoice))
            {
                Color fadedColor = isUnconstrainedAlwaysOptimal ? goldColor : accentColor;
                paintFadedButton(currentUnconstrainedStrategy.middleChoice, fadedColor);
            }

            //UserDrinksLabel.Text = String.Format("{0:.##}", strategy.GetExpectedUserDrinks(deck.CardsLikelihoods()));
            //DealerDrinksLabel.Text = String.Format("{0:.##}", strategy.GetExpectedDealerDrinks(deck.CardsLikelihoods()));
        }

        private void paintFadedButton(CardValue cardValue, Color color)
        {
            Button middleButton = findCardButton(cardValue);
            if (middleButton.IsEnabled)
            {
                middleButton.Background = new SolidColorBrush(color);
                middleButton.Opacity = 0.4;
            }
        }

        private void paintMiddleChoice(CardValue value, Color color)
        {
            Button middleButton = findCardButton(value);
            if (middleButton.IsEnabled)
            {
                middleButton.Background = new SolidColorBrush(color);
            }
        }

        private void paintSecondChoices(CardValue value, CardValue middleChoice, Color color)
        {
            Button button = findCardButton(value);
            if (button.IsEnabled && value.Value != middleChoice.Value)
            {
                button.BorderBrush = new SolidColorBrush(color);
                button.Foreground = new SolidColorBrush(color);
            }
        }

        private Button findCardButton(CardValue value)
        {
            return (Button)this.FindName("Card_" + value.Value);
        }

        
        private void refreshDeckCount()
        {
            foreach (Card card in deck.Cards)
            {
                string cardCountFieldName = "Card" + card.Value.Value + "_DrawnCount";
                TextBlock countBlock = (TextBlock)this.FindName(cardCountFieldName);
                countBlock.Text = card.NumberDealt.ToString();
                Button cardButton = (Button)this.FindName("Card_" + card.Value.Value);
                if (card.NumberLeft == 0)
                {
                    Color disabledColor = (Color)Resources["PhoneDisabledColor"];
                    cardButton.IsEnabled = false;
                    //cardButton.Opacity = 0.7;
                    //countBlock.Opacity = 0.7;
                    cardButton.Foreground = new SolidColorBrush(disabledColor);
                }
                else
                {
                    cardButton.IsEnabled = true;
                    cardButton.Opacity = 1.0;
                    countBlock.Opacity = 1.0;
                }
            }
        }

        private void AggressivenessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.AggressivenessSlider != null)
            {
                //computeOptimalStrategy(GetAggressiveness());
                AggressivenessChanged();
            }
            if (this.AgressivenessLabel != null)
            {
                this.AgressivenessLabel.Text = "Aggressiveness: " + Math.Round(e.NewValue * 100) + "%";
            }
        }

        private double GetAggressiveness()
        {
            return this.AggressivenessSlider.Value;
        }

        private void CardLower_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            CardValue value = actionStack.Pop();
            Card card = deck.Cards.Where(x => x.Value.Equals(value)).First();
            card.UndoDraw();
            DeckChanged();
            if (actionStack.Count == 0)
            {
                GetUndoButton().IsEnabled = false;
            }
        }

        private void NewGameButton_Click(object sender, EventArgs e)
        {
            MessageBoxResult result = 
                MessageBox.Show("Reset all cards and start a new game?", "New Game", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                startNewGame();
            }
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/HelpPage", UriKind.Relative));
        }

        private void GestureListener_DragCompleted_1(object sender, DragCompletedGestureEventArgs e)
        {
            //MessageBox.Show("completed");
        }

        private TranslateTransform translateTransform;
        private void GestureListener_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            if (dragTracker.Invalid)
            {
                return;
            }
            //translateTransform.X += e.HorizontalChange;
            //translateTransform.Y += e.VerticalChange;
            dragTracker.DragDelta(e.HorizontalChange, e.VerticalChange);
            
            // HACK
            Canvas.SetLeft(myRectangle, (int)Math.Round(dragTracker.Rect.Left));
            Canvas.SetTop(myRectangle, (int)Math.Round(dragTracker.Rect.Top));
            if (dragTracker.Intersects())
            {
                Button intersectingButton = dragTracker.GetIntersectingButton().Button;
                if (intersectingButton.IsEnabled)
                {
                    CardValue intersectingValue = CardValueFromButton(intersectingButton);
                    if (dragTracker.StartingCardMiddleChoice)
                    {
                        ForceMiddleChoice(intersectingValue);
                    }
                    else if (dragTracker.StartingCardLowerChoice)
                    {
                        ForceLowerChoice(intersectingValue);
                    }
                    else if (dragTracker.StartingCardHigherChoice)
                    {
                        ForceHigherChoice(intersectingValue);
                    }
                }
                //intersectingButton.BorderBrush = new SolidColorBrush(Colors.Purple);
            }
        }

        private void ForceLowerChoice(CardValue lowerChoice)
        {
            if (lowerChoice.Value < currentOptimalStrategy.middleChoice.Value)
            {
                currentConstraint.MiddleChoice = currentOptimalStrategy.middleChoice;
                currentConstraint.LowerChoice = lowerChoice;
                currentConstraint.HigherChoice = currentOptimalStrategy.higherChoice;
            }
            ConstraintsChanged();
        }

        private void ForceHigherChoice(CardValue higherChoice)
        {
            if (higherChoice.Value > currentOptimalStrategy.middleChoice.Value)
            {
                currentConstraint.MiddleChoice = currentOptimalStrategy.middleChoice;
                currentConstraint.HigherChoice = higherChoice;
                currentConstraint.LowerChoice = currentOptimalStrategy.lowerChoice;
            }
            ConstraintsChanged();
        }

        private void ForceMiddleChoice(CardValue middleChoice)
        {
            //Strategy optimalStrategy = stra
            currentConstraint.Reset();
            if (middleChoice.Value != currentUnconstrainedStrategy.middleChoice.Value)
            {
                currentConstraint.MiddleChoice = middleChoice;
            }
            ConstraintsChanged();
        }

        private void GestureListener_DragStarted_1(object sender, DragStartedGestureEventArgs e)
        {
            if (buttonFrames.Count == 0)
            {
                GetButtonFrames();
            }
            Button senderButton = sender as Button;

            Rect senderFrame = GetButtonFrame(senderButton);
            // HACK: change true
            dragTracker = new DragDropTracker(senderFrame, buttonFrames);
            CardValue senderValue = CardValueFromButton(senderButton);
            if (senderValue.Value == currentOptimalStrategy.middleChoice.Value)
            {
                dragTracker.StartingCardMiddleChoice = true;
            }
            else if (senderValue.Value == currentOptimalStrategy.lowerChoice.Value)
            {
                dragTracker.StartingCardLowerChoice = true;
            }
            else if (senderValue.Value == currentOptimalStrategy.higherChoice.Value)
            {
                dragTracker.StartingCardHigherChoice = true;
            }
            else
            {
                dragTracker.Invalid = true;
            }
            // HACK
            Canvas.SetLeft(myRectangle,(int)Math.Round(senderFrame.X));
            Canvas.SetTop(myRectangle, (int)Math.Round(senderFrame.Y));
            myRectangle.Width = senderFrame.Width;
            myRectangle.Height = senderFrame.Height;
        }

        private void GetButtonFrames()
        {
            for (int value = CardDeck.kLowestCardValue; value <= CardDeck.kHighestCardValue; value++)
            {
                Button button = findCardButton(new CardValue(value));
                this.buttonFrames.Add(new ButtonFrame() { Button = button, Frame = GetButtonFrame(button) });
            }
        }

        private Rect GetButtonFrame(Button button)
        {
            Point topLeft = button.TransformToVisual(ContentPanel).Transform(new Point(0, 0));
            return new Rect(topLeft.X, topLeft.Y, button.Width, button.Height);
        }

        GestureType prevGestureType;
        private void disableGestures(object sender, ManipulationStartedEventArgs e)
        {
            prevGestureType = TouchPanel.EnabledGestures;
            TouchPanel.EnabledGestures = GestureType.None;
            //AggressivenessSlider.IsHitTestVisible = false;
        }

        private void restoreGestures(object sender, ManipulationCompletedEventArgs e)
        {
            TouchPanel.EnabledGestures = prevGestureType;
            //AggressivenessSlider.IsHitTestVisible = true;
        }

        private void DrinksPivot_ItemLoaded(object sender, PivotItemEventArgs e)
        {
            Color disabledColor = (Color)Resources["PhoneDisabledColor"];
            Color enabledColor = (Color)Resources["PhoneForegroundColor"];
            totalDrinksLabel.Foreground = new SolidColorBrush(disabledColor);
            lowerDrinksLabel.Foreground = new SolidColorBrush(disabledColor);
            higherDrinksLabel.Foreground = new SolidColorBrush(disabledColor);

            if (e.Item == MiddleItem)
            {
                totalDrinksLabel.Foreground = new SolidColorBrush(enabledColor);
            }
            if (e.Item == LowerItem)
            {
                lowerDrinksLabel.Foreground = new SolidColorBrush(enabledColor);
            }
            if (e.Item == HigherItem)
            {
                higherDrinksLabel.Foreground = new SolidColorBrush(enabledColor);
            }
            DisplayExpectedDrinks(e.Item);
        }

        private void DisplayExpectedDrinks()
        {
            DisplayExpectedDrinks(DrinksPivot.SelectedItem as PivotItem);
        }

        private void DisplayExpectedDrinks(PivotItem loadedItem)
        {
            if (loadedItem == MiddleItem)
            {
                CardValue lowestRemaining = deck.LowestRemainingCard();
                if (lowestRemaining == null)
                {
                    DisplayImpossibleDrinks(PlayerDrinksText_Total.Inlines, DealerDrinksText_Total.Inlines);
                }
                else
                {
                    DisplayExpectedDrinks(PlayerDrinksText_Total.Inlines,
                        DealerDrinksText_Total.Inlines,
                        currentExpectedDrinks.UserDrinks,
                        currentExpectedDrinks.DealerDrinks,
                        currentUnconstrainedDrinks.UserDrinks,
                        currentUnconstrainedDrinks.DealerDrinks);
                }
            }
            if (loadedItem == LowerItem)
            {
                CardValue lowestRemaining = deck.LowestRemainingCard();
                if (lowestRemaining == null || currentOptimalStrategy.middleChoice.Equals(lowestRemaining))
                {
                    DisplayImpossibleDrinks(PlayerDrinksText_Lower.Inlines, DealerDrinksText_Lower.Inlines);
                }
                else
                {
                    ExpectedDrinks unconstrainedConditionalDrinks = ExpectedDrinks.ComputeDrinksIfLower(
                        currentUnconstrainedStrategy, currentOptimalStrategy.middleChoice, deck.CardsLikelihoods());
                    DisplayExpectedDrinks(
                        PlayerDrinksText_Lower.Inlines,
                        DealerDrinksText_Lower.Inlines,
                        currentExpectedDrinks.UserDrinksIfLower,
                        currentExpectedDrinks.DealerDrinksIfLower,
                        unconstrainedConditionalDrinks.UserDrinks ,
                        unconstrainedConditionalDrinks.DealerDrinks);
                }
            }
            if (loadedItem == HigherItem)
            {
                CardValue highestRemaining = deck.HighestRemainingCard();
                if (highestRemaining == null || currentOptimalStrategy.middleChoice.Equals(highestRemaining))
                {
                    DisplayImpossibleDrinks(PlayerDrinksText_Higher.Inlines, DealerDrinksText_Higher.Inlines);
                }
                else
                {
                    ExpectedDrinks unconstrainedConditionalDrinks = ExpectedDrinks.ComputeDrinksIfHigher(
                        currentUnconstrainedStrategy, currentOptimalStrategy.middleChoice, deck.CardsLikelihoods());
                    DisplayExpectedDrinks(
                        PlayerDrinksText_Higher.Inlines,
                        DealerDrinksText_Higher.Inlines,
                        currentExpectedDrinks.UserDrinksIfHigher,
                        currentExpectedDrinks.DealerDrinksIfHigher,
                        unconstrainedConditionalDrinks.UserDrinks,
                        unconstrainedConditionalDrinks.DealerDrinks);
                }
            }
        }

        private void DisplayImpossibleDrinks(InlineCollection playerInlines, InlineCollection dealerInlines)
        {
            playerInlines.Clear();
            playerInlines.Add(new Run()
            {
                Text = "-"
            });
            dealerInlines.Clear();
            dealerInlines.Add(new Run()
            {
                Text = "-"
            });
        }

        private void DisplayExpectedDrinks(InlineCollection playerInlines, InlineCollection dealerInlines,
            double currentUserDrinks, double currentDealerDrinks,
            double unconstrainedUserDrinks, double unconstrainedDealerDrinks)
        {
            playerInlines.Clear();
            playerInlines.Add(new Run()
            {
                Text = String.Format("{0:0.##}", currentUserDrinks)
            });
            dealerInlines.Clear();
            dealerInlines.Add(new Run()
            {
                Text = String.Format("{0:0.##}", currentDealerDrinks)
            });

            if (!currentConstraint.IsSatisfiedBy(currentUnconstrainedStrategy))
            {
                AddComparisonToOptimal(playerInlines, currentUserDrinks,
                    unconstrainedUserDrinks, true);
                AddComparisonToOptimal(dealerInlines, currentDealerDrinks,
                    unconstrainedDealerDrinks, false);
            }
        }

        private void AddComparisonToOptimal(InlineCollection inlines, double current, double optimal, bool smallIsGood)
        {
            double difference = current - optimal;
            Color color = new Color();
            if (difference < 0)
            {
                color = smallIsGood ? Colors.Green : Colors.Red;
            }
            else
            {
                color = smallIsGood ? Colors.Red : Colors.Green;
            }
            if (Math.Abs(difference) > 0.004)
            {
                inlines.Add(new Run() { Text = " (" });
                inlines.Add(new Run() { Text = difference.ToString("+0.##;-0.##"), Foreground = new SolidColorBrush(color) });
                inlines.Add(new Run() { Text = ")" });
            }
        }

    }
}