using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace beMyCyberHero
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private MouseState currentMouseState;
        private MouseState previousMouseState;
        private Random random = new Random();
        private string inputText = ""; /// <summary>
                                       /// Przechowywane wprowadzone hasło
                                       /// </summary>
        private bool isTextInputActive = false; /// <summary>
                                                /// Czy pole tekstowe jest aktywne
                                                /// </summary>
        private Rectangle textFieldBounds; /// <summary>
                                           /// Obszar pola tekstowego
                                           /// </summary>

        private string validationMessage = "";
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;

        private string gameState = "MainMenu";
        private string difficultyLevel = "Normal"; /// <summary>
                                                   /// Domyślny poziom trudności
                                                   /// </summary>
        private List<Task> tasks;
        private Task currentTask;
        private string lastMiniGame = "";

        /// <summary>
        /// Elementy UI i tła
        /// </summary>
        private Texture2D buttonTexture, menuButtonTexture;
        private Texture2D mapTexture;
        private Texture2D mapBackground, portraitBackground, taskDescriptionBackground;
        private Texture2D mainMenuBackground;

        // Ikony
        private Texture2D heartIcon;

        private List<string> emailOptions; /// <summary>
                                           /// Lista e-maili do wyświetlenia
                                           /// </summary>
        private int correctEmailIndex;    /// <summary>
                                          /// Indeks fałszywego e-maila
                                          /// </summary>
        private List<Rectangle> emailRectangles;

        private List<string> miniGames; /// <summary>
                                        /// Lista nazw minigier
                                        /// </summary>
        private string currentMiniGame; /// <summary>
                                        /// Obecnie aktywna minigra
                                        /// </summary>

        private Rectangle actionFieldBounds;
        private bool isDragTaskComplete;

        private List<Rectangle> fileRects; /// <summary>
                                           /// Prostokąty reprezentujące pliki
                                           /// </summary>
        private int infectedFileIndex; /// <summary>
                                       /// Indeks pliku zainfekowanego
                                       /// </summary>
        private Rectangle quarantineZone; // Strefa kwarantanny
        private int? draggedFileIndex = null; /// <summary>
                                              /// Indeks aktualnie przeciąganego pliku
                                              /// </summary>
        private int infectedWebsiteIndex;   // Indeks zainfekowanej strony
        private List<string> infectedWebsites; /// <summary>
                                               /// Lista stron zainfekowanych
                                               /// </summary>
        private List<string> safeWebsites;     /// <summary>
                                               /// Lista stron niezainfekowanych
                                               /// </summary>
        private List<string> allWebsites;      /// <summary>
                                               /// Lista wszystkich stron (do rysowania)
                                               /// </summary>
        private bool isFeedbackActive = false; /// <summary>
                                               /// Czy trwa wyświetlanie komunikatu
                                               /// </summary>
        private bool isAnswerCorrect = false; /// <summary>
                                              /// Czy odpowiedź była poprawna
                                              /// </summary>

        private List<string> infectedFiles; /// <summary>
                                            /// Lista plików zainfekowanych
                                            /// </summary>
        private List<string> safeFiles;     /// <summary>
                                            /// Lista plików niezainfekowanych
                                            /// </summary>
        private List<string> allFiles;      /// <summary>
                                            /// Lista wszystkich plików (do rysowania)
                                            /// </summary>
        private List<Texture2D> portraits;
        private Texture2D currentPortrait;
        private Dictionary<string, string> taskDescriptions;

        private List<string> infectedEmails; /// <summary>
                                             /// Lista zainfekowanych e-maili
                                             /// </summary>
        private List<string> safeEmails;     /// <summary>
                                             /// Lista poprawnych e-maili
                                             /// </summary>
        private int currentStage = 1; /// <summary>
                                      /// Bieżący etap (1-3)
                                      /// </summary>
        private int currentLevel = 1; /// <summary>
                                      /// Bieżący poziom w etapie (1-3)
                                      /// </summary>
        private List<bool> badges = new List<bool> { false, false, false }; /// <summary>
                                                                            /// Odznaki za etapy                                                  
                                                                            /// </summary>
        private List<string> stageMiniGames;
        private Texture2D badgeTexture;
        private int remainingLives;
        private List<Texture2D> maps;
        private Texture2D victoryBackground; /// <summary>
                                             /// Tło dla ekranu zwycięstwa
                                             /// </summary>
        private Texture2D gameOverBackground; /// Tło dla ekranu przegranej
        private double taskTimeLimit = 0; /// Limit czasu na zadanie (w sekundach)
        private double remainingTime = 0; /// Pozostały czas na zadanie
        private bool isMenuExpanded = false; /// Czy menu jest rozwinięte
        private bool hasFinalChance = true; /// Flaga do zarządzania dodatkową szansą


        /// <summary>
        /// Konstruktor inicjalizujący podstawowe ustawienia gry.
        /// </summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Inicjalizacja miniGames
            miniGames = new List<string>
    {
        "PasswordGame",
        "EmailTask",
        "DragTask",
        "WebsiteTask"
    };
        }


        /// <summary>
        /// Ustawia poziom trudności gry, żyć i limit czasu.
        /// </summary>
        /// <param name="level"></param>
        private void SetDifficultyLevel(string level)
        {
            difficultyLevel = level;

            switch (difficultyLevel)
            {
                case "Easy":
                    remainingLives = 3; // 3 życia
                    taskTimeLimit = 0; // Brak limitu czasu
                    break;

                case "Normal":
                    remainingLives = 1; // 1 życie
                    taskTimeLimit = 0; // Brak limitu czasu
                    break;

                case "Hard":
                    remainingLives = 0; // Brak żyć
                    taskTimeLimit = 30; // Limit czasu: 30 sekund
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"Ustawiono poziom trudności: {difficultyLevel}, Życia: {remainingLives}, Limit czasu: {taskTimeLimit}");
        }


        /// <summary>
        /// Wczytuje wszystkie zasoby, takie jak tekstury, czcionki i dane.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("DefaultFont");
            taskDescriptions = new Dictionary<string, string>
                {
                    { "PasswordGame",
                        "Proszę, musisz mi pomóc! Moje konto jest zagrożone, a jeśli ktoś się na nie włamie, stracę wszystkie moje dane! Stwórz dla mnie hasło, które będzie wystarczająco silne – co najmniej 8 znaków, wielką literę, cyfrę i znak specjalny. To moja jedyna nadzieja na zabezpieczenie moich danych!" },

                    { "EmailTask",
                        "To wygląda bardzo podejrzanie... Nie wiem, który z tych e-maili jest fałszywy, ale jeśli otworzę zły, mogę stracić dostęp do wszystkiego! Proszę, znajdź ten, który jest oszustwem. Moje bezpieczeństwo w sieci zależy od Twojej decyzji!" },

                    { "DragTask",
                        "Pomocy! Znalazłem coś podejrzanego w moim systemie. Jeden z tych plików to wirus, który może wszystko zniszczyć! Musisz przeciągnąć zainfekowany plik do strefy kwarantanny. Ale uważaj, bo jeśli przeniesiesz zdrowy plik, mogę stracić coś bardzo ważnego!" },

                    { "WebsiteTask",
                        "Nie mogę się pomylić! Jedna z tych stron to pułapka, która może mnie zaatakować wirusem. Jeśli wybiorę złą stronę, mogę stracić dostęp do wszystkiego, co dla mnie ważne. Proszę, wskaż, która strona jest niebezpieczna. Moje życie w sieci leży w Twoich rękach!" }
                };

            // Wczytanie map do listy
            maps = new List<Texture2D>
                {
                    Content.Load<Texture2D>("Backgrounds/map_image_1"), // Etap 1, poziom 1
                    Content.Load<Texture2D>("Backgrounds/map_image_2"), // Etap 1, poziom 2
                    Content.Load<Texture2D>("Backgrounds/map_image_3"), // Etap 1, poziom 3
                    Content.Load<Texture2D>("Backgrounds/map_image_4"), // Etap 2, poziom 1
                    Content.Load<Texture2D>("Backgrounds/map_image_5"), // Etap 2, poziom 2
                    Content.Load<Texture2D>("Backgrounds/map_image_6"), // Etap 2, poziom 3
                    Content.Load<Texture2D>("Backgrounds/map_image_7"), // Etap 3, poziom 1
                    Content.Load<Texture2D>("Backgrounds/map_image_8"), // Etap 3, poziom 2
                    Content.Load<Texture2D>("Backgrounds/map_image_9")  // Etap 3, poziom 3
                };

            // Ustawienie początkowej mapy
            mapTexture = maps[0];
            textFieldBounds = new Rectangle(
                GraphicsDevice.Viewport.Width / 2 - 150, // X
                GraphicsDevice.Viewport.Height / 2 - 25, // Y
                300, // Szerokość
                50   // Wysokość
            );
            actionFieldBounds = new Rectangle(
                GraphicsDevice.Viewport.Width * 2 / 5, // Początek pola akcji
                50,                                   // Y - poniżej menu
                GraphicsDevice.Viewport.Width * 3 / 5, // Szerokość
                GraphicsDevice.Viewport.Height - 50    // Wysokość
            );
            portraits = new List<Texture2D>();
            // Dodaj portrety z Content
            portraits.Add(Content.Load<Texture2D>("Backgrounds/portrait_image_woman"));
            portraits.Add(Content.Load<Texture2D>("Backgrounds/portrait_image_elephant"));
            portraits.Add(Content.Load<Texture2D>("Backgrounds/portrait_image_wolf"));
            portraits.Add(Content.Load<Texture2D>("Backgrounds/portrait_image_snake"));
            currentPortrait = portraits[0];

            buttonTexture = CreateTexture(Color.Gray);
            menuButtonTexture = CreateTexture(Color.LightGray);
            mapBackground = CreateTexture(Color.LightBlue);
            portraitBackground = CreateTexture(Color.LightYellow);
            taskDescriptionBackground = CreateTexture(Color.Beige);

            // Wczytaj tekstury mapy
            mapTexture = Content.Load<Texture2D>("Backgrounds/map_image_1");


            // Wczytaj ikony
            badgeTexture = Content.Load<Texture2D>("Backgrounds/icon_star");
            heartIcon = Content.Load<Texture2D>("Backgrounds/icon_hearth");
            menuButtonTexture = CreateTexture(Color.LightGray);
            buttonTexture = CreateTexture(Color.Gray);

            if (heartIcon != null)
            {
                Console.WriteLine("Tekstura serca wczytana poprawnie.");
            }
            else
            {
                Console.WriteLine("Błąd: Tekstura serca nie została wczytana!");
            }
            // Wczytaj tło menu głównego
            mainMenuBackground = Content.Load<Texture2D>("Backgrounds/main_menu_background");
            // Wczytywanie tła ekranów końcowych
            victoryBackground = Content.Load<Texture2D>("Backgrounds/victory_background");
            gameOverBackground = Content.Load<Texture2D>("Backgrounds/gameover_background");
            // Tasks
            tasks = new List<Task>
            {
                new Task("Podaj silne hasło (min. 8 znaków):", "Hasło powinno mieć min. 8 znaków, 1 wielką literę, 1 cyfrę i 1 znak specjalny."),

            };
            currentTask = tasks[0];
            tasks.Add(new Task(
            "Rozpoznaj fałszywy e-mail:",
            "Kliknij na e-mail, który wygląda podejrzanie."
        ));

            miniGames = new List<string>
            {
                "PasswordGame",
                "EmailTask",
                "DragTask",
                "WebsiteTask"
            };
            currentMiniGame = ""; // Zaczynamy bez aktywnej minigry


        }


        /// <summary>
        /// Przygotowuje listę minigier dla danego etapu.
        /// </summary>
        private void SetupStageMiniGames()
        {
            stageMiniGames = new List<string>(miniGames); // Odśwież listę minigier dla nowego etapu
            System.Diagnostics.Debug.WriteLine($"Przygotowano nowe minigry dla etapu {currentStage}: {string.Join(", ", stageMiniGames)}");

            currentLevel = 1; // Reset poziomu dla nowego etapu
        }


        /// <summary>
        /// Aktualizuje logikę gry, stan wejścia i minigier.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (gameState == "MainMenu")
            {
                HandleMainMenuInput();
            }
            else if (gameState == "Gameplay")
            {
                // Obsługa kliknięcia przycisku MENU
                Rectangle menuButtonBounds = new Rectangle(GraphicsDevice.Viewport.Width - 100, 5, 90, 40);
                if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed &&
                    menuButtonBounds.Contains(currentMouseState.Position))
                {
                    isMenuExpanded = !isMenuExpanded; // Przełącz rozwinięcie menu
                }

                // Obsługa rozwiniętego menu
                if (isMenuExpanded)
                {
                    int menuX = GraphicsDevice.Viewport.Width - 210;
                    int menuY = 50;
                    Rectangle mainMenuButton = new Rectangle(menuX + 10, menuY + 10, 200 - 20, 30);
                    Rectangle exitButton = new Rectangle(menuX + 10, menuY + 50, 200 - 20, 30);

                    if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (mainMenuButton.Contains(currentMouseState.Position))
                        {
                            gameState = "MainMenu";
                            isMenuExpanded = false;
                        }
                        else if (exitButton.Contains(currentMouseState.Position))
                        {
                            Exit();
                        }
                    }
                }

                // Obsługa poziomu trudnego (licznik czasu)
                if (difficultyLevel == "Hard" && taskTimeLimit > 0)
                {
                    remainingTime -= gameTime.ElapsedGameTime.TotalSeconds;

                    if (remainingTime <= 0)
                    {
                        // Czas się skończył - gracz przegrywa
                        gameState = "GameOverScreen";
                        return;
                    }
                }

                // Obsługa aktualnej minigry
                switch (currentMiniGame)
                {
                    case "PasswordGame":
                        if (currentMouseState.LeftButton == ButtonState.Released &&
                            previousMouseState.LeftButton == ButtonState.Pressed)
                        {
                            if (textFieldBounds.Contains(currentMouseState.Position))
                            {
                                isTextInputActive = true; // Aktywuj pole tekstowe
                                validationMessage = "";  // Wyczyść wiadomość walidacyjną
                            }
                            else
                            {
                                isTextInputActive = false; // Dezaktywuj pole tekstowe
                            }
                        }

                        if (isTextInputActive)
                        {
                            HandleTextInput();
                        }
                        else if (ZadanieZakonczone())
                        {
                            bool isCorrect = validationMessage == "Hasło jest silne!";
                            AdvanceLevelOrStage(isCorrect);
                        }
                        break;

                    case "EmailTask":
                        if (emailRectangles != null && emailRectangles.Count > 0)
                        {
                            if (currentMouseState.LeftButton == ButtonState.Released &&
                                previousMouseState.LeftButton == ButtonState.Pressed)
                            {
                                for (int i = 0; i < emailRectangles.Count; i++)
                                {
                                    if (emailRectangles[i].Contains(currentMouseState.Position))
                                    {
                                        if (i == correctEmailIndex)
                                        {
                                            validationMessage = "Poprawnie wskazałeś podejrzany e-mail!";
                                            AdvanceLevelOrStage(true); // Odpowiedź poprawna
                                        }
                                        else
                                        {
                                            validationMessage = "To nie jest podejrzany e-mail. Tracisz jedno z żyć.";
                                            AdvanceLevelOrStage(false); // Odpowiedź błędna
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        break;

                    case "DragTask":
                        if (!isDragTaskComplete && fileRects != null)
                        {
                            if (currentMouseState.LeftButton == ButtonState.Pressed)
                            {
                                // Jeśli plik nie jest przeciągany, sprawdź, czy kliknięto na plik
                                if (draggedFileIndex == null)
                                {
                                    for (int i = 0; i < fileRects.Count; i++)
                                    {
                                        if (fileRects[i].Contains(currentMouseState.Position))
                                        {
                                            draggedFileIndex = i; // Ustaw indeks przeciąganego pliku
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    // Aktualizacja pozycji przeciąganego pliku
                                    int newX = Math.Clamp(currentMouseState.X - fileRects[draggedFileIndex.Value].Width / 2,
                                                          actionFieldBounds.X,
                                                          actionFieldBounds.Right - fileRects[draggedFileIndex.Value].Width);
                                    int newY = Math.Clamp(currentMouseState.Y - fileRects[draggedFileIndex.Value].Height / 2,
                                                          actionFieldBounds.Y,
                                                          actionFieldBounds.Bottom - fileRects[draggedFileIndex.Value].Height);

                                    fileRects[draggedFileIndex.Value] = new Rectangle(newX, newY,
                                                                                      fileRects[draggedFileIndex.Value].Width,
                                                                                      fileRects[draggedFileIndex.Value].Height);
                                }
                            }
                            else if (currentMouseState.LeftButton == ButtonState.Released && draggedFileIndex != null)
                            {
                                // Sprawdź, czy przeciągnięto plik do strefy kwarantanny
                                Rectangle extendedQuarantineZone = new Rectangle(
                                    quarantineZone.X - 10,
                                    quarantineZone.Y - 10,
                                    quarantineZone.Width + 20,
                                    quarantineZone.Height + 20
                                );

                                if (extendedQuarantineZone.Intersects(fileRects[draggedFileIndex.Value]))
                                {
                                    if (draggedFileIndex == infectedFileIndex)
                                    {
                                        validationMessage = "Gratulacje! Ten plik był zainfekowany.";
                                        isDragTaskComplete = true;
                                        AdvanceLevelOrStage(true); // Odpowiedź poprawna
                                    }
                                    else
                                    {
                                        validationMessage = "Zły plik! Tracisz jedno z żyć.";
                                        AdvanceLevelOrStage(false); // Odpowiedź błędna
                                    }
                                }

                                // Resetowanie stanu przeciągania
                                draggedFileIndex = null;
                            }
                        }
                        break;



                    case "WebsiteTask":
                        if (currentMouseState.LeftButton == ButtonState.Released &&
                            previousMouseState.LeftButton == ButtonState.Pressed)
                        {
                            int yOffset = actionFieldBounds.Y + 20;
                            for (int i = 0; i < allWebsites.Count; i++)
                            {
                                Rectangle websiteRect = new Rectangle(
                                    actionFieldBounds.X + 20,
                                    yOffset,
                                    actionFieldBounds.Width - 40,
                                    50
                                );

                                if (websiteRect.Contains(currentMouseState.Position))
                                {
                                    if (i == infectedWebsiteIndex)
                                    {
                                        validationMessage = "Gratulacje! Wskazałeś zainfekowaną stronę.";
                                        AdvanceLevelOrStage(true); // Odpowiedź poprawna
                                    }
                                    else
                                    {
                                        validationMessage = "Błąd! Postać straciła ważne dane osobowe. Tracisz jedno z żyć.";
                                        AdvanceLevelOrStage(false); // Odpowiedź błędna
                                    }
                                    break;
                                }

                                yOffset += 60;
                            }
                        }
                        break;
                }
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Rysuje aktualny stan gry na ekranie.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            switch (gameState)
            {
                case "MainMenu":
                    DrawMainMenu();
                    break;

                case "Gameplay":
                    DrawMenuBar();
                    DrawGameInterface();

                    // Wywołanie odpowiedniej minigry
                    switch (currentMiniGame)
                    {
                        case "PasswordGame":
                            DrawPasswordGame();
                            break;
                        case "EmailTask":
                            DrawEmailTask();
                            break;
                        case "DragTask":
                            DrawDragTask();
                            break;
                        case "WebsiteTask":
                            DrawWebsiteTask();
                            break;
                    }

                    // Rysowanie przycisku MENU na wierzchu
                    Rectangle menuButtonBounds = new Rectangle(GraphicsDevice.Viewport.Width - 100, 5, 90, 40);
                    spriteBatch.Draw(menuButtonTexture, menuButtonBounds, Color.LightGray);
                    spriteBatch.DrawString(font, "MENU", new Vector2(menuButtonBounds.X + 10, menuButtonBounds.Y + 10), Color.Black);

                    // Jeśli menu jest rozwinięte, rysuj jego zawartość
                    if (isMenuExpanded)
                    {
                        DrawExpandedMenu();
                    }
                    break;

                case "VictoryScreen":
                    DrawVictoryScreen();
                    break;

                case "GameOverScreen":
                    DrawGameOverScreen();
                    break;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }



        /// <summary>
        /// Obsługuje wprowadzanie tekstu w polu tekstowym.
        /// </summary>
        private void HandleTextInput()
        {
            Keys[] pressedKeys = currentKeyboardState.GetPressedKeys();
            bool shiftPressed = currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift);

            foreach (Keys key in pressedKeys)
            {
                // Ignoruj klawisze wciśnięte w poprzedniej ramce
                if (previousKeyboardState.IsKeyDown(key))
                    continue;

                // Obsługa klawisza Backspace
                if (key == Keys.Back && inputText.Length > 0)
                {
                    inputText = inputText.Substring(0, inputText.Length - 1);
                }
                // Obsługa klawisza Enter
                else if (key == Keys.Enter)
                {
                    ValidatePassword();
                }
                // Filtrowanie znaków alfanumerycznych i specjalnych
                else
                {
                    string filteredKey = FilterInputKey(key, shiftPressed);
                    if (!string.IsNullOrEmpty(filteredKey) && inputText.Length < 20)
                    {
                        inputText += filteredKey;
                    }
                }
            }
        }


        /// <summary>
        /// Obsługuje kliknięcia myszą w menu głównym.
        /// </summary>
        private void HandleMainMenuInput()
        {
            int screenWidth = GraphicsDevice.Viewport.Width;
            int buttonWidth = 300;
            int buttonHeight = 50;
            int buttonX = screenWidth - buttonWidth - 50; // Przyciski po prawej stronie
            int buttonYStart = 200;
            int buttonSpacing = 10;

            Rectangle easyButtonBounds = new Rectangle(buttonX, buttonYStart, buttonWidth, buttonHeight);
            Rectangle normalButtonBounds = new Rectangle(buttonX, buttonYStart + buttonHeight + buttonSpacing, buttonWidth, buttonHeight);
            Rectangle hardButtonBounds = new Rectangle(buttonX, buttonYStart + 2 * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);
            Rectangle startGameButtonBounds = new Rectangle(buttonX, buttonYStart + 3 * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);

            if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (easyButtonBounds.Contains(currentMouseState.Position))
                {
                    ResetGameProgress(); // Zresetuj postęp gry
                    SetDifficultyLevel("Easy");
                    System.Diagnostics.Debug.WriteLine("Wybrano poziom trudności: Łatwy");
                }
                else if (normalButtonBounds.Contains(currentMouseState.Position))
                {
                    ResetGameProgress(); // Zresetuj postęp gry
                    SetDifficultyLevel("Normal");
                    System.Diagnostics.Debug.WriteLine("Wybrano poziom trudności: Normalny");
                }
                else if (hardButtonBounds.Contains(currentMouseState.Position))
                {
                    ResetGameProgress(); // Zresetuj postęp gry
                    SetDifficultyLevel("Hard");
                    System.Diagnostics.Debug.WriteLine("Wybrano poziom trudności: Trudny");
                }
                else if (startGameButtonBounds.Contains(currentMouseState.Position))
                {
                    if (string.IsNullOrEmpty(difficultyLevel))
                    {
                        SetDifficultyLevel("Easy"); // Ustaw domyślnie na Easy, jeśli nie wybrano trudności
                    }

                    gameState = "Gameplay"; // Przejdź do gry
                    SelectRandomMiniGame();
                    System.Diagnostics.Debug.WriteLine("Rozpoczęto grę");
                }
                // Dodaj opcjonalne logi, aby monitorować kliknięcia poza przyciskami
                else
                {
                    System.Diagnostics.Debug.WriteLine("Kliknięcie poza przyciskiem w menu głównym.");
                }
            }
        }


        /// <summary>
        /// Losuje nowy portret dla bieżącego poziomu
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void SelectRandomPortrait()
        {
            if (portraits == null || portraits.Count == 0)
            {
                throw new InvalidOperationException("Lista portretów jest pusta lub niezainicjalizowana.");
            }

            Texture2D newPortrait;
            do
            {
                int randomIndex = random.Next(portraits.Count);
                newPortrait = portraits[randomIndex];
            } while (newPortrait == currentPortrait); // Upewnij się, że portret jest inny

            currentPortrait = newPortrait; // Ustaw nowy portret
            System.Diagnostics.Debug.WriteLine($"Wylosowano nowy portret: {currentPortrait}");
        }


        /// <summary>
        /// Metoda pomocnicza do aktualizacji mapy zależnie od etapu i poziomu
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        private void UpdateMapTexture()
        {
            // Obliczenie indeksu mapy na podstawie etapu i poziomu
            int mapIndex = (currentStage - 1) * 3 + (currentLevel - 1);

            // Sprawdzenie poprawności indeksu
            if (mapIndex >= 0 && mapIndex < maps.Count)
            {
                mapTexture = maps[mapIndex];
                System.Diagnostics.Debug.WriteLine($"Wczytano mapę: Indeks={mapIndex}, Etap={currentStage}, Poziom={currentLevel}");
            }
            else
            {
                throw new IndexOutOfRangeException($"Nieprawidłowy indeks mapy: {mapIndex}");
            }
        }


        /// <summary>
        /// Metoda pomocnicza do weryfikacji ukończenia zadania
        /// </summary>
        /// <returns></returns>
        private bool ZadanieZakonczone()
        {
            // Sprawdź kryteria zakończenia dla każdej minigry
            switch (currentMiniGame)
            {
                case "PasswordGame":
                    return validationMessage == "Hasło jest silne!";
                case "EmailTask":
                    return validationMessage == "Poprawnie wskazałeś podejrzany e-mail!";
                case "DragTask":
                    return validationMessage == "Dane zostały prawidłowo zabezpieczone!";
                case "WebsiteTask":
                    return validationMessage == "Poprawnie wybrałeś stronę!";
                default:
                    return false;
            }
        }


        /// <summary>
        /// Rysuje ekran głównego menu.
        /// </summary>
        private void DrawMainMenu()
        {
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            // Tło menu głównego
            spriteBatch.Draw(mainMenuBackground, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);

            // Wymiary przycisków
            int buttonWidth = 300;
            int buttonHeight = 50;
            int buttonX = screenWidth - buttonWidth - 50; // Przyciski po prawej stronie
            int buttonYStart = 200;
            int buttonSpacing = 10;

            // Tło za napisem "Poziom trudności"
            Rectangle textBackground = new Rectangle(buttonX, buttonYStart - 70, buttonWidth, 30);
            spriteBatch.Draw(buttonTexture, textBackground, Color.Gray * 0.8f);

            // Wyświetlanie poziomu trudności
            spriteBatch.DrawString(font, $"Poziom trudności: {difficultyLevel}", new Vector2(buttonX + 10, buttonYStart - 65), Color.White);

            // Przyciski
            Rectangle easyButtonBounds = new Rectangle(buttonX, buttonYStart, buttonWidth, buttonHeight);
            Rectangle normalButtonBounds = new Rectangle(buttonX, buttonYStart + buttonHeight + buttonSpacing, buttonWidth, buttonHeight);
            Rectangle hardButtonBounds = new Rectangle(buttonX, buttonYStart + 2 * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);
            Rectangle startGameButtonBounds = new Rectangle(buttonX, buttonYStart + 3 * (buttonHeight + buttonSpacing), buttonWidth, buttonHeight);

            // Rysowanie przycisków
            spriteBatch.Draw(buttonTexture, easyButtonBounds, Color.White);
            spriteBatch.DrawString(font, "Łatwy", new Vector2(easyButtonBounds.X + 10, easyButtonBounds.Y + 10), Color.Black);

            spriteBatch.Draw(buttonTexture, normalButtonBounds, Color.White);
            spriteBatch.DrawString(font, "Normalny", new Vector2(normalButtonBounds.X + 10, normalButtonBounds.Y + 10), Color.Black);

            spriteBatch.Draw(buttonTexture, hardButtonBounds, Color.White);
            spriteBatch.DrawString(font, "Trudny", new Vector2(hardButtonBounds.X + 10, hardButtonBounds.Y + 10), Color.Black);

            spriteBatch.Draw(buttonTexture, startGameButtonBounds, Color.White);
            spriteBatch.DrawString(font, "Rozpocznij grę", new Vector2(startGameButtonBounds.X + 10, startGameButtonBounds.Y + 10), Color.Black);
        }


        /// <summary>
        /// Rysuje pasek menu z informacjami o stanie gry.
        /// </summary>
        private void DrawMenuBar()
        {
            int menuHeight = 50;

            // Rysuj tło paska menu
            spriteBatch.Draw(buttonTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, menuHeight), Color.Gray);

            int xOffset = 20;

            // Wyświetlanie etapu i poziomu
            spriteBatch.DrawString(font, $"ETAP: {currentStage} | POZIOM: {currentLevel}", new Vector2(xOffset, 15), Color.White);
            xOffset += 250;

            // Wyświetlanie poziomu trudności
            spriteBatch.DrawString(font, $"Trudność: {difficultyLevel}", new Vector2(xOffset, 15), Color.White);
            xOffset += 200;

            // Wyświetlanie tekstu "Odznaki:"
            spriteBatch.DrawString(font, "Odznaki:", new Vector2(xOffset, 15), Color.White);
            xOffset += 80;

            // Wyświetlanie zdobytych odznak
            for (int i = 0; i < badges.Count; i++)
            {
                if (badges[i]) // Jeśli odznaka została zdobyta
                {
                    spriteBatch.Draw(badgeTexture, new Rectangle(xOffset, 5, 40, 40), Color.White);
                }
                else
                {
                    spriteBatch.Draw(badgeTexture, new Rectangle(xOffset, 5, 40, 40), Color.Gray); // Odznaka w szarym kolorze
                }

                xOffset += 50; // Odstęp między odznakami
            }

            xOffset += 50; // Większy odstęp między odznakami a życiami

            // Wyświetlanie żyć
            spriteBatch.DrawString(font, "Życia:", new Vector2(xOffset, 15), Color.White);
            xOffset += 60;

            for (int i = 0; i < remainingLives; i++)
            {
                spriteBatch.Draw(heartIcon, new Rectangle(xOffset, 5, 40, 40), Color.White);
                xOffset += 50;
            }

            // Komunikat "Ostatnia szansa!" po prawej stronie od żyć
            if (remainingLives <= 0 && !hasFinalChance || difficultyLevel == "Hard")
            {
                spriteBatch.DrawString(
                    font,
                    "Ostatnia szansa!",
                    new Vector2(xOffset, 15), // Wyświetl po prawej stronie od żyć
                    Color.Red
                );
            }

            // Licznik czasu dla poziomu Hard
            if (difficultyLevel == "Hard")
            {
                string timeText = $"Czas: {Math.Max(0, (int)remainingTime)} s";
                spriteBatch.DrawString(font, timeText, new Vector2(GraphicsDevice.Viewport.Width - 250, 15), Color.White); // Ustaw odpowiednią pozycję
            }


        }


        /// <summary>
        /// Rysuje rozwinięte menu podczas gry.
        /// </summary>
        private void DrawExpandedMenu()
        {
            int menuWidth = 200;
            int menuHeight = 100;
            int menuX = GraphicsDevice.Viewport.Width - menuWidth - 10;
            int menuY = 50;

            // Tło rozwijanego menu
            spriteBatch.Draw(buttonTexture, new Rectangle(menuX, menuY, menuWidth, menuHeight), Color.DarkGray);

            // Przycisk wyjścia do głównego menu
            Rectangle mainMenuButton = new Rectangle(menuX + 10, menuY + 10, menuWidth - 20, 30);
            spriteBatch.Draw(buttonTexture, mainMenuButton, Color.Gray);
            spriteBatch.DrawString(font, "Menu Główne", new Vector2(mainMenuButton.X + 10, mainMenuButton.Y + 5), Color.White);

            // Przycisk wyjścia z gry
            Rectangle exitButton = new Rectangle(menuX + 10, menuY + 50, menuWidth - 20, 30);
            spriteBatch.Draw(buttonTexture, exitButton, Color.Gray);
            spriteBatch.DrawString(font, "Wyjdź z gry", new Vector2(exitButton.X + 10, exitButton.Y + 5), Color.White);
        }


        /// <summary>
        /// Rysuje opis aktualnego zadania.
        /// </summary>
        private void DrawTaskDescription()
        {
            int descriptionX = 0;
            int descriptionY = GraphicsDevice.Viewport.Height * 4 / 9;
            int descriptionWidth = GraphicsDevice.Viewport.Width * 2 / 5; // Dopasowane do lewego panelu
            int descriptionHeight = GraphicsDevice.Viewport.Height - descriptionY;

            // Rysowanie tła opisu zadania
            spriteBatch.Draw(CreateTexture(Color.SlateGray),
                new Rectangle(descriptionX, descriptionY, descriptionWidth, descriptionHeight), Color.White);

            // Pobieranie opisu zadania
            string taskDescription = "Brak opisu dla tego zadania.";
            if (!string.IsNullOrEmpty(currentMiniGame) && taskDescriptions.ContainsKey(currentMiniGame))
            {
                taskDescription = taskDescriptions[currentMiniGame];
            }

            string wrappedText = WrapText(font, taskDescription, descriptionWidth - 20);
            spriteBatch.DrawString(font, wrappedText, new Vector2(descriptionX + 10, descriptionY + 10), Color.Black);
        }


        /// <summary>
        /// Rysuje główny interfejs gry, w tym mapę i opis zadania.
        /// </summary>
        private void DrawGameInterface()
        {
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            int leftPanelWidth = screenWidth * 2 / 5;
            int menuHeight = 50;
            int mapHeight = screenHeight / 5;
            int portraitHeight = screenHeight / 5;
            int actionFieldWidth = screenWidth - leftPanelWidth;

            // Rysowanie tła mapy
            spriteBatch.Draw(mapBackground, new Rectangle(0, menuHeight, leftPanelWidth, mapHeight), Color.White);
            spriteBatch.Draw(mapTexture, new Rectangle(0, menuHeight, leftPanelWidth, mapHeight), Color.White);

            // Rysowanie tła portretu
            spriteBatch.Draw(portraitBackground, new Rectangle(0, menuHeight + mapHeight, leftPanelWidth, portraitHeight), Color.White);

            // Rysowanie portretu
            spriteBatch.Draw(
                currentPortrait,
                new Rectangle(0, menuHeight + mapHeight, leftPanelWidth, portraitHeight),
                Color.White
            );


            // Wyświetlenie opisu zadania
            DrawTaskDescription();

            // Rysowanie tła pola akcji
            spriteBatch.Draw(CreateTexture(Color.Black),
                new Rectangle(leftPanelWidth, menuHeight, actionFieldWidth, screenHeight - menuHeight), Color.White);

            // Wywołanie odpowiedniej minigry
            switch (currentMiniGame)
            {
                case "PasswordGame":
                    DrawPasswordGame();
                    break;
                case "EmailTask":
                    DrawEmailTask();
                    break;
                case "DragTask":
                    DrawDragTask();
                    break;
                case "WebsiteTask":
                    DrawWebsiteTask();
                    break;
            }
        }


        /// <summary>
        /// Klasa reprezentująca zadanie z tytułem i opisem, używana w minigrach.
        /// </summary>
        public class Task
        {
            public string Title { get; } ///
            public string Description { get; }

            public Task(string title, string description)
            {
                Title = title;
                Description = description;
            }
        }


        /// <summary>
        /// Losuje nową minigrę dla bieżącego etapu.
        /// </summary>
        private void SelectRandomMiniGame()
        {
            isTextInputActive = false;
            inputText = "";
            validationMessage = "";
            SelectRandomPortrait(); // Zmień portret po losowaniu nowej minigry

            // Sprawdzenie, czy lista minigier jest pusta
            if (stageMiniGames.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Brak dostępnych minigier w bieżącym etapie!");
                return;
            }

            // Wybór losowej minigry z dostępnych w bieżącym etapie
            Random random = new Random();
            int randomIndex = random.Next(stageMiniGames.Count);
            string nextMiniGame = stageMiniGames[randomIndex];

            // Ustawienie wybranej minigry
            currentMiniGame = nextMiniGame;
            lastMiniGame = currentMiniGame;

            // Usunięcie wybranej minigry z listy
            stageMiniGames.Remove(nextMiniGame);

            System.Diagnostics.Debug.WriteLine($"Wybrano minigrę: {currentMiniGame}, pozostałe minigry: {string.Join(", ", stageMiniGames)}");

            // Resetowanie czasu na zadanie w przypadku poziomu Hard
            if (difficultyLevel == "Hard")
            {
                remainingTime = taskTimeLimit;
            }

            // Konfiguracja odpowiedniej minigry
            switch (currentMiniGame)
            {
                case "PasswordGame":
                    ResetTextInput();
                    break;
                case "EmailTask":
                    SetupEmailTask();
                    break;
                case "DragTask":
                    SetupDragTask();
                    break;
                case "WebsiteTask":
                    SetupWebsiteTask();
                    break;
            }
        }


        /// <summary>
        /// Rysuje interfejs minigry tworzenia hasła.
        /// </summary>
        private void DrawPasswordGame()
        {
            // Aktualizuj pozycję i rozmiar pola tekstowego w granicach pola akcji
            textFieldBounds = new Rectangle(
                actionFieldBounds.X + 20,
                actionFieldBounds.Y + 20,
                actionFieldBounds.Width - 40,
                50
            );

            // Rysowanie tła pola akcji
            spriteBatch.Draw(CreateTexture(Color.White), actionFieldBounds, Color.DarkSlateGray);

            // Rysowanie pola tekstowego
            Color textFieldColor = isTextInputActive ? Color.LightBlue : Color.Gray;
            spriteBatch.Draw(CreateTexture(textFieldColor), textFieldBounds, Color.White);

            // Wyświetlanie wpisywanego tekstu
            string displayText = string.IsNullOrEmpty(inputText) ? "Kliknij, aby wpisać hasło..." : inputText;
            spriteBatch.DrawString(font, displayText, new Vector2(textFieldBounds.X + 10, textFieldBounds.Y + 10), Color.Black);

            // Wyświetlanie komunikatu o walidacji
            spriteBatch.DrawString(font, validationMessage, new Vector2(textFieldBounds.X, textFieldBounds.Y + 60), Color.Green);
        }


        /// <summary>
        /// Metoda pomocnicza do minigry z tworzeniem hasła
        /// </summary>
        private void ValidatePassword()
        {
            if (inputText.Length >= 8 &&
                Regex.IsMatch(inputText, @"[A-Z]") &&
                Regex.IsMatch(inputText, @"\d") &&
                Regex.IsMatch(inputText, @"[@$!%*?&#]"))
            {
                validationMessage = "Hasło jest silne!";
            }
            else
            {
                validationMessage = "Klapa! Haker wykradł hasło postaci. Tracisz jedno z żyć";
            }
        }


        /// <summary>
        /// Inicjalizuje dane dla minigry "Znajdź fałszywy e-mail".
        /// </summary>
        private void SetupEmailTask()
        {
            // Inicjalizacja list
            emailOptions = new List<string>();
            emailRectangles = new List<Rectangle>();

            // Lista zainfekowanych e-maili
            infectedEmails = new List<string>
                {
                    "support@goog1e.com",
                    "update@bank-alerts.org",
                    "security@amazzon-shop.com",
                    "contact@phisshing-alert.net",
                    "admin@linkedln.com",
                    "service@paypai-secure.com",
                    "support@microsof-tupdate.net",
                    "team@zoom-upgrade.com",
                    "info@netfflix-billing.com",
                    "help@twittter-alerts.com"
                };

            // Lista poprawnych e-maili
            safeEmails = new List<string>
                {
                    "support@google.com",
                    "help@amazon.com",
                    "info@netflix.com",
                    "contact@microsoft.com",
                    "admin@github.com",
                    "support@paypal.com",
                    "service@apple.com",
                    "contact@spotify.com",
                    "help@twitter.com",
                    "admin@linkedin.com",
                    "team@zoom.us",
                    "service@reddit.com"
                };


            // Losowanie jednego zainfekowanego e-maila
            Random random = new Random();
            string chosenInfectedEmail = infectedEmails[random.Next(infectedEmails.Count)];

            // Losowanie sześciu poprawnych e-maili
            List<string> chosenSafeEmails = new List<string>();
            while (chosenSafeEmails.Count < 6)
            {
                string safeEmail = safeEmails[random.Next(safeEmails.Count)];
                if (!chosenSafeEmails.Contains(safeEmail))
                {
                    chosenSafeEmails.Add(safeEmail);
                }
            }

            // Łączenie listy e-maili
            emailOptions.AddRange(chosenSafeEmails);
            correctEmailIndex = random.Next(emailOptions.Count); // Losowa pozycja dla zainfekowanego e-maila
            emailOptions.Insert(correctEmailIndex, chosenInfectedEmail);

            // Tworzenie prostokątów dla e-maili
            int yOffset = actionFieldBounds.Y + 20;
            for (int i = 0; i < emailOptions.Count; i++)
            {
                emailRectangles.Add(new Rectangle(
                    actionFieldBounds.X + 20,
                    yOffset,
                    actionFieldBounds.Width - 40,
                    50
                ));
                yOffset += 60; // Odstęp między opcjami
            }

            // Resetowanie komunikatu walidacyjnego
            validationMessage = "";
        }


        /// <summary>
        /// Rysuje interfejs minigry znajdowania fałszywego e-maila.
        /// </summary>
        private void DrawEmailTask()
        {
            // Sprawdzenie, czy dane są dostępne
            if (emailOptions == null || emailOptions.Count == 0)
            {
                spriteBatch.DrawString(font, "Brak danych do wyświetlenia.",
                    new Vector2(actionFieldBounds.X + 20, actionFieldBounds.Y + 20),
                    Color.Red);
                return;
            }

            // Rysowanie tła pola akcji
            spriteBatch.Draw(CreateTexture(Color.White), actionFieldBounds, Color.DarkSlateGray);

            int yOffset = actionFieldBounds.Y + 20;

            // Rysowanie opcji e-maili
            for (int i = 0; i < emailOptions.Count; i++)
            {
                Rectangle emailRect = emailRectangles[i];
                Color backgroundColor = emailRect.Contains(currentMouseState.Position) ? Color.LightBlue : Color.LightGray;

                spriteBatch.Draw(CreateTexture(backgroundColor), emailRect, Color.White);
                spriteBatch.DrawString(font, emailOptions[i], new Vector2(emailRect.X + 10, emailRect.Y + 10), Color.Black);
            }

            // Wyświetlanie komunikatu o wyniku
            if (isFeedbackActive)
            {
                spriteBatch.DrawString(
                    font,
                    validationMessage,
                    new Vector2(actionFieldBounds.X + 20, actionFieldBounds.Bottom - 50),
                    isAnswerCorrect ? Color.Green : Color.Red
                );
            }
        }


        /// <summary>
        /// Przygotowuje dane i pliki dla minigry "Przeciągnij plik do kwarantanny".
        /// </summary>
        private void SetupDragTask()
        {
            System.Diagnostics.Debug.WriteLine("Rozpoczęto inicjalizację Drag Task.");

            // Lista plików zainfekowanych
            infectedFiles = new List<string>
                {
                    "aktualizacja_patch.exe",
                    "alert_bezpieczenstwa.scr",
                    "dane_ważne.zip",
                    "plik_systemowy.bat",
                    "kopia_dokumentu.js",
                    "tajne.vbs",
                    "prywatne.dll",
                    "album_zdjec.tmp"
                };

            // Lista plików niezainfekowanych
            safeFiles = new List<string>
                {
                    "zdjecia_wakacje.jpg",
                    "prezentacja.pptx",
                    "raport.docx",
                    "arkusz_kalkulacyjny.xlsx",
                    "film.mp4",
                    "muzyka.mp3",
                    "diagram.svg",
                    "notatki.txt",
                    "archiwum.rar",
                    "konfiguracja.ini",
                    "hasla.kdbx",
                    "plan_projektu.pdf",
                    "projekt.grafik",
                    "cv.odt",
                    "plik_log.log"
                };

            // Losowanie jednego pliku zainfekowanego
            string chosenInfectedFile = infectedFiles[random.Next(infectedFiles.Count)];

            // Losowanie ośmiu plików niezainfekowanych
            List<string> chosenSafeFiles = new List<string>();
            while (chosenSafeFiles.Count < 8)
            {
                string safeFile = safeFiles[random.Next(safeFiles.Count)];
                if (!chosenSafeFiles.Contains(safeFile))
                {
                    chosenSafeFiles.Add(safeFile);
                }
            }

            // Połączenie list
            allFiles = new List<string>(chosenSafeFiles);
            infectedFileIndex = random.Next(allFiles.Count); // Losowe miejsce dla pliku zainfekowanego
            allFiles.Insert(infectedFileIndex, chosenInfectedFile);

            System.Diagnostics.Debug.WriteLine($"Inicjalizowane pliki: {string.Join(", ", allFiles)}");

            // Generowanie prostokątów dla plików
            fileRects = new List<Rectangle>();
            int fileWidth = 150;
            int fileHeight = 50;
            int xOffset = actionFieldBounds.X + 20;
            int yOffset = actionFieldBounds.Y + 20;
            int spacing = 20;

            for (int i = 0; i < allFiles.Count; i++)
            {
                fileRects.Add(new Rectangle(
                    xOffset,
                    yOffset,
                    fileWidth,
                    fileHeight
                ));

                xOffset += fileWidth + spacing; // Odstęp między plikami
                if (xOffset + fileWidth > actionFieldBounds.Right)
                {
                    xOffset = actionFieldBounds.X + 20; // Nowy rząd
                    yOffset += fileHeight + spacing;
                }
            }

            // Strefa kwarantanny
            quarantineZone = new Rectangle(
                actionFieldBounds.X + actionFieldBounds.Width / 2 - 75,
                actionFieldBounds.Bottom - 100,
                150,
                50
            );

            draggedFileIndex = null; // Żaden plik nie jest przeciągany
            validationMessage = "";  // Resetuj komunikaty
            isDragTaskComplete = false; // Resetuj stan gry

            System.Diagnostics.Debug.WriteLine("Drag Task został pomyślnie zainicjalizowany.");
        }


        /// <summary>
        /// Rysuje interfejs minigry przeciągania plików.
        /// </summary>
        private void DrawDragTask()
        {
            if (fileRects == null || fileRects.Count == 0 || allFiles == null)
            {
                spriteBatch.DrawString(font, "Brak plików do wyświetlenia.",
                    new Vector2(actionFieldBounds.X + 20, actionFieldBounds.Y + 20),
                    Color.Red);
                return;
            }

            // Rysowanie tła pola akcji
            spriteBatch.Draw(CreateTexture(Color.White), actionFieldBounds, Color.DarkSlateGray);

            // Rysowanie strefy kwarantanny
            spriteBatch.Draw(CreateTexture(Color.Red), quarantineZone, Color.White);
            spriteBatch.DrawString(font, "Kwarantanna",
                new Vector2(quarantineZone.X + 10, quarantineZone.Y + 15),
                Color.Black);

            // Rysowanie plików
            for (int i = 0; i < fileRects.Count; i++)
            {
                Rectangle rect = fileRects[i];
                Vector2 textSize = font.MeasureString(allFiles[i]);
                rect.Width = (int)textSize.X + 20;

                if (draggedFileIndex == i)
                {
                    int clampedX = Math.Clamp(currentMouseState.X - rect.Width / 2, actionFieldBounds.X, actionFieldBounds.Right - rect.Width);
                    int clampedY = Math.Clamp(currentMouseState.Y - rect.Height / 2, actionFieldBounds.Y, actionFieldBounds.Bottom - rect.Height);
                    rect.X = clampedX;
                    rect.Y = clampedY;
                }

                fileRects[i] = rect;

                spriteBatch.Draw(CreateTexture(Color.LightGreen), rect, Color.White);
                spriteBatch.DrawString(font, allFiles[i], new Vector2(rect.X + 10, rect.Y + 10), Color.Black);
            }

            spriteBatch.DrawString(font, validationMessage,
                new Vector2(actionFieldBounds.X + 20, actionFieldBounds.Bottom - 50),
                Color.Green);
        }


        /// <summary>
        /// Inicjalizuje dane dla minigry "Znajdź zainfekowaną stronę".
        /// </summary>
        private void SetupWebsiteTask()
        {
            // Lista stron zainfekowanych
            infectedWebsites = new List<string>
                {
                    "www.go0gle-secure.com",
                    "www.gihub-support.com",
                    "microsfot-update.info",
                    "www.wikipedea-update.net",
                    "stack0verflow-help.org",
                    "reedit-login.com",
                    "insta-gram-verification.info",
                    "faceboook-secure.net",
                    "twittter-login.org",
                    "linkdln-apps.com",
                    "netfliix-billing.info",
                    "twitch-logins.com",
                    "apple-support-help.net",
                    "amazon-prime-login.site",
                    "ebay-giftcard-verify.net"
                };

            // Lista stron niezainfekowanych
            safeWebsites = new List<string>
                {
                    "www.google.com",
                    "www.github.com",
                    "www.microsoft.com",
                    "www.wikipedia.org",
                    "www.stackoverflow.com",
                    "www.reddit.com",
                    "www.instagram.com",
                    "www.facebook.com",
                    "www.twitter.com",
                    "www.linkedin.com",
                    "www.netflix.com",
                    "www.twitch.tv",
                    "www.apple.com",
                    "www.amazon.com",
                    "www.ebay.com"
                };

            // Losowanie jednej strony zainfekowanej
            Random random = new Random();
            string chosenInfectedWebsite = infectedWebsites[random.Next(infectedWebsites.Count)];

            // Losowanie pięciu stron niezainfekowanych
            List<string> chosenSafeWebsites = new List<string>();
            while (chosenSafeWebsites.Count < 5)
            {
                string safeWebsite = safeWebsites[random.Next(safeWebsites.Count)];
                if (!chosenSafeWebsites.Contains(safeWebsite))
                {
                    chosenSafeWebsites.Add(safeWebsite);
                }
            }

            // Połączenie puli stron
            allWebsites = new List<string>(chosenSafeWebsites);
            infectedWebsiteIndex = random.Next(allWebsites.Count); // Losowe miejsce dla strony zainfekowanej
            allWebsites.Insert(infectedWebsiteIndex, chosenInfectedWebsite);

            validationMessage = ""; // Resetuj komunikat
        }


        /// <summary>
        /// Rysuje interfejs minigry znajdowania zainfekowanej strony.
        /// </summary>
        private void DrawWebsiteTask()
        {
            int yOffset = actionFieldBounds.Y + 20;

            // Rysowanie tła pola akcji
            spriteBatch.Draw(CreateTexture(Color.White), actionFieldBounds, Color.DarkSlateGray);

            // Rysowanie opcji stron WWW
            for (int i = 0; i < allWebsites.Count; i++)
            {
                Rectangle websiteRect = new Rectangle(
                    actionFieldBounds.X + 20,
                    yOffset,
                    actionFieldBounds.Width - 40,
                    50
                );

                Color backgroundColor = websiteRect.Contains(currentMouseState.Position) ? Color.LightBlue : Color.LightGray;

                spriteBatch.Draw(CreateTexture(backgroundColor), websiteRect, Color.White);
                spriteBatch.DrawString(font, allWebsites[i], new Vector2(websiteRect.X + 10, websiteRect.Y + 10), Color.Black);

                yOffset += 60; // Odstęp między opcjami
            }

            // Wyświetlanie komunikatu o wyniku, jeśli jest aktywny
            if (isFeedbackActive)
            {
                spriteBatch.DrawString(font, validationMessage, new Vector2(actionFieldBounds.X + 20, actionFieldBounds.Bottom - 50), Color.Green);
            }
        }


        /// <summary>
        /// Obsługuje postęp gracza na kolejne poziomy lub etapy.
        /// </summary>
        /// <param name="isAnswerCorrect"></param>
        private void AdvanceLevelOrStage(bool isAnswerCorrect)
        {
            if (isAnswerCorrect)
            {
                hasFinalChance = true; // Reset flagi dodatkowej szansy przy poprawnej odpowiedzi

                if (currentLevel < 3)
                {
                    currentLevel++; // Przejdź do następnego poziomu
                }
                else
                {
                    badges[currentStage - 1] = true; // Dodaj odznakę za ukończony etap
                    currentLevel = 1;

                    if (currentStage < 3)
                    {
                        currentStage++; // Przejdź do kolejnego etapu
                        SetupStageMiniGames(); // Przygotuj nowe minigry
                    }
                    else
                    {
                        gameState = "VictoryScreen"; // Gracz wygrał grę
                        return;
                    }
                }

                UpdateMapTexture();
            }
            else
            {
                // Obsługa poziomu trudnego
                if (difficultyLevel == "Hard")
                {
                    gameState = "GameOverScreen";
                    return;
                }

                // Odejmij życie
                remainingLives--;
                System.Diagnostics.Debug.WriteLine($"Błąd! Pozostałe życia: {remainingLives}");

                if (remainingLives <= 0)
                {
                    if (hasFinalChance)
                    {
                        hasFinalChance = false; // Zużyj dodatkową szansę
                        validationMessage = "Ostatnia szansa! Uważaj!";
                        SelectRandomMiniGame(); // Losuj nową minigrę
                        return;
                    }

                    gameState = "GameOverScreen"; // Gracz przegrywa
                    return;
                }

                validationMessage = "Błąd! Spróbuj ponownie.";
            }

            SelectRandomMiniGame(); // Losuj nową minigrę

        }


        /// <summary>
        /// Resetuje postęp gry i wszystkie kluczowe zmienne.
        /// </summary>
        private void ResetGameProgress()
        {
            currentStage = 1;
            currentLevel = 1;
            remainingLives = 0;
            badges = new List<bool> { false, false, false };
            hasFinalChance = true;
            validationMessage = "";
            currentMiniGame = "";
            lastMiniGame = "";
            stageMiniGames = new List<string>(miniGames); // Reset listy minigier
            UpdateMapTexture(); // Reset mapy do początkowej
            System.Diagnostics.Debug.WriteLine("Gra została zresetowana. Wszystkie zmienne startowe ustawione.");
        }


        /// <summary>
        /// Rysuje ekran zwycięstwa.
        /// </summary>
        private void DrawVictoryScreen()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Rysowanie tła
            spriteBatch.Draw(victoryBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

            // Obsługa przycisków
            DrawEndScreenButtons();
        }


        /// <summary>
        /// Rysuje ekran przegranej.
        /// </summary>
        private void DrawGameOverScreen()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // Rysowanie tła
            spriteBatch.Draw(gameOverBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            DrawEndScreenButtons();
        }


        /// <summary>
        /// Rysuje przyciski na ekranie końcowym gry.
        /// </summary>
        private void DrawEndScreenButtons()
        {
            // Przycisk powrotu do menu
            Rectangle menuButton = new Rectangle(GraphicsDevice.Viewport.Width / 2 - 100, GraphicsDevice.Viewport.Height / 2, 200, 50);
            spriteBatch.Draw(buttonTexture, menuButton, Color.Gray);
            spriteBatch.DrawString(font, "Powrót do menu", new Vector2(menuButton.X + 20, menuButton.Y + 15), Color.Black);

            // Przycisk wyjścia z gry
            Rectangle exitButton = new Rectangle(GraphicsDevice.Viewport.Width / 2 - 100, GraphicsDevice.Viewport.Height / 2 + 70, 200, 50);
            spriteBatch.Draw(buttonTexture, exitButton, Color.Gray);
            spriteBatch.DrawString(font, "Wyjście z gry", new Vector2(exitButton.X + 40, exitButton.Y + 15), Color.Black);

            // Obsługa kliknięcia na przyciski
            if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (menuButton.Contains(currentMouseState.Position))
                {
                    ResetGameProgress();   // Resetuj postęp gry
                    gameState = "MainMenu"; // Powrót do menu głównego
                }
                else if (exitButton.Contains(currentMouseState.Position))
                {
                    Exit(); // Wyjście z gry
                }
            }
        }


        /// <summary>
        /// Dopasowuje tekst do maksymalnej szerokości, dzieląc go na linie.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="maxLineWidth"></param>
        /// <returns></returns>
        private string WrapText(SpriteFont font, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            string wrappedText = "";
            string line = "";

            foreach (string word in words)
            {
                string testLine = string.IsNullOrEmpty(line) ? word : line + " " + word;

                // Sprawdzenie szerokości testowej linii
                if (font.MeasureString(testLine).X > maxLineWidth)
                {
                    // Jeśli szerokość przekracza maksymalną, przenosimy tekst do nowej linii
                    wrappedText += line + "\n";
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }


            if (!string.IsNullOrEmpty(line))
            {
                wrappedText += line;
            }

            return wrappedText;
        }


        /// <summary>
        /// Tworzy teksturę o podanym kolorze.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private Texture2D CreateTexture(Color color)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { color });
            return texture;
        }


        /// <summary>
        /// Metoda dodatkowa do obsługi wpisywania
        /// </summary>
        /// <param name="key"></param>
        /// <param name="shiftPressed"></param>
        /// <returns></returns>
        private string FilterInputKey(Keys key, bool shiftPressed)
        {
            // Mapowanie klawiszy z uwzględnieniem Shift
            if (key >= Keys.A && key <= Keys.Z)
                return shiftPressed ? key.ToString().ToUpper() : key.ToString().ToLower();
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                // Obsługa cyfr i znaków specjalnych z Shift
                if (shiftPressed)
                {
                    switch (key)
                    {
                        case Keys.D1: return "!";
                        case Keys.D2: return "@";
                        case Keys.D3: return "#";
                        case Keys.D4: return "$";
                        case Keys.D5: return "%";
                        case Keys.D6: return "^";
                        case Keys.D7: return "&";
                        case Keys.D8: return "*";
                        case Keys.D9: return "(";
                        case Keys.D0: return ")";
                        default: return ""; // Ignoruj pozostałe
                    }
                }
                else
                {
                    return key.ToString().Substring(1); // Cyfry bez Shift
                }
            }

            if (key == Keys.Space) return " ";

            // Obsługa innych znaków specjalnych
            switch (key)
            {
                case Keys.OemPeriod: return shiftPressed ? ">" : ".";
                case Keys.OemComma: return shiftPressed ? "<" : ",";
                case Keys.OemMinus: return shiftPressed ? "_" : "-";
                case Keys.OemPlus: return shiftPressed ? "+" : "=";
                case Keys.OemQuestion: return shiftPressed ? "?" : "/";
                case Keys.OemTilde: return shiftPressed ? "~" : "`";
                case Keys.OemOpenBrackets: return shiftPressed ? "{" : "[";
                case Keys.OemCloseBrackets: return shiftPressed ? "}" : "]";
                case Keys.OemSemicolon: return shiftPressed ? ":" : ";";
                case Keys.OemQuotes: return shiftPressed ? "\"" : "'";
                default: return ""; // Ignoruj pozostałe
            }
        }


        /// <summary>
        /// Metoda dodatkowa do obsługi wpisywania
        /// </summary>
        private void ResetTextInput()
        {
            inputText = "";
            validationMessage = "";
        }

    }

}
