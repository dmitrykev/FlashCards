using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;

namespace FlashCards
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DbContextOptionsBuilder<FlashCardsContext> optionsBuilder;
        private DbContextOptions<FlashCardsContext> options;

        public MainWindow()
        {
            InitializeComponent();

            // Get connection string from the appsettings.json file
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            string connectionString = config.GetConnectionString("DefaultConnection");

            // Building options
            optionsBuilder = new DbContextOptionsBuilder<FlashCardsContext>();
            options = optionsBuilder.UseSqlServer(connectionString).Options;

            LoadLibrary();
            
        }

        public void LoadLibrary()
        {
            using (FlashCardsContext db = new FlashCardsContext(options))
            {
                if (!db.Database.CanConnect())
                    throw new Exception("Database is not available");
                List<Card> items = new List<Card>();
                var cards = db.Cards.ToList();
                foreach (var card in cards)
                {
                    items.Add(new Card() { ID = card.ID, Word = card.Word, Translate = card.Translate });
                }

                Labrary.ItemsSource = items;

                if (textBlockCards1.Text == "")
                {
                    textBlockCards1.Text = items[0].Word;
                    textBlockCards2.Text = items[0].Translate;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (textBoxAdd1.Text != "" && textBoxAdd2.Text != "")
            {
                // форматирование вводимых строк "Такое"
                string subWord = textBoxAdd1.Text.Substring(0, 1).ToUpper() + textBoxAdd1.Text.Substring(1).ToLower();
                string subTranslate = textBoxAdd2.Text.Substring(0, 1).ToUpper() + textBoxAdd2.Text.Substring(1).ToLower();
                // INSERT INTO CardsDB
                using (FlashCardsContext db = new FlashCardsContext(options))
                {
                    // создаем экземпляр класса и описываем его свойства
                    Card card = new Card { Word = subWord, Translate = subTranslate };

                    // добавляем объект в коллекцию (таблицу), которую ранее описали в CardsContext.cs
                    
                    try
                    {
                        db.Cards.Add(card);
                        db.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show("This card already exists.");
                    }
                }

                textBoxAdd1.Text = textBoxAdd2.Text = null;
                LoadLibrary();
            }
            else
                MessageBox.Show("Fields must not be empty!");

        }
        
        private void btnTrash_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            Card? card = button.DataContext as Card;
            //int? id = card.ID;

            /*Button btn = ((Button)sender);
            object elementToDelete = btn.DataContext;*/

            if (card != null)
            {
                using (FlashCardsContext db = new FlashCardsContext(options))
                {
                    //var cards = db.Cards.FirstOrDefault(x => x.ID == elementToDelete.);
                    db.Cards.Remove(card);
                    db.SaveChanges();
                }
            }
            else
            {
                MessageBox.Show("A deletion error has occurred.");
            }
            //textBoxSearch.Text == "" ? LoadLibrary(): ;
            LoadLibrary();
            textBoxSearch.Text = "";
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = textBoxSearch.Text.ToLower();
            
            using (FlashCardsContext db = new FlashCardsContext(options))
            {
                List<Card> items = new List<Card>();
                var cards = db.Cards.ToList();
                foreach(var card in cards)
                {
                    if (card.Word.ToLower().Contains(searchText) || card.Translate.ToLower().Contains(searchText))
                    {
                        items.Add(new Card() { ID = card.ID, Word = card.Word, Translate = card.Translate });
                    }
                    
                }
                Labrary.ItemsSource = items;
            }
        }

        int cardCounter = 0;
        private void GetNextCard()
        {
            using (FlashCardsContext db = new FlashCardsContext(options))
            {
                var cards = db.Cards.ToList();

                RefreshCards:
                if (cardCounter < cards.Count)
                {
                    if (cards[cardCounter].Show > 0)
                    {
                        textBlockCards1.Text = cards[cardCounter].Word;
                        textBlockCards2.Text = cards[cardCounter].Translate;

                        cards[cardCounter].Show--;
                        db.Update(cards[cardCounter]);
                        db.SaveChanges();

                        showsCount.Text = Convert.ToString(cards[cardCounter].Show);

                        cardCounter++;
                    }
                    else
                    {
                        var card = db.Cards.FirstOrDefault(x => x.Show > 0);
                        if (card != null)
                        {
                            cardCounter++;
                            goto RefreshCards;
                        }
                        else
                        {
                            btnCards.Visibility = textBlockCards1.Visibility = textBlockCards2.Visibility = textBlockCards3.Visibility = Visibility.Hidden;
                            btnCardsRestart.Visibility = Visibility.Visible;
                            MessageBox.Show("Shows are over. Click the Restart button to reset the shows counter.");
                        }
                    }
                }
                else
                {
                    cardCounter = 0;
                    goto RefreshCards;
                }
            }

        }
        private void btnCards_Click(object sender, RoutedEventArgs e)
        {
            GetNextCard();
        }

        private void btnCardsStart_Click(object sender, RoutedEventArgs e)
        {
            btnCardsStart.Visibility = Visibility.Hidden;
            btnCards.Visibility = textBlockCards1.Visibility = textBlockCards2.Visibility = textBlockCards3.Visibility = Visibility.Visible;

            GetNextCard();

            /*using (FlashCardsContext db = new FlashCardsContext(options))
            {
                var card = db.Cards.FirstOrDefault(x => x.Show >= 0);

                textBlockCards1.Text = card.Word;
                textBlockCards2.Text = card.Translate;

                card.Show--;
                db.Update(card);
                db.SaveChanges();

                showsCount.Text = Convert.ToString(card.Show);
            }*/

        }

        private void btnCardsRestart_Click(object sender, RoutedEventArgs e)
        {
            using (FlashCardsContext db = new FlashCardsContext(options))
            {
                var cards = db.Cards.ToList();
                foreach (var card in cards)
                {
                    card.Show = 5;
                    db.Update(card);
                }
                db.SaveChanges();

                cardCounter = 0;

                btnCards.Visibility = textBlockCards1.Visibility = textBlockCards2.Visibility = textBlockCards3.Visibility = Visibility.Visible;
                btnCardsRestart.Visibility = Visibility.Hidden;

                GetNextCard();
            }
        }

        private void btnTrainingStart_Click(object sender, RoutedEventArgs e)
        {
            textBlockTraining1.Visibility = textBlockTraining2.Visibility = btnTraining1.Visibility = btnTraining2.Visibility = btnTraining3.Visibility = Visibility.Visible;
            btnTrainingStart.Visibility = Visibility.Hidden;

            GetNextTry();
        }

        int tryCounter = 0;
        private void GetNextTry()
        {
            using (FlashCardsContext db = new FlashCardsContext(options))
            {
                var cards = db.Cards.ToList();

            RefreshTry:
                if (tryCounter < cards.Count)
                {
                    if (cards[tryCounter].Try > 0)
                    {
                        textBlockTraining1.Text = cards[tryCounter].Translate;
                        string[] rndTranslate = new string[3];
                        rndTranslate[0] = cards[tryCounter].Word;
                        Random rnd = new Random();

                        for (int i = 1; i < rndTranslate.Length; i++)
                        {
                            rndTranslate[i] = cards[rnd.Next(cards.Count - 1)].Word;

                            if (rndTranslate[i] == rndTranslate[0] || rndTranslate[i] == rndTranslate[i - 1])
                            {
                                i--;
                                continue;
                            }
                        }

                        for (int i = rndTranslate.Length - 1; i >= 1; i--)
                        {
                            int j = rnd.Next(i + 1);

                            string tmp = rndTranslate[j];
                            rndTranslate[j] = rndTranslate[i];
                            rndTranslate[i] = tmp;
                        }

                        btnTraining1.Content = rndTranslate[0];
                        btnTraining2.Content = rndTranslate[1];
                        btnTraining3.Content = rndTranslate[2];

                        cards[tryCounter].Try--;
                        db.Update(cards[tryCounter]);
                        db.SaveChanges();

                        tryCount.Text = Convert.ToString(cards[tryCounter].Try);

                        tryCounter++;
                    }
                    else
                    {
                        var card = db.Cards.FirstOrDefault(x => x.Try > 0);
                        if (card != null)
                        {
                            tryCounter++;
                            goto RefreshTry;
                        }
                        else
                        {
                            textBlockTraining1.Visibility = textBlockTraining2.Visibility = btnTraining1.Visibility = btnTraining2.Visibility = btnTraining3.Visibility = Visibility.Hidden;
                            btnTrainingRestart.Visibility = Visibility.Visible;
                            MessageBox.Show("Trials are over. Click the Restart button to reset the try counter.");
                        }
                    }
                }
                else
                {
                    tryCounter = 0;
                    goto RefreshTry;
                }
            }

        }

        private void btnTrainingRestart_Click(object sender, RoutedEventArgs e)
        {
            using (FlashCardsContext db = new FlashCardsContext(options))
            {
                var cards = db.Cards.ToList();
                foreach (var card in cards)
                {
                    card.Try = 5;
                    db.Update(card);
                }
                db.SaveChanges();

                tryCounter = 0;

                textBlockTraining2.Visibility = btnTraining1.Visibility = btnTraining2.Visibility = btnTraining3.Visibility = Visibility.Visible;
                btnTrainingRestart.Visibility = Visibility.Hidden;

                GetNextTry();
            }
        }

        void PushButtonTraining(Button btn)
        {
            using(FlashCardsContext db = new FlashCardsContext(options))
            {
                var cards = db.Cards.FirstOrDefault(x => x.Translate == textBlockTraining1.Text);
                if (cards.Word == Convert.ToString(btn.Content))
                {
                    textBlockTraining3.Visibility = Visibility.Hidden;
                    GetNextTry();
                }
                else
                {
                    textBlockTraining3.Visibility = Visibility.Visible;
                    Random rnd = new Random();
                    textBlockTraining3.Text = errorWord[rnd.Next(errorWord.Length - 1)];

                    cards.Show += 2;
                    cards.Try += 2;
                    tryCount.Text = Convert.ToString(cards.Try);

                    db.Update(cards);
                    db.SaveChanges();
                }
            }
        }

        string[] errorWord = new string[] { "Let's try again", "Nope...", "Not really!" };
        private void btnTraining1_Click(object sender, RoutedEventArgs e)
        {
            PushButtonTraining(btnTraining1);
        }

        private void btnTraining2_Click(object sender, RoutedEventArgs e)
        {
            PushButtonTraining(btnTraining2);
        }

        private void btnTraining3_Click(object sender, RoutedEventArgs e)
        {
            PushButtonTraining(btnTraining3);
        }
    }
}
